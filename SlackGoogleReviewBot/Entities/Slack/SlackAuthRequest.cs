namespace SlackGoogleReviewBot.Entities.Slack
{
    public class SlackAuthRequest
    {
        public string ClientId { get; set; }

        public string RedirectUri { get; set; }

        public List<string> Scopes { get; set; } = new List<string>();

        public Dictionary<string, string?> GetRequestData()
        {
            return new Dictionary<string, string?>
            {
                {"client_id", ClientId},
                {"redirect_uri", RedirectUri},
                {"scope", string.Join(",", Scopes)},
                {"granular_bot_scope", "1" }
            };
        }
    }
}
