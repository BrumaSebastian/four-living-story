using System.Reflection;

namespace FourLivingStory.Application;

public static class ModuleExtensions
{
	public static IServiceCollection AddModules (
		this IServiceCollection services,
		IConfiguration configuration,
		params Assembly [ ] assemblies )
	{
		foreach (var assembly in assemblies)
		{
			var types = assembly.GetTypes()
				.Where(t => typeof(IServiceModule).IsAssignableFrom(t)
						 && t is { IsInterface: false, IsAbstract: false }
						 && t.GetConstructor(Type.EmptyTypes) is not null);

			foreach (var type in types)
				((IServiceModule)Activator.CreateInstance(type)!)
					.AddServices(services, configuration);
		}

		return services;
	}

	public static IEndpointRouteBuilder MapModules (
		this IEndpointRouteBuilder app,
		params Assembly [ ] assemblies )
	{
		foreach (var assembly in assemblies)
		{
			var types = assembly.GetTypes()
				.Where(t => typeof(IEndpointModule).IsAssignableFrom(t)
						 && t is { IsInterface: false, IsAbstract: false }
						 && t.GetConstructor(Type.EmptyTypes) is not null);

			foreach (var type in types)
				((IEndpointModule)Activator.CreateInstance(type)!)
					.MapEndpoints(app);
		}

		return app;
	}
}
