namespace SuperMetroid.Core.Game;

/// <summary>Physical spawn and signed 8.8 velocity for one exploded escape-door fragment.</summary>
internal readonly record struct MotherBrainDoorFragmentDefinition(
    short XOffset,
    short YOffset,
    short XVelocity,
    short YVelocity);

/// <summary>Compiled physical definitions for Mother Brain's eight escape-door fragments.</summary>
internal static class MotherBrainDoorFragmentDefinitions
{
    /// <summary>
    /// Interleaved X/Y offsets at <c>$86:C992-$86:C9B1</c> and signed 8.8 velocities at
    /// <c>$86:C9B2-$86:C9D1</c>, selected by enemy-projectile parameter zero through seven.
    /// </summary>
    private static readonly MotherBrainDoorFragmentDefinition[] Definitions =
    [
        new(0, -32, 0x0500, -0x0200),
        new(0, -24, 0x0500, -0x0100),
        new(0, -16, 0x0500, -0x0100),
        new(0, -8, 0x0500, -0x0080),
        new(0, 0, 0x0500, -0x0080),
        new(0, 8, 0x0500, 0x0080),
        new(0, 16, 0x0500, -0x0100),
        new(0, 24, 0x0500, 0x0200),
    ];

    /// <summary>Returns the fragment record selected by the native spawn parameter.</summary>
    internal static MotherBrainDoorFragmentDefinition ForParameter(ushort parameter)
    {
        if (parameter >= Definitions.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain door fragment parameter must be zero through seven.");
        }

        return Definitions[parameter];
    }
}
