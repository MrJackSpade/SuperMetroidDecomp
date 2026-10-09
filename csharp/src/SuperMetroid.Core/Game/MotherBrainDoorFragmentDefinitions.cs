namespace SuperMetroid.Core.Game;

/// <summary>Physical spawn and signed 8.8 velocity for one exploded escape-door fragment.</summary>
/// <param name="XOffset">Horizontal spawn offset from the door in pixels.</param>
/// <param name="YOffset">Vertical spawn offset from the door in pixels.</param>
/// <param name="XVelocity">Signed 8.8 horizontal velocity applied to the fragment.</param>
/// <param name="YVelocity">Signed 8.8 vertical velocity selected for the fragment's scatter arc.</param>
internal readonly record struct MotherBrainDoorFragmentDefinition(
    short XOffset,
    short YOffset,
    short XVelocity,
    short YVelocity);

/// <summary>Compiled physical definitions for Mother Brain's eight escape-door fragments.</summary>
internal static class MotherBrainDoorFragmentDefinitions
{
    /// <summary>Number of parameter-indexed fragment trajectories encoded by the scatter table.</summary>
    internal const int Count = 8;

    /// <summary>$86:C9B2-$86:C9D1, interleaved with the X velocity: the fragments' authored scatter speeds.</summary>
    private static ReadOnlySpan<short> YVelocities => [-0x0200, -0x0100, -0x0100, -0x0080, -0x0080, 0x0080, -0x0100, 0x0200];

    /// <summary>
    /// Returns the fragment selected by enemy-projectile parameter zero through seven. The
    /// interleaved offsets at <c>$86:C992-$86:C9B1</c> are a zero X and a Y that steps eight
    /// pixels from 32 above the door; every fragment flies right at $0500.
    /// </summary>
    internal static MotherBrainDoorFragmentDefinition ForParameter(ushort parameter)
    {
        if (parameter >= Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain door fragment parameter must be zero through seven.");
        }

        return new(0, (short)(-0x20 + 8 * parameter), 0x0500, YVelocities[parameter]);
    }
}
