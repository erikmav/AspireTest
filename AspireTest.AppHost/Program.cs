// Force Podman use.
// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling?tabs=windows&pivots=visual-studio#container-runtime
// Environment.SetEnvironmentVariable("DOTNET_ASPIRE_CONTAINER_RUNTIME", "podman");

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Add a Qdrant container for local testing. Impliictly uses ContainerLifetime.Session meaning start and stop with the apphost.
// https://learn.microsoft.com/en-us/dotnet/aspire/database/qdrant-integration?tabs=package-reference
// https://qdrant.tech/documentation/quickstart/
IResourceBuilder<QdrantServerResource> qdrant = builder.AddQdrant("qdrant");

IResourceBuilder<ProjectResource> apiService = builder.AddProject<Projects.AspireTest_ApiService>("apiservice")
    .WithReference(qdrant)
    .WaitFor(qdrant);

builder.AddProject<Projects.AspireTest_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
