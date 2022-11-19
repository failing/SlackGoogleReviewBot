using Microsoft.EntityFrameworkCore;
using SlackGoogleReviewBot.Database;
using SlackGoogleReviewBot.Entities;
using SlackGoogleReviewBot.Services.Interfaces;
using SlackNet;
using SlackNet.Events;

namespace SlackGoogleReviewBot.Slack.EventHandling
{
    public class AppUninstallHandler : IEventHandler
    {
        private readonly SlackReviewerDbContext _context;
        private readonly ISlackJobService _slackJobService;

        public AppUninstallHandler(SlackReviewerDbContext context, ISlackJobService service)
        {
            _context = context;
            _slackJobService = service;
        }

        public async Task Handle(EventCallback eventCallback)
        {
            if (eventCallback.Event.Type == "app_uninstalled")
            {
                string teamId = eventCallback.TeamId;

                List<SlackUserToken> entriesToRemove = await _context.SlackUserTokens.Where(r => r.TeamId == teamId).ToListAsync();
                List<string> entrieIds = entriesToRemove.Select(r => r.SlackUserId).Distinct().ToList();
                List<SlackBusinessSubscription> usersToRemove = await _context.SlackBusinessSubscriptions.Where(r => entrieIds.Contains(r.SlackUserId)).ToListAsync();

                await _slackJobService.DeleteJobFromTeamId(teamId);

                _context.SlackUserTokens.RemoveRange(entriesToRemove);

                await _context.SaveChangesAsync();
            }
        }
    }
}
