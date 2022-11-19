using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlackGoogleReviewBot.Entities;

namespace SlackGoogleReviewBot.Database.Configurators
{
    public class SlackUserTokenConfigurator : IEntityTypeConfiguration<SlackUserToken>
    {
        public void Configure(EntityTypeBuilder<SlackUserToken> builder)
        {
            builder.HasKey(r => r.SlackUserId);
            builder.Property(r => r.SlackUserId).HasColumnName("slack_user_id");
            builder.Property(r => r.AccessToken).HasColumnName("access_token");
            builder.Property(r => r.ChannelId).HasColumnName("channel_id");
            builder.Property(r => r.TeamId).HasColumnName("team_id");

            builder.ToTable("slack_user_tokens");
        }
    }
}
