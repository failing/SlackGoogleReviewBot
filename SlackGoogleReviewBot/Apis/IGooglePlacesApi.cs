using RestEase;
using SlackGoogleReviewBot.Entities.Google;

namespace SlackGoogleReviewBot.Apis
{
    [BasePath("maps/api")]
    public interface IGooglePlacesApi
    {
        [Get("place/findplacefromtext/json?inputtype=textquery&fields=formatted_address,name,place_id,photo,icon,rating,geometry,user_ratings_total")]
        Task<GooglePlaceSearchResult> SearchPlaces([Query("key")] string apiKey, [Query("input")] string searchInput);

        [Get("place/details/json")]
        Task<GooglePlaceDetail> GetPlace([Query("key")] string apiKey, [Query("place_id")] string placeId);

        [Get("place/photo")]
        Task<HttpResponseMessage> GetImage([Query("key")] string apiKey, [Query("photo_reference")] string photoId, [Query("maxwidth")] int maxWidth);

        [Get("staticmap?zoom=13&size=200x200")]
        Task<HttpResponseMessage> GetMapStaticImage([Query("key")] string apiKey, [Query("center")] string latLong);
    }
}
