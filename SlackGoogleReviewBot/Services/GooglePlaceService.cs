using SlackGoogleReviewBot.Apis;
using SlackGoogleReviewBot.Entities.Auth;
using SlackGoogleReviewBot.Entities.Google;

public class GooglePlaceService : IGooglePlaceService
{
    private readonly GoogleSecrets _settings;
    private readonly IGooglePlacesApi _placesApi;

    public GooglePlaceService(GoogleSecrets settings,
                           IGooglePlacesApi api)
    {
        _settings = settings;
        _placesApi = api;
    }

    public async Task<GooglePlaceDetail?> GetPlaceDetails(string placeId)
    {
        return await _placesApi.GetPlace(_settings.ApiKey, placeId);
    }

    public async Task<GooglePlaceSearchResult?> SearchPlace(string placeId)
    {
        return await _placesApi.SearchPlaces(_settings.ApiKey, placeId);
    }
}