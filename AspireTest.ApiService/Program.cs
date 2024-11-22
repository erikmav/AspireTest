using AspireTest.Models;
using Qdrant.Client;
using Qdrant.Client.Grpc;

#pragma warning disable CA1861 // Avoid constant arrays as arguments

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// AppHost lists the local container to execute under this connection name.
// https://learn.microsoft.com/en-us/dotnet/aspire/database/qdrant-integration
builder.AddQdrantClient("qdrant");

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    WeatherForecast[] forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

IHostApplicationLifetime lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
CancellationToken stopCt = lifetime.ApplicationStopping;
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapGet("/qdrantQuery1", async (HttpContext context) =>
{
    logger.LogInformation("Entered qdrantQuery1");
    try
    {
        QdrantClient qdrantClient = app.Services.GetRequiredService<QdrantClient>();
        PayloadIncludeSelector payloadIncludeSelector = new();
        payloadIncludeSelector.Fields.Add("city");
        IReadOnlyList<ScoredPoint> dotProductSimilarityScoredPoints = await qdrantClient.QueryAsync(
            "test_collection",
            query: new[] { 0.2f, 0.1f, 0.9f, 0.7f },
            limit: 3,
            payloadSelector: new WithPayloadSelector { Include = payloadIncludeSelector },
            cancellationToken: context.RequestAborted);
        List<CityDotProductSimilarity> results = dotProductSimilarityScoredPoints
            .Select(p => new CityDotProductSimilarity(p.Payload["city"].StringValue, p.Score))
            .ToList();
        return Results.Ok(results);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "qdrantQuery1");
        throw;
    }
})
.WithName("RunQdrant1");

// Create a test collection and populate with data as seen in the Qdrant quick start:
// https://qdrant.tech/documentation/quickstart/
QdrantClient qdrantClient = app.Services.GetRequiredService<QdrantClient>();

logger.LogInformation("Creating Qdrant test_collection");
await qdrantClient.CreateCollectionAsync("test_collection", new VectorParams { Size = 4, Distance = Distance.Dot }, cancellationToken: stopCt);

logger.LogInformation("Populating Qdrant test_collection with initial data");
UpdateResult operationInfo = await qdrantClient.UpsertAsync(collectionName: "test_collection", points:
[
    new PointStruct
    {
        Id = 1,
            Vectors = new[] { 0.05f, 0.61f, 0.76f, 0.74f },
            Payload = { ["city"] = "Berlin" },
    },
    new PointStruct
    {
        Id = 2,
            Vectors = new[] { 0.19f, 0.81f, 0.75f, 0.11f },
            Payload = { ["city"] = "London" },
    },
    new PointStruct
    {
        Id = 3,
        Vectors = new[] { 0.36f, 0.55f, 0.47f, 0.94f },
        Payload = { ["city"] = "Moscow" },
    },
    new PointStruct
    {
        Id = 3,
        Vectors = new[] { 0.18f, 0.01f, 0.85f, 0.80f },
        Payload = { ["city"] = "New York" },
    },
    new PointStruct
    {
        Id = 3,
        Vectors = new[] { 0.24f, 0.18f, 0.22f, 0.44f },
        Payload = { ["city"] = "Beijing" },
    },
    new PointStruct
    {
        Id = 3,
        Vectors = new[] { 0.35f, 0.08f, 0.11f, 0.44f },
        Payload = { ["city"] = "Mumbai" },
    },
]);
if (operationInfo.Status == UpdateStatus.Completed)
{
    logger.LogInformation("test_collection populated");
}
else
{
    logger.LogError("test_collection population failed: {UpdateResult}", operationInfo);
}

app.MapDefaultEndpoints();

await app.RunAsync(stopCt);
