using Microsoft.EntityFrameworkCore;
using SlackGoogleReviewBot.Database;
using SlackGoogleReviewBot.Entities;
using SlackGoogleReviewBot.Services.Interfaces;

namespace SlackGoogleReviewBot.Services
{
    public class SlackService : ISlackService
    {
        private readonly SlackReviewerDbContext _context;

        public SlackService(SlackReviewerDbContext context)
        {
            _context = context;
        }

        public async Task<SlackUserToken> GetSlackUserToken(string userId, string? teamId = null)
        {
            return await _context.SlackUserTokens.Where(r => r.SlackUserId == userId && string.IsNullOrEmpty(teamId) ? true : r.TeamId == teamId).FirstOrDefaultAsync();
        }

        public async Task<SlackUserToken> CreateOrUpdateSlackUserToken(SlackUserToken userAuth)
        {
            SlackUserToken existing = await GetSlackUserToken(userAuth.SlackUserId, userAuth.TeamId);

            if (existing != null)
            {
                existing.AccessToken = userAuth.AccessToken;
                existing.ChannelId = userAuth.ChannelId;
                existing.TeamId = userAuth.TeamId;

                _context.SlackUserTokens.Update(existing);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.SlackUserTokens.Add(userAuth);
                await _context.SaveChangesAsync();
            }

            return userAuth;
        }

        public async Task<GooglePlaceReview> GetGoogleBussinessPlaceItem(string placeId)
        {
            return await _context.GooglePlaceReviews.FirstOrDefaultAsync(r => r.PlaceId == placeId);
        }

        public async Task<GooglePlaceReview> AddGoogleBusiness(GooglePlaceReview review)
        {
            GooglePlaceReview? existingPlace = await GetGoogleBussinessPlaceItem(review.PlaceId);

            if (existingPlace == null)
            {
                _context.GooglePlaceReviews.Add(review);
                await _context.SaveChangesAsync();

                return review;
            }

            return null;
        }

        public async Task<GooglePlaceReview> UpdateGoogleReview(GooglePlaceReview existingReview)
        {
            var existingPlace = await GetGoogleBussinessPlaceItem(existingReview.PlaceId);

            if (existingPlace != null)
            {
                existingPlace.PlaceId = existingReview.PlaceId;
                existingPlace.LatestReviewTime = existingReview.LatestReviewTime;

                _context.GooglePlaceReviews.Update(existingReview);
                await _context.SaveChangesAsync();

                return existingReview;
            }

            return null;
        }

        public async Task<SlackBusinessSubscription> GetSubscription(string userId, string teamId, string placeId)
        {
            return await _context.SlackBusinessSubscriptions.Where(r => r.SlackUserId == userId && r.PlaceId == placeId && r.TeamId == teamId).FirstOrDefaultAsync();
        }

        public async Task<List<SlackBusinessSubscription>> ListSubscriptions(string? placeId = null, string? teamId = null)
        {
            return await _context.SlackBusinessSubscriptions.Where(r => string.IsNullOrEmpty(placeId) ? true : r.PlaceId == placeId && string.IsNullOrEmpty(teamId) ? true : r.TeamId == teamId).ToListAsync();
        }

        public async Task<SlackBusinessSubscription> CreateNewSubscription(string userId, string teamId, string placeId)
        {
            SlackBusinessSubscription slackReviewUser = new SlackBusinessSubscription()
            {
                PlaceId = placeId,
                SlackUserId = userId,
                TeamId = teamId
            };

            _context.SlackBusinessSubscriptions.Add(slackReviewUser);
            await _context.SaveChangesAsync();

            return slackReviewUser;
        }

        public async Task DeleteSlackReviewerUser(string userId, string teamId, string placeId)
        {
            SlackBusinessSubscription subscription = await GetSubscription(userId, teamId, placeId);

            if (subscription != null)
            {
                _context.SlackBusinessSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }
    }
}
