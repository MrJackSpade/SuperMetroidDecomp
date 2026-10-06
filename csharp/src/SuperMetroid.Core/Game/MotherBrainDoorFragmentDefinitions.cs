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
