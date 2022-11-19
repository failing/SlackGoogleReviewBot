using SlackGoogleReviewBot.Apis;
using SlackGoogleReviewBot.Entities;
using SlackGoogleReviewBot.Entities.Auth;
using SlackGoogleReviewBot.Entities.Google;
using SlackGoogleReviewBot.Services.Interfaces;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Interaction;
using SlackNet.WebApi;
using Button = SlackNet.Blocks.Button;

namespace SlackGoogleReviewBot.SlashCommands
{
    public class SearchSlashCommandHandler : ISlashCommandHandler, IBlockActionHandler<ButtonAction>
    {
        private readonly IGooglePlacesApi _api;
        private readonly ISlackJobService _slackJobService;
        private readonly ISlackApiClient _slackApiClient;
        private readonly ISlackService _slackService;
        private readonly GoogleSecrets _googleSettings;
        private readonly ILogger<SearchSlashCommandHandler> _logger;

        public SearchSlashCommandHandler(IGooglePlacesApi api, ISlackApiClient slackApiClient, ISlackService slackService, GoogleSecrets settings, ISlackJobService slackJobService, ILogger<SearchSlashCommandHandler> logger)
        {
            _api = api;
            _googleSettings = settings;
            _slackJobService = slackJobService;
            _logger = logger;
            _slackApiClient = slackApiClient;
            _slackService = slackService;
        }

        public async Task<SlashCommandResponse> Handle(SlashCommand command)
        {
            _logger.LogInformation("{UserId} is searching with the input {Input}", command.UserId, command.Text);

            GooglePlaceSearchResult? searchResults = await _api.SearchPlaces(_googleSettings.ApiKey, command.Text);
            List<Block> blocks = new List<Block>();

            foreach (PlaceSearchDetailItem place in searchResults.Places)
            {
                blocks.Add(SlackUiBlockHelp.GetHeaderBlock(place.Name));
                blocks.Add(new SectionBlock()
                {
                    Text = new Markdown($"{SlackUiBlockHelp.GetStartCount(place.Rating)}\n\n{place.FormattedAddress}\n\nTotal Reviews: {place.UserRatingsTotal}")
                });

                if (place.Photos.Count > 0)
                {
                    HttpResponseMessage message = await _api.GetImage(_googleSettings.ApiKey, place.Photos[0].PhotoId, place.Photos[0].Width);

                    if (message != null && (int)message.StatusCode < 300)
                    {
                        blocks.Add(SlackUiBlockHelp.GetImageBlock(place.Name, message.RequestMessage!.RequestUri!.ToString()!));
                    }
                }

                blocks.Add(new ActionsBlock()
                {
                    Elements = new List<IActionElement>()
                    {
                        new Button()
                        {
                            Value = place.PlaceId,
                            Text = "Track Reviews",
                            AccessibilityLabel = "A button that once clicked the bot will track reviews for",
                            Style = ButtonStyle.Primary,
                            ActionId = "confirm_review"
                        },
                    }
                });

                blocks.Add(new DividerBlock());
            }

            return new SlashCommandResponse()
            {
                ResponseType = ResponseType.Ephemeral,
                Message = new Message()
                {
                    Blocks = blocks,
                    Text = blocks.Count <= 0 ? "No results found" : null
                }
            };
        }

        public async Task Handle(ButtonAction action, BlockActionRequest request)
        {
            string placeIdToTrack = action.Value;
            string userId = request.User.Id;
            string teamId = request.Team.Id;

            _slackJobService.CreateJob(userId, teamId, placeIdToTrack);

            SlackUserToken userToken = await _slackService.GetSlackUserToken(userId, teamId);

            _ = _slackApiClient.WithAccessToken(userToken.AccessToken).Chat.PostEphemeral(userId, new Message()
            {
                Channel = request.Channel.Id,
                Text = "Sucessfully subscribed!"
            });
        }
    }
}
