using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Where each claw height stands in the cartridge's word-sized claw-height tables (#1747), which only the Ridley oracles
/// read: the native cue's operand and Ridley's native state hold the height's byte offset into them.
/// </summary>
internal static class RidleyClawHeightPlaces
{
    /// <summary>The claw heights in the tables' order.</summary>
    internal static readonly RidleyClawHeight[] TableOrder =
        [RidleyClawHeight.Standing, RidleyClawHeight.LegsHalfExtended, RidleyClawHeight.LegsExtended];

    /// <summary>The place of <paramref name="height"/> in the tables.</summary>
    internal static int Place(RidleyClawHeight height)
    {
        int place = Array.IndexOf(TableOrder, height);
        return place >= 0 ? place : throw new ArgumentOutOfRangeException(nameof(height), height, "Undefined claw height.");
    }

    /// <summary>The byte offset of <paramref name="height"/>'s word in the tables.</summary>
    internal static int ByteOffset(RidleyClawHeight height) => Place(height) * sizeof(ushort);
}
