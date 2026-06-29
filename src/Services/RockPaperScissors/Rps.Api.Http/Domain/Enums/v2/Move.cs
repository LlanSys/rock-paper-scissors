using System.ComponentModel;

namespace Rps.Api.Http.Domain.Enums.v2;

public enum Move
{
    [Description("Fire")]
    Fire,

    [Description("Water")]
    Water,

    [Description("Wood")]
    Wood,

    [Description("Metal")]
    Metal,

    [Description("Earth")]
    Earth
}

