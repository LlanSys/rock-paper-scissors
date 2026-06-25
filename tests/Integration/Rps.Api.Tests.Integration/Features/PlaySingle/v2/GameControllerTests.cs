using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Rps.Api.Http.Features.PlaySingle.v2.Dtos;
using Rps.Api.Http.Shared.Dtos;

namespace Rps.Api.Tests.Integration.Features.PlaySingle.v2;

public class GameControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GameControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Play_WithValidAction_ReturnsOk()
    {
        // Arrange
        var request = new PlayRequest("fire");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v2/game/play", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AppResponse<PlayResponse>>();
        content.Should().NotBeNull();
        content!.IsSuccess.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.PlayerMove.Should().Be("Fire");
    }

    [Fact]
    public async Task Play_WithInvalidAction_ReturnsBadRequest()
    {
        // Arrange
        var request = new PlayRequest("rock");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v2/game/play", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
