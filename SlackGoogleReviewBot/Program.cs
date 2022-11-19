using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using RestEase;
using SlackGoogleReviewBot;
using SlackGoogleReviewBot.Apis;
using SlackGoogleReviewBot.Database;
using SlackGoogleReviewBot.Entities;
using SlackGoogleReviewBot.Entities.Auth;
using SlackGoogleReviewBot.Entities.Slack;
using SlackGoogleReviewBot.Services;
using SlackGoogleReviewBot.Services.Interfaces;
using SlackGoogleReviewBot.Slack.EventHandling;
using SlackGoogleReviewBot.SlashCommands;
using SlackNet.AspNetCore;
using SlackNet.Blocks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

string dbConnectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<SlackReviewerDbContext>(options =>
              options
                  .UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString),
                      providerOptions => providerOptions
                          .EnableRetryOnFailure(5))
           );


ISlackAuthApi slackAuthApi = RestClient.For<ISlackAuthApi>("https://slack.com");
IGooglePlacesApi googlePlacesApi = RestClient.For<IGooglePlacesApi>("https://maps.googleapis.com");

GoogleSecrets googleSecrets = new GoogleSecrets();
SlackSecrets secrets = new SlackSecrets();

builder.Configuration.GetSection("GoogleSecrets").Bind(googleSecrets);
builder.Configuration.GetSection("SlackSecrets").Bind(secrets);

builder.Services.AddSingleton(googlePlacesApi);
builder.Services.AddSingleton(slackAuthApi);
builder.Services.AddSingleton(googleSecrets);
builder.Services.AddSingleton(secrets);

builder.Services.AddSingleton<ISlackJobService, SlackJobService>();
builder.Services.AddScoped<ISlackService, SlackService>();
builder.Services.AddScoped<IGooglePlaceService, GooglePlaceService>();
builder.Services.AddHostedService<HydrateJobService>();

builder.Services.AddSlackNet(c =>
{
    c.RegisterSlashCommandHandler<SearchSlashCommandHandler>("/bsearch");
    c.RegisterSlashCommandHandler<ListSlashCommandHandler>("/blist");
    c.RegisterBlockActionHandler<ButtonAction, ListSlashCommandHandler>("stop_review");
    c.RegisterBlockActionHandler<ButtonAction, SearchSlashCommandHandler>("confirm_review");
    c.RegisterEventHandler<AppUninstallHandler>();
});

WebApplication app = builder.Build();

app.UseSlackNet(c =>
{
    c.UseSigningSecret(secrets.SigningSecret);
});

app.MapGet("/oauth-callback", async (string? code, ISlackService service, ILogger<Program> logger) =>
{
    SlackAuthTokenExchangeRequest request = new SlackAuthTokenExchangeRequest()
    {
        ClientId = secrets.ClientId,
        RedirectUri = secrets.RedirectUri,
        ClientSecret = secrets.ClientSecret,
        Code = code
    };

    if (string.IsNullOrEmpty(code))
    {
        return Results.Redirect("/");
    }

    try
    {
        SlackTokenResponse token = await slackAuthApi.ExchangeAuthorizationTokenForAccessToken(request.GetRequestData());

        if (token != null && token.Ok)
        {
            string botAccessToken = token.AccessToken;
            string userId = token.AuthedUser.Id;

            var newUserUpdate = new SlackUserToken()
            {
                AccessToken = botAccessToken,
                ChannelId = token.IncomingWebhook.ChannelId,
                SlackUserId = userId,
                TeamId = token.Team.Id
            };

            await service.CreateOrUpdateSlackUserToken(newUserUpdate);

            return Results.Redirect($"https://slack.com/app_redirect?channel={newUserUpdate.ChannelId}");
        }
        else
        {
            logger.LogError("Failed to retrieve token");
            return Results.Redirect($"/");
        }
    }
    catch (ApiException ex)
    {
        logger.LogError(ex, "Error");
        return Results.Redirect($"/");
    }
});

app.MapGet("/direct-install", (SlackSecrets slackSecrets, ILogger<Program> logger) =>
{
    SlackAuthRequest newSlackAuthRequest = new SlackAuthRequest()
    {
        ClientId = slackSecrets.ClientId,
        RedirectUri = slackSecrets.RedirectUri,
        Scopes = slackSecrets.Scopes
    };

    Uri newUrl = new Uri(QueryHelpers.AddQueryString(slackSecrets.AuthUrl, newSlackAuthRequest.GetRequestData()));

    return Results.Redirect(newUrl.ToString());
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
