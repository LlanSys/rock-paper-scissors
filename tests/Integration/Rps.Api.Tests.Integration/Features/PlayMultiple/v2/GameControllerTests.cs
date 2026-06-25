using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Rps.Api.Http.Features.PlayMultiple.v2.Dtos;
using Rps.Api.Http.Features.PlaySingle.v2.Dtos;
using Rps.Api.Http.Shared.Dtos;
using System.Collections.Generic;

namespace Rps.Api.Tests.Integration.Features.PlayMultiple.v2;

public class GameControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GameControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PlayMultiple_WithValidActions_ReturnsOk()
    {
        // Arrange
        var request = new PlayMultipleRequest(["fire", "water"]);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v2/game/playmultiple", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AppResponse<List<PlayResponse>>>();
        content.Should().NotBeNull();
        content!.IsSuccess.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data.Should().HaveCount(2);
        content.Data![0].PlayerMove.Should().Be("Fire");
        content.Data[1].PlayerMove.Should().Be("Water");
    }

    [Fact]
    public async Task PlayMultiple_WithInvalidAction_ReturnsBadRequest()
    {
        // Arrange
        var request = new PlayMultipleRequest(["fire", "rock"]);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v2/game/playmultiple", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
