var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
	.WithPgAdmin();

var db = postgres.AddDatabase("fourlivingstory");

var apiService = builder.AddProject<Projects.FourLivingStory_ApiService>("apiservice")
	.WithHttpHealthCheck("/health")
	.WithReference(db)
	.WaitFor(db);

var web = builder.AddProject<Projects.FourLivingStory_Web>("webfrontend")
	.WithHttpHealthCheck("/health")
	.WithReference(apiService)
	.WaitFor(apiService);

if (builder.Environment.EnvironmentName == "Development")
{
	// In dev, expose Web directly — no proxy in the way.
	web.WithExternalHttpEndpoints();
}
else
{
	// In non-dev, the gateway is the single public entry point.
	// Web and ApiService are internal only.
	builder.AddProject<Projects.FourLivingStory_Gateway>("gateway")
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(apiService)
		.WithReference(web)
		.WaitFor(web);
}

builder.Build().Run();
