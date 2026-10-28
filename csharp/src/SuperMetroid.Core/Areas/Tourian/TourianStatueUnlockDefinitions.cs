namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Compiled fixed physical definitions for Tourian statue unlock effects.</summary>
internal static class TourianStatueUnlockDefinitions
{
    /// <summary>
    /// Four eye/soul X words at <c>$86:B90E-$86:B915</c> paired with the four Y words at
    /// <c>$86:B916-$86:B91D</c>. The native boss parameter is a doubled byte offset:
    /// Phantoon zero, Ridley two, Draygon four, or Kraid six.
    /// </summary>
    internal static readonly Position16<RoomPixels>[] EyePositions =
    [
        new(0x0084, 0x0090),
        new(0x007a, 0x0051),
        new(0x009e, 0x0080),
        new(0x0068, 0x0072),
    ];
}
