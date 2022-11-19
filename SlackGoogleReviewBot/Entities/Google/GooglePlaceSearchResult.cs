using Newtonsoft.Json;

namespace SlackGoogleReviewBot.Entities.Google
{
    public class GooglePlaceSearchResult
    {
        [JsonProperty("candidates")]
        public List<PlaceSearchDetailItem> Places { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class PlaceSearchDetailItem
    {
        public string ImageUrl { get; set; }

        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty("geometry")]
        public PlaceGeometry Geometry { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("photos")]
        public List<PlaceSearchDetailItemPhoto> Photos { get; set; }

        [JsonProperty("place_id")]
        public string PlaceId { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("user_ratings_total")]
        public int UserRatingsTotal { get; set; }
    }

    public class PlaceGeometry
    {
        [JsonProperty("location")]
       public Location Location { get; set; }
    }

    public class PlaceSearchDetailItemPhoto
    {
        [JsonProperty("photo_reference")]
        public string PhotoId { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
