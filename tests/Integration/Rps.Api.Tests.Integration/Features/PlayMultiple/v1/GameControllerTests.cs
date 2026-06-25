using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Rps.Api.Http.Features.PlayMultiple.v1.Dtos;

namespace Rps.Api.Tests.Integration.Features.PlayMultiple.v1;

public class GameControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GameControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PlayMultiple_ReturnsOkAndArray()
    {
        // Arrange
        var request = new MultiplePlayRequest { Actions = ["rock", "paper"] };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/game/playmultiple", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<string[]>();
        content.Should().NotBeNull();
        content.Should().HaveCount(4);
    }
}
