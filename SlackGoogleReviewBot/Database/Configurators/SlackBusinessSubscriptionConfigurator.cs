using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlackGoogleReviewBot.Entities;

namespace SlackGoogleReviewBot.Database.Configurators
{
    public class SlackBusinessSubscriptionConfigurator : IEntityTypeConfiguration<SlackBusinessSubscription>
    {
        public void Configure(EntityTypeBuilder<SlackBusinessSubscription> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd().HasColumnName("id");
            builder.Property(x => x.SlackUserId).HasColumnName("slack_user_id");
            builder.Property(x => x.PlaceId).HasColumnName("place_id");
            builder.Property(r => r.TeamId).HasColumnName("team_id");

            builder.ToTable("slack_review_subscriptions");
        }
    }
}
