namespace SuperMetroid.Core.Game;

/// <summary>World position shared by one Tourian statue eye glow and released soul.</summary>
internal readonly record struct TourianStatueEyePosition(ushort X, ushort Y);

/// <summary>Compiled fixed physical definitions for Tourian statue unlock effects.</summary>
internal static class TourianStatueUnlockDefinitions
{
    /// <summary>
    /// Four eye/soul X words at <c>$86:B90E-$86:B915</c> paired with the four Y words at
    /// <c>$86:B916-$86:B91D</c>. The native boss parameter is a doubled byte offset:
    /// Phantoon zero, Ridley two, Draygon four, or Kraid six.
    /// </summary>
    private static readonly TourianStatueEyePosition[] EyePositions =
    [
        new(0x0084, 0x0090),
        new(0x007a, 0x0051),
        new(0x009e, 0x0080),
        new(0x0068, 0x0072),
    ];

    /// <summary>Returns the eye position selected by the native doubled boss parameter.</summary>
    internal static TourianStatueEyePosition EyePosition(ushort parameter)
    {
        if (parameter > 6 || (parameter & 1) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Tourian statue parameter must be zero, two, four, or six.");
        }

        return EyePositions[parameter >> 1];
    }
}
