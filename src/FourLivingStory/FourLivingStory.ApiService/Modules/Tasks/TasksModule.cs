namespace FourLivingStory.ApiService.Modules.Tasks;

public static class TasksModule
{
    public static IServiceCollection AddTasksModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapTasksModule(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
