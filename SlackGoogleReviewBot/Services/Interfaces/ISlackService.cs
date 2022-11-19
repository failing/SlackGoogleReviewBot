using SlackGoogleReviewBot.Entities;

namespace SlackGoogleReviewBot.Services.Interfaces
{
    public interface ISlackService
    {
        Task<GooglePlaceReview> GetGoogleBussinessPlaceItem(string placeId);
        Task DeleteSlackReviewerUser(string userId, string teamId, string placeId);
        Task<SlackBusinessSubscription> GetSubscription(string userId, string teamId, string placeId);
        Task<List<SlackBusinessSubscription>> ListSubscriptions(string? placeId = null, string? teamId = null);
        Task<GooglePlaceReview> AddGoogleBusiness(GooglePlaceReview review);
        Task<SlackUserToken> GetSlackUserToken(string userId, string? teamId = null);
        Task<SlackBusinessSubscription> CreateNewSubscription(string userId, string teamId, string placeId);
        Task<GooglePlaceReview> UpdateGoogleReview(GooglePlaceReview existingReview);
        Task<SlackUserToken> CreateOrUpdateSlackUserToken(SlackUserToken userAuth);
    }
}