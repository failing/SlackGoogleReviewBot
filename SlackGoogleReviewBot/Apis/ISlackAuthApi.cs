using RestEase;
using SlackGoogleReviewBot.Entities.Slack;

namespace SlackGoogleReviewBot.Apis
{
    public interface ISlackAuthApi
    {
        [Post("api/oauth.v2.access")]
        [Header("Content-Type", "application/x-www-form-urlencoded")]
        Task<SlackTokenResponse> ExchangeAuthorizationTokenForAccessToken([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data);
    }
}
