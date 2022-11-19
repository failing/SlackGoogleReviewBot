using Microsoft.EntityFrameworkCore;
using SlackGoogleReviewBot.Database;
using SlackGoogleReviewBot.Entities;
using SlackGoogleReviewBot.Entities.Google;
using SlackGoogleReviewBot.Services.Interfaces;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.WebApi;

namespace SlackGoogleReviewBot.Services
{
    public class SlackJobService : ISlackJobService
    {
        private readonly IServiceScopeFactory _service;
        private List<GooglePlaceReview> _runningJobs = new List<GooglePlaceReview>();

        public SlackJobService(IServiceScopeFactory service)
        {
            _service = service;
        }

        public async Task DeleteJob(string userId, string teamId, string placeId)
        {
            using var scope = _service.CreateScope();
            ISlackService _slackService = scope.ServiceProvider.GetService<ISlackService>()!;
            await _slackService.DeleteSlackReviewerUser(userId, teamId, placeId);

            if (!(await _slackService.ListSubscriptions(placeId)).Any())
            {
                GooglePlaceReview job = _runningJobs.First(r => r.PlaceId == placeId);
                job.CancelJob();
                _runningJobs = _runningJobs.Where(r => r.PlaceId == placeId).ToList();
            }
        }

        public async Task DeleteJobFromTeamId(string teamId)
        {
            using var scope = _service.CreateScope();
            SlackReviewerDbContext _dbContext = scope.ServiceProvider.GetService<SlackReviewerDbContext>()!;
            ISlackService _slackService = scope.ServiceProvider.GetService<ISlackService>()!;

            List<SlackBusinessSubscription> subscriptions = await _dbContext.SlackBusinessSubscriptions.Where(r => r.TeamId == teamId).ToListAsync();
            List<string> places = subscriptions.Select(r => r.PlaceId).Distinct().ToList();

            _dbContext.SlackBusinessSubscriptions.RemoveRange(subscriptions);
            await _dbContext.SaveChangesAsync();

            foreach (var placeId in places)
            {
                if (!(await _slackService.ListSubscriptions(placeId)).Any())
                {
                    GooglePlaceReview job = _runningJobs.First(r => r.PlaceId == placeId);
                    job.CancelJob();
                    _runningJobs = _runningJobs.Where(r => r.PlaceId == placeId).ToList();
                }
            }
        }

        public async Task CreateJob(string userId, string teamId, string placeId)
        {
            using var scope = _service.CreateScope();
            ISlackService _slackService = scope.ServiceProvider.GetService<ISlackService>()!;
            IGooglePlaceService _googlePlaceService = scope.ServiceProvider.GetService<IGooglePlaceService>()!;

            GooglePlaceReview existingJob = await _slackService.GetGoogleBussinessPlaceItem(placeId);
            SlackBusinessSubscription existingReviewer = await _slackService.GetSubscription(userId, teamId, placeId);

            if (existingJob == null)
            {
                GooglePlaceDetail? existingPlaceDetail = await _googlePlaceService.GetPlaceDetails(placeId);

                if (existingPlaceDetail?.Result != null)
                {
                    var newSlackReview = new GooglePlaceReview()
                    {
                        PlaceId = placeId,
                    };

                    await _slackService.AddGoogleBusiness(newSlackReview);
                    await _slackService.CreateNewSubscription(userId, teamId, placeId);

                    newSlackReview.RunningTask = ConstructNewSchedule(placeId, newSlackReview.CancellationToken);
                    _runningJobs.Add(newSlackReview);
                }
            }
            else if (existingReviewer == null)
            {
                await _slackService.CreateNewSubscription(userId, teamId, placeId);
            }
        }

        public async Task RemakeSchedules()
        {
            foreach (var existingJob in _runningJobs)
            {
                existingJob.CancelJob();
            }

            _runningJobs.Clear();

            using var scope = _service.CreateScope();
            ISlackService slack = scope.ServiceProvider.GetService<ISlackService>()!;

            List<string> jobsToCreate = (await slack.ListSubscriptions()).GroupBy(p => p.PlaceId)
                                                                              .Select(g => g.First())
                                                                              .ToList().Select(r => r.PlaceId).ToList();

            foreach (string placeId in jobsToCreate)
            {
                GooglePlaceReview? job = await slack.GetGoogleBussinessPlaceItem(placeId);

                if (job != null)
                {
                    job.RunningTask = ConstructNewSchedule(placeId, job.CancellationToken);
                    _runningJobs.Add(job);
                }
            }
        }

