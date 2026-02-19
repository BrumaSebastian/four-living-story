namespace FourLivingStory.Application.Modules.Identity;

public interface ICurrentUser
{
    string UserId { get; }
    bool IsAuthenticated { get; }
}
