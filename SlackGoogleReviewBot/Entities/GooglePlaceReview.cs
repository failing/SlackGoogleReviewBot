namespace SlackGoogleReviewBot.Entities
{
    public class GooglePlaceReview
    {
        public GooglePlaceReview() => CancellationToken = cts.Token;
        public Guid Id { get; set; }
        public string PlaceId { get; set; }
        public DateTimeOffset? LatestReviewTime { get; set; }
        public Task RunningTask { get; set; }
        public CancellationToken CancellationToken { get; set; }
        private CancellationTokenSource cts = new CancellationTokenSource();

        public void CancelJob()
        {
            cts.Cancel();
        }
    }
}
