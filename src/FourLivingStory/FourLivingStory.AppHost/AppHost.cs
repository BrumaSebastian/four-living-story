var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
	.WithPgAdmin();

var db = postgres.AddDatabase("fourlivingstory");

var apiService = builder.AddProject<Projects.FourLivingStory_ApiService>("apiservice")
	.WithHttpHealthCheck("/health")
	.WithReference(db)
	.WaitFor(db);

builder.AddProject<Projects.FourLivingStory_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithHttpHealthCheck("/health")
	.WithReference(apiService)
	.WaitFor(apiService);

builder.Build().Run();
