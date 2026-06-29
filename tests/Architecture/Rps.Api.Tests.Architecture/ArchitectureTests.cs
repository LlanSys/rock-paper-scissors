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
            .ResideInNamespaceMatching(@"Rps\.Api\.Http\.Features\..*\.v1")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Rps.Api.Http.Features.PlaySingle.v2",
                "Rps.Api.Http.Features.PlayMultiple.v2")
            .GetResult();

        Assert.True(result.IsSuccessful, $"V1 logic is leaking dependencies into V2! Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
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

        Assert.True(result.IsSuccessful, $"Domain logic must not depend on Features! Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void FeaturesV1_Should_Not_Depend_On_MediatR()
    {
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .ResideInNamespaceMatching(@"Rps\.Api\.Http\.Features\..*\.v1")
            .ShouldNot()
            .HaveDependencyOn("MediatR")
            .GetResult();

        Assert.True(result.IsSuccessful, $"V1 features should not depend on MediatR! Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
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

        Assert.True(result.IsSuccessful, $"Domain.Contracts must not depend on Microsoft.AspNetCore.Mvc! Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void V2_CommandHandlers_Should_Not_Be_Public()
    {
        // Handlers are an implementation detail of a slice — they must not be
        // referenced directly from outside the slice; only via MediatR.
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .ResideInNamespaceMatching(@"Rps\.Api\.Http\.Features\..*\.v2\.Handlers")
            .And()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .BePublic()
            .GetResult();

        Assert.True(result.IsSuccessful, $"V2 command handlers must be internal! Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Shared_Should_Not_Depend_On_Features()
    {
        // Cross-cutting shared infrastructure must not import from any feature slice.
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .ResideInNamespace("Rps.Api.Http.Shared")
            .ShouldNot()
            .HaveDependencyOn("Rps.Api.Http.Features")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Shared layer must not depend on Features! Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Shared_Dtos()
    {
        // Domain contracts must remain free of HTTP/API layer DTO concerns.
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .ResideInNamespace("Rps.Api.Http.Domain")
            .ShouldNot()
            .HaveDependencyOn("Rps.Api.Http.Shared.Dtos")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain must not depend on Shared.Dtos! Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Controllers_Should_Not_Contain_Business_Logic()
    {
        // Controllers should only reside in namespaces ending in .Controllers —
        // this prevents business logic from drifting back into controllers.
        var result = Types.InAssembly(typeof(Move).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespaceMatching(@".*\.Controllers")
            .GetResult();

        Assert.True(result.IsSuccessful, $"All controllers must be in a .Controllers namespace! Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
