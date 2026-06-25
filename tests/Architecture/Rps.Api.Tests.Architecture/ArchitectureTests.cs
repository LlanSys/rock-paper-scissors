using NetArchTest.Rules;

using Rps.Api.Http.Domain.Enums.v2;

using Xunit;

namespace Rps.Api.Tests.Architecture;

public class ArchitectureTests
{
    [Fact]
    public void V1_Slices_Should_Not_Depend_On_V2_Slices()
    {
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .ResideInNamespace("Rps.Api.Http.Features.PlaySingle.v1.Controllers")
            .Or()
            .ResideInNamespace("Rps.Api.Http.Features.PlayMultiple.v1.Controllers")
            .Or()
            .ResideInNamespace("Rps.Api.Http.Features.PlaySingle.v1.Dtos")
            .Or()
            .ResideInNamespace("Rps.Api.Http.Features.PlayMultiple.v1.Dtos")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Rps.Api.Http.Features.PlaySingle.v2.Controllers",
                "Rps.Api.Http.Features.PlayMultiple.v2.Controllers",
                "Rps.Api.Http.Features.PlaySingle.v2",
                "Rps.Api.Http.Features.PlayMultiple.v2")
            .GetResult();

        Assert.True(result.IsSuccessful, "V1 logic is leaking dependencies into V2!");
    }
}
