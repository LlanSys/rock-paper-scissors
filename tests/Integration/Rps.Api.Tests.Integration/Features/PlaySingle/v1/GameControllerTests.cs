using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Rps.Api.Http.Features.PlaySingle.v1.Dtos;

namespace Rps.Api.Tests.Integration.Features.PlaySingle.v1;

public class GameControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GameControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Play_ReturnsOkAndResult()
    {
        // Arrange
        var request = new PlayRequest { Action = "rock" };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/game/play", request);

        // Assert
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new System.Exception("Response failed with status " + response.StatusCode + ": " + err);
        }
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().ContainAny("draw", "Winner", "Loser");
    }
}
