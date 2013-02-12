using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twitter.Application;
using Twitter.Models.Home;
using Twitter_Shared.Data.Model;

namespace Twitter.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            HomeViewModel model = GenerateIndexModel();
            return View(model);
        }

        [Authorize]
        [HttpPost, ActionName("Tweet")]
        public PartialViewResult Tweet(HomeViewModel model)
        {
            using (TwitterContext ctx = new TwitterContext())
            {
                Tweet tweet = new Tweet()
                {
                    BelongsTo = ctx.Feeds.Find(Convert.ToInt64(model.SelectedFeed)), 
                    Content = model.Tweet, 
                    PostDate = DateTime.Now
                };
                ctx.Tweets.Add(tweet);
                ctx.SaveChanges();
            }

            HomeViewModel updatedMode = GenerateIndexModel();
            updatedMode.Tweet = "";
            updatedMode.SelectedFeed = model.SelectedFeed;
            ModelState.Clear();
            return PartialView("_TweetView", updatedMode);
        }

        [Authorize]
        [HttpPost]
        public PartialViewResult CreateFeed(HomeViewModel model)
        {
            using (TwitterContext ctx = new TwitterContext())
            {
                // get a user that matches the name of the currently logged in user
                User user = ctx.Users.First(u => u.Email == User.Identity.Name);
                Feed feed = new Feed()
                {
                    Name = model.NewFeedName,
                    Owner = user
                };
                ctx.Feeds.Add(feed);
                ctx.SaveChanges();
                model.Feeds = ctx.Entry<User>(user).Entity.Feeds;
            }

            model.NewFeedName = "";
            ModelState.Clear();
            return PartialView("_FeedView", model);
        }

        [Authorize]
        [HttpPost]
        public PartialViewResult SubscribeToFeed(string feedQuery)
        {
            HomeViewModel model = new HomeViewModel();

            using (TwitterContext ctx = new TwitterContext())
            {
                User user = ctx.Users.First(u => u.Email == User.Identity.Name);

                model.SubscriptionList = ctx.Feeds.Where(f => f.Owner.ID != user.ID).
                    Select(f => f.Name + " by " + f.Owner.Name).ToArray<string>();
            }

            return PartialView("_SubscriptionView", model);
        }

        private HomeViewModel GenerateIndexModel()
        {
            HomeViewModel model = new HomeViewModel();
            using (TwitterContext ctx = new TwitterContext())
            {
                // get a user that matches the name of the currently logged in user
                User user = ctx.Users.First(u => u.Email == User.Identity.Name);

                /* Find all of the tweets that should be displayed. The tweets that 
                 * should be displayed are either authored by the user or those that 
                 * are found in a user subscription */
                List<Tweet> tweetsToDisplay = new List<Tweet>();

                if (user.Feeds != null)
                {
                    foreach (Feed feed in user.Feeds)
                    {
                        if (feed.Tweets != null && feed.Tweets.Count > 0)
                        {
                            tweetsToDisplay.AddRange(feed.Tweets);
                        }
                    }
                }

                if (user.Subscriptions != null)
                {
                    foreach (Feed feed in user.Subscriptions)
                    {
                        if (feed.Tweets != null && feed.Tweets.Count > 0)
                        {
                            tweetsToDisplay.AddRange(feed.Tweets);
                        }
                    }
                }

                model.Tweets = tweetsToDisplay.OrderByDescending(t => t.PostDate).ToList<Tweet>();

                if (user.Feeds != null && user.Feeds.Count > 0)
                {
                    model.HasFeeds = true;
                }
                model.DisplayFeed = -1;

                // Add the user's feeds to the feed model that will be rendered as a partial view
                model.Feeds = user.Feeds == null ? new List<Feed>() : user.Feeds;

                model.SubscriptionList = ctx.Feeds.Where(f => f.Owner.ID != user.ID).
                    Select(f => f.Name +" by "+f.Owner.Name).ToArray<string>();
            }
            model.FeedList = model.Feeds.Select(f => new SelectListItem() { Text = f.Name, Value = Convert.ToString(f.ID) });

            return model;
        }
    }
}
