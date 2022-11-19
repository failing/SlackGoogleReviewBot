using SlackGoogleReviewBot.Services.Interfaces;

namespace SlackGoogleReviewBot
{
    public class HydrateJobService : IHostedService
    {
        private readonly ISlackJobService _slackJobService;
        public HydrateJobService(ISlackJobService slackJobService)
        {
            _slackJobService = slackJobService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _slackJobService.RemakeSchedules();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
