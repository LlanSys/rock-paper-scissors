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

    [Fact]
    public void Domain_Should_Not_Depend_On_Features()
    {
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .ResideInNamespace("Rps.Api.Http.Domain")
            .ShouldNot()
            .HaveDependencyOn("Rps.Api.Http.Features")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain logic must not depend on Features!");
    }

    [Fact]
    public void FeaturesV1_Should_Not_Depend_On_MediatR()
    {
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .ResideInNamespace("Rps.Api.Http.Features.PlaySingle.v1")
            .Or()
            .ResideInNamespace("Rps.Api.Http.Features.PlayMultiple.v1")
            .ShouldNot()
            .HaveDependencyOn("MediatR")
            .GetResult();

        Assert.True(result.IsSuccessful, "V1 features should not depend on MediatR!");
    }

    [Fact]
    public void DomainContracts_Should_Not_Depend_On_AspNetCore_Mvc()
    {
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .ResideInNamespace("Rps.Api.Http.Domain.Contracts")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore.Mvc")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain.Contracts must not depend on Microsoft.AspNetCore.Mvc!");
    }
}
