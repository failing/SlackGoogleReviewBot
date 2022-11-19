using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlackGoogleReviewBot.Entities;

namespace SlackGoogleReviewBot.Database.Configurators
{
    public class GooglePlaceReviewConfigurator : IEntityTypeConfiguration<GooglePlaceReview>
    {
        public void Configure(EntityTypeBuilder<GooglePlaceReview> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).HasColumnName("id");
            builder.Property(r => r.PlaceId).HasColumnName("location_name");
            builder.Property(r => r.LatestReviewTime).HasColumnName("latest_review_ts");
            builder.Ignore(r => r.RunningTask);
            builder.Ignore(r => r.CancellationToken);

            builder.ToTable("slack_place_reviews");
        }
    }
}