        private static Message GetReviewMessage(string channelId, string placeId, string placeName, Review review)
        {
            string directLink = review.AuthorUrl.ToString().Replace("reviews", "place/");
            directLink += placeId;

            string? updatedProfileImage = review.ProfilePhotoUrl?.ToString()?.Split("=")[0];

            if (updatedProfileImage != null)
            {
                updatedProfileImage += "=s256";
            }

            return new Message()
            {
                Channel = channelId,
                Blocks = new List<Block>()
                {
                    new SectionBlock()
                    {
                       Accessory = new Image()
                       {
                            ImageUrl = updatedProfileImage,
                            AltText = "Profile Picture Of Reviewer"
                       },
                       Text = new Markdown($"*{placeName}* - {review.AuthorName}\n\n{SlackUiBlockHelp.GetStartCount(review.Rating)}\n\n>{review.Text}")
                    },
                    new ActionsBlock()
                    {
                        Elements = new List<IActionElement>()
                        {
                            new Button()
                            {
                                Url = directLink,
                                Text = "Go To Review"
                            },
                        }
                    }
                }
            };
        }

        private async Task NotifiyPeople(string placeId, string placeName, List<Review> reviews)
        {
            using var scope = _service.CreateScope();

            ISlackService slack = scope.ServiceProvider.GetService<ISlackService>()!;
            ISlackApiClient slackApi = scope.ServiceProvider.GetService<ISlackApiClient>()!;

            List<SlackBusinessSubscription> usersSubscribed = await slack.ListSubscriptions(placeId);

            foreach (var user in usersSubscribed)
            {
                SlackUserToken userAuth = await slack.GetSlackUserToken(user.SlackUserId, user.TeamId);

                foreach (var review in reviews)
                {
                    await slackApi.WithAccessToken(userAuth.AccessToken).Chat.PostMessage(GetReviewMessage(userAuth.ChannelId, placeId, placeName, review));
                }
            }
        }

        private Task ConstructNewSchedule(string placeId, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                using var scope = _service.CreateScope();
                ISlackService _slack = scope.ServiceProvider.GetService<ISlackService>()!;
                IGooglePlaceService _googlePlaceService = scope.ServiceProvider.GetService<IGooglePlaceService>()!;

                while (true)
                {
                    GooglePlaceDetail? existingPlaceDetail = await _googlePlaceService.GetPlaceDetails(placeId);

                    if (existingPlaceDetail == null)
                    {
                        return;
                    }

                    GooglePlaceReview lastKnownReview = await _slack.GetGoogleBussinessPlaceItem(placeId);
                    List<Review> latestReviews = existingPlaceDetail.Result.Reviews.OrderByDescending(r => r.Time).ToList();

                    if (lastKnownReview.LatestReviewTime.HasValue)
                    {
                        latestReviews = latestReviews.Where(r => r.Time > lastKnownReview.LatestReviewTime.Value.ToUnixTimeMilliseconds() / 1000).ToList();

                        if (latestReviews.Any())
                        {
                            lastKnownReview.LatestReviewTime = DateTimeOffset.FromUnixTimeMilliseconds(latestReviews.First().Time * 1000);
                            await _slack.UpdateGoogleReview(lastKnownReview);

                            await NotifiyPeople(placeId, existingPlaceDetail.Result.Name, latestReviews);
                        }
                    }
                    else
                    {
                        latestReviews = latestReviews.GetRange(0, 1);

                        lastKnownReview.LatestReviewTime = DateTimeOffset.FromUnixTimeMilliseconds(latestReviews.First().Time * 1000);

                        await _slack.UpdateGoogleReview(lastKnownReview);

                        await NotifiyPeople(placeId, existingPlaceDetail.Result.Name, latestReviews);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(7.5));
                }
            }, token);
        }
    }
}
