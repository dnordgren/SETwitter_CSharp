﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twitter.Models.Account;
using Twitter_Shared.Data.Model;

namespace Twitter.Models.Home
{
    public class HomeViewModel
    {
        [StringLength(140)]
        public string Tweet { get; set; }

        public ICollection<Tweet> Tweets { get; set; }

        public bool HasFeeds { get; set; }
        public string SelectedFeed { get; set; }

        public long DisplayFeed { get; set; }

        public string NewFeedName { get; set; }
        public ICollection<Feed> Feeds { get; set; }
        public ICollection<Feed> Subscriptions { get; set; }

        public IEnumerable<SelectListItem> FeedList { get; set; }

        public string[] SubscriptionList { get; set; }
        public long ToSubscribe { get; set; }

        // I'll convert this to Json and use it to match on subscription autocomplete events
        public IDictionary<string, long> SubscriptionLookup { get; set; }
    }
}