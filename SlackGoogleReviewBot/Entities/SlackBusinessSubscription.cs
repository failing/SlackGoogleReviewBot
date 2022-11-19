namespace SlackGoogleReviewBot.Entities
{
    public class SlackBusinessSubscription
    {
        public int Id { get; set; }
        public string SlackUserId { get; set; }
        public string PlaceId { get; set; }
        public string TeamId { get; set; }
    }
}
