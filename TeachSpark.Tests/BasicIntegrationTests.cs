using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;

namespace TeachSpark.Tests;

/// <summary>
/// WebApplicationFactory that wires test-safe configuration overrides:
/// in-temp SQLite, a placeholder LLM key, and a writable log path.
/// </summary>
public class TeachSparkWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"teachspark-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["LlmConfiguration:ApiKey"] = "test-placeholder-key",
                ["TeachSpark:LogFilePath"] = Path.Combine(Path.GetTempPath(), "teachspark-test.log"),
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}

/// <summary>
/// Baseline integration tests: one success path, one validation/failure path.
/// These run against the full ASP.NET Core pipeline via an in-process test server.
/// </summary>
public class BasicIntegrationTests : IClassFixture<TeachSparkWebFactory>
{
    private readonly HttpClient _client;

    public BasicIntegrationTests(TeachSparkWebFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task Get_HomePage_Returns200OK()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_NonExistentRoute_Returns404()
    {
        var response = await _client.GetAsync("/this-route-xyz-does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
