namespace Micro_social_app.Services
{
    public interface IAIContentModerationService
    {
        Task<bool> IsContentAllowedAsync(string text);
    }
}



