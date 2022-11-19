namespace SlackGoogleReviewBot.Services.Interfaces
{
    public interface ISlackJobService
    {
        Task DeleteJob(string userId, string teamId, string placeId);
        Task CreateJob(string userId, string teamId, string placeId);
        Task DeleteJobFromTeamId(string teamId);
        Task RemakeSchedules();
    }
}
