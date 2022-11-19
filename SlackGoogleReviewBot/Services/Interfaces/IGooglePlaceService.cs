using SlackGoogleReviewBot.Entities.Google;

public interface IGooglePlaceService
{
    Task<GooglePlaceDetail?> GetPlaceDetails(string placeId);
    Task<GooglePlaceSearchResult?> SearchPlace(string placeId);
}