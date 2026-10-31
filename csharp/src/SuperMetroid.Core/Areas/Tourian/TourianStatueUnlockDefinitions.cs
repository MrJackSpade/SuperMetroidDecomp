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
        new(132, 144),
        new(122, 81),
        new(158, 128),
        new(104, 114),
    ];
}
