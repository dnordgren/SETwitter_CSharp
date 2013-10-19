using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Twitter.Application;
using Twitter.Models.Home;
using Twitter_Shared.Data;
using Twitter_Shared.Data.Model;
using Twitter_Shared.Service;

namespace Twitter.Controllers
{
    public class HomeController : Controller
    {
        private IUnitOfWork _unit;
        private ITwitterService _twitterService;
        private IUserService _userService;

        public HomeController(IUnitOfWork unit, ITwitterService twitterService, IUserService userService)
        {
            _unit = unit;
            _twitterService = twitterService;
            _userService = userService;
        }

        [Authorize]
        public ActionResult Index()
        {
            HomeViewModel model = GenerateIndexModel(-1);
            return View(model);
        }

        [Authorize]
        [HttpPost, ActionName("Tweet")]
        public PartialViewResult Tweet(HomeViewModel model)
        {
            Tweet tweet = new Tweet()
            {
                BelongsTo = _twitterService.GetFeed(Convert.ToInt64(model.SelectedFeed)),
                Content = model.Tweet,
                PostDate = DateTime.Now
            };
            _twitterService.AddTweet(tweet);
            _unit.Commit();

            HomeViewModel updatedMode = GenerateIndexModel(model.DisplayFeed);
            updatedMode.Tweet = "";
            updatedMode.SelectedFeed = model.SelectedFeed;
            ModelState.Clear();
            return PartialView("_TweetView", updatedMode);
        }

        [Authorize]
        [HttpPost]
        public PartialViewResult CreateFeed(HomeViewModel model)
        {
            // get a user that matches the name of the currently logged in user
            User user = _userService.FindUserForName(User.Identity.Name);
            Feed feed = new Feed()
            {
                Name = model.NewFeedName,
                Owner = user
            };
            model.Feeds = _twitterService.AddFeed(feed);
            _unit.Commit();

            model.NewFeedName = "";
            ModelState.Clear();
            return PartialView("_FeedView", model);
        }

        [Authorize]
        [HttpPost]
        public PartialViewResult SubscribeToFeed(long subscribeTo)
        {
            if (subscribeTo != -1)
            {
                User user = _userService.FindUserForName(User.Identity.Name);
                Feed feed = _twitterService.GetFeed(subscribeTo);
                _twitterService.SubscribeToFeed(user, feed);

                _unit.Commit();
            }

            HomeViewModel model = GenerateIndexModel(-1);
            return PartialView("_SubscriptionView", model);
        }

        [Authorize]
        [HttpGet, ActionName("FilterFeed")]
        public PartialViewResult FilterByFeed(long feedId)
        {
            HomeViewModel model = GenerateIndexModel(feedId);
            if (feedId == -1)
            {
                // the default feed (i.e. home state) should be displayed
                return PartialView("_TweetView", model);
            }
            else
            {
                /* I'm only care about part of model needed for this
                 * partial view.  This is an example of why I should actually
                 * break the single view model into its own model for each
                 * partial view on this page.
                 */
                Feed feed = _twitterService.GetFeed(feedId);

                model.Tweets = (feed == null) ? new List<Tweet>() : feed.Tweets.OrderByDescending(t => t.PostDate).ToList<Tweet>();
                model.DisplayFeed = feedId;
                
                return PartialView("_TweetView", model);
            }
        }

        [Authorize]
        [HttpPost, ActionName("Search")]
        public PartialViewResult Search(string searchQuery)
        {
            HomeViewModel model = GenerateIndexModel(-1);
            model.Tweets = _twitterService.Search(searchQuery);
            model.DisplayFeed = -1;

            return PartialView("_TweetView", model);
        }

        [Authorize]
        [HttpGet, ActionName("LogOFf")]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        /* this entire process of setting up the models should be refactored 
         * in terms of partitioning the models and partial views */
        private HomeViewModel GenerateIndexModel(long displayFeed)
        {
            HomeViewModel model = new HomeViewModel();

                // get a user that matches the name of the currently logged in user
                User user = _userService.FindUserForName(User.Identity.Name);

                /* Find all of the tweets that should be displayed. The tweets that 
                 * should be displayed are either authored by the user or those that 
                 * are found in a user subscription */
                List<Tweet> tweetsToDisplay = new List<Tweet>();

                if (displayFeed == -1)
                {
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
                }
                else
                {
                    Feed feed = _twitterService.GetFeed(displayFeed);
                    tweetsToDisplay = (feed == null) ? new List<Tweet>() : feed.Tweets.ToList<Tweet>();
                }

                model.Tweets = tweetsToDisplay.OrderByDescending(t => t.PostDate).ToList<Tweet>();

                if (user.Feeds != null && user.Feeds.Count > 0)
                {
                    model.HasFeeds = true;
                }
                model.DisplayFeed = -1;

                // Add the user's feeds to the feed model that will be rendered as a partial view
                model.Feeds = user.Feeds == null ? new List<Feed>() : user.Feeds;
                model.Subscriptions = user.Subscriptions == null ? new List<Feed>() : user.Subscriptions;

                List<string> subscriptionStrings = new List<string>();
                model.SubscriptionLookup = new Dictionary<string, long>();

                List<Feed> possible = _twitterService.GetPossibleSubscriptionsFor(user);
                foreach (Feed feed in possible)
                {
                    string subscription = string.Format("{0} by {1}", feed.Name, feed.Owner.Name);
                    subscriptionStrings.Add(subscription);
                    model.SubscriptionLookup.Add(subscription, feed.ID);
                }

                model.SubscriptionList = subscriptionStrings.ToArray<string>();
            
            model.FeedList = model.Feeds.Select(f => new SelectListItem() { Text = f.Name, Value = Convert.ToString(f.ID) });

            return model;
        }
    }
}
