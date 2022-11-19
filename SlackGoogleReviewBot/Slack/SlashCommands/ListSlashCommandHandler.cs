using Microsoft.EntityFrameworkCore;
using SlackGoogleReviewBot.Apis;
using SlackGoogleReviewBot.Database;
using SlackGoogleReviewBot.Entities;
using SlackGoogleReviewBot.Entities.Auth;
using SlackGoogleReviewBot.Entities.Google;
using SlackGoogleReviewBot.Services.Interfaces;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Interaction;
using SlackNet.WebApi;

namespace SlackGoogleReviewBot.SlashCommands
{
    public class ListSlashCommandHandler : ISlashCommandHandler, IBlockActionHandler<ButtonAction>
    {
        private readonly IGooglePlacesApi _api;
        private readonly ISlackJobService _slackJobService;
        private readonly ISlackApiClient _slackApi;
        private readonly GoogleSecrets _googleSettings;
        private readonly SlackReviewerDbContext _context;
        private readonly ILogger<ListSlashCommandHandler> _logger;

        public ListSlashCommandHandler(IGooglePlacesApi api, ISlackApiClient slackApi, GoogleSecrets settings, ISlackJobService slackJobService, ILogger<ListSlashCommandHandler> logger, SlackReviewerDbContext context)
        {
            _api = api;
            _googleSettings = settings;
            _slackJobService = slackJobService;
            _logger = logger;
            _context = context;
            _slackApi = slackApi;
        }

        public async Task<SlashCommandResponse> Handle(SlashCommand command)
        {
            _logger.LogInformation("{UserId} is listing businesses", command.UserId);

            List<string> distinctSubscribedPlacesForUser = await _context.SlackBusinessSubscriptions.Where(r => r.SlackUserId == command.UserId).Select(r => r.PlaceId).Distinct().ToListAsync();
            List<Task<GooglePlaceDetail>> placeResults = new List<Task<GooglePlaceDetail>>();

            foreach (string placeId in distinctSubscribedPlacesForUser)
            {
                placeResults.Add(_api.GetPlace(_googleSettings.ApiKey, placeId));
            }

            await Task.WhenAll(placeResults);

            List<Block> blocks = new List<Block>();
            foreach (Task<GooglePlaceDetail> detailTask in placeResults)
            {
                var detailResult = (await detailTask).Result;

                blocks.Add(SlackUiBlockHelp.GetHeaderBlock(detailResult.Name));
                blocks.Add(new SectionBlock()
                {
                    Text = new Markdown($"{SlackUiBlockHelp.GetStartCount(detailResult.Rating)}\n\n{detailResult.FormattedAddress}\n\n<{detailResult.Website}>")
                });

                if (detailResult.Photos.Count > 0)
                {
                    HttpResponseMessage message = await _api.GetImage(_googleSettings.ApiKey, detailResult.Photos[0].PhotoId, detailResult.Photos[0].Width);

                    if (message != null && (int)message.StatusCode < 300)
                    {
                        blocks.Add(SlackUiBlockHelp.GetImageBlock(detailResult.Name, message.RequestMessage!.RequestUri!.ToString()!));
                    }
                }

                blocks.Add(new ActionsBlock()
                {
                    Elements = new List<IActionElement>()
                    {
                        new SlackNet.Blocks.Button()
                        {
                            Text = "Go To Business",
                            AccessibilityLabel = "A quick link button to take you to the business url",
                            Style = ButtonStyle.Primary,
                            Url = detailResult.Url,
                        },
                        new SlackNet.Blocks.Button()
                        {
                            Value = detailResult.PlaceId,
                            Text = "Stop Tracking",
                            AccessibilityLabel = "A button that once clicked the bot will stop tracking reviews for",
                            Style = ButtonStyle.Danger,
                            ActionId = "stop_review"
                        }
                    }
                });

                blocks.Add(new DividerBlock());
            }

            return new SlashCommandResponse()
            {
                ResponseType = ResponseType.Ephemeral,
                Message = new Message()
                {
                    Blocks = blocks
                }
            };
        }

        public async Task Handle(ButtonAction action, BlockActionRequest request)
        {
            if (action.ActionId == "stop_review")
            {
                string userId = request.User.Id;
                string placeId = action.Value;
                string teamId = request.Team.Id;

                await _slackJobService.DeleteJob(userId, teamId, placeId);

                SlackUserToken userToken = await _context.SlackUserTokens.Where(r => r.TeamId == teamId && r.SlackUserId == userId).FirstOrDefaultAsync();

                await _slackApi.WithAccessToken(userToken!.AccessToken).Chat.PostEphemeral(userId, new Message()
                {
                    Channel = userToken.ChannelId,
                    Text = "Successfully unsubscribed!"
                });
            }
        }
    }
}
