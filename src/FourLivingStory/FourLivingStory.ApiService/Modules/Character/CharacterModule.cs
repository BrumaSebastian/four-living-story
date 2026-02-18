namespace FourLivingStory.ApiService.Modules.Character;

public static class CharacterModule
{
    public static IServiceCollection AddCharacterModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapCharacterModule(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
