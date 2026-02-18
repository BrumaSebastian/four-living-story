namespace FourLivingStory.ApiService.Modules.Expenses;

public static class ExpensesModule
{
    public static IServiceCollection AddExpensesModule(this IServiceCollection services)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapExpensesModule(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
