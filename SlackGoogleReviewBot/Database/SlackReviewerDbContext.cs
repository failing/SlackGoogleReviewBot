using Microsoft.EntityFrameworkCore;
using SlackGoogleReviewBot.Entities;
using SlackGoogleReviewBot.Database.Configurators;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace SlackGoogleReviewBot.Database
{
    public class SlackReviewerDbContext : DbContext
    {
        public SlackReviewerDbContext()
        {

        }

        public SlackReviewerDbContext(DbContextOptions<SlackReviewerDbContext> options) : base(options)
        {

        }

        public static int? SoundsLike(string input)
        {
            throw new NotImplementedException();
        }

        public virtual DbSet<GooglePlaceReview> GooglePlaceReviews { get; set; }
        public virtual DbSet<SlackBusinessSubscription> SlackBusinessSubscriptions { get; set; }
        public virtual DbSet<SlackUserToken> SlackUserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new GooglePlaceReviewConfigurator());
            modelBuilder.ApplyConfiguration(new SlackBusinessSubscriptionConfigurator());
            modelBuilder.ApplyConfiguration(new SlackUserTokenConfigurator());

            modelBuilder
                .HasDbFunction(typeof(SlackReviewerDbContext).GetMethod(nameof(SlackReviewerDbContext.SoundsLike)))
                .HasTranslation(args => SqlFunctionExpression.Create("SOUNDEX", args, typeof(int?), null));
        }
    }
}
