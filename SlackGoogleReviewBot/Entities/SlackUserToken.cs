namespace SlackGoogleReviewBot.Entities
{
    public class SlackUserToken
    {
        public string SlackUserId { get; set; }

        public string AccessToken { get; set; }

        public string ChannelId { get; set; }

        public string TeamId { get; set; }
    }
}
