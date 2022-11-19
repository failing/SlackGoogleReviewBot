namespace SlackGoogleReviewBot.Entities.Slack
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SlackTokenResponse
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("authed_user")]
        public AuthedUser AuthedUser { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("bot_user_id")]
        public string BotUserId { get; set; }

        [JsonProperty("team")]
        public Team Team { get; set; }

        [JsonProperty("enterprise")]
        public object Enterprise { get; set; }

        [JsonProperty("is_enterprise_install")]
        public bool IsEnterpriseInstall { get; set; }

        [JsonProperty("incoming_webhook")]
        public IncomingWebhook IncomingWebhook { get; set; }
    }

    public partial class AuthedUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class IncomingWebhook
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("configuration_url")]
        public Uri ConfigurationUrl { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class Team
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
