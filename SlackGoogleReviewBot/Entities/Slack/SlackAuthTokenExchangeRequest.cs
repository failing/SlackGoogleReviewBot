namespace SlackGoogleReviewBot.Entities.Slack
{
    public class SlackAuthTokenExchangeRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string Code { get; set; }

        public Dictionary<string, object> GetRequestData()
        {
            return new Dictionary<string, object>
            {
                {"code", Code },
                {"client_id", ClientId},
                {"client_secret", ClientSecret},
                {"redirect_uri", RedirectUri},
            };
        }
    }
}
