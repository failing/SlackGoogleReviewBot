using Newtonsoft.Json;

namespace SlackGoogleReviewBot.Entities.Auth
{
    public class SlackSecrets
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SigningSecret { get; set; }
        public string VerificationToken { get; set; }
        public string RedirectUri { get; set; }
        public List<string> Scopes { get; set; } = new List<string>();
        public string AuthUrl { get; set; }
        public string TokenEndpoint { get; set; }
        public string AppToken { get; set; }
    }
}
