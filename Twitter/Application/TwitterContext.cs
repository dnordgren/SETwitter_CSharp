using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Twitter_Shared.Data.Model;

namespace Twitter.Application
{
    public class TwitterContext : DbContext
    {

        public TwitterContext()
            : base("TwitterConnection")
        {

        }

        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Feed> Feeds { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
                .HasMany<Feed>(u => u.Feeds)
                .WithRequired(f => f.Owner)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Feed>()
                .HasMany<Tweet>(f => f.Tweets).WithRequired(t => t.BelongsTo).WillCascadeOnDelete(false);

            modelBuilder.Entity<Tweet>()
                .HasRequired<Feed>(t => t.BelongsTo)
                .WithMany(f => f.Tweets)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tweet>().HasMany<Tweet>(t => t.RelatedTweets).WithMany(t => t.RelatedTo).Map(m =>
            {
                m.MapLeftKey("TweetId");
                m.MapRightKey("RelatedId");
                m.ToTable("RelatedTweets");
            });

            modelBuilder.Entity<Feed>().HasMany<User>(f => f.Subscribers).WithMany(u => u.Subscriptions).Map(m =>
            {
                m.MapLeftKey("FeedId");
                m.MapRightKey("SubscriberId");
                m.ToTable("FeedSubscriptions");
            });
        }
    }

}