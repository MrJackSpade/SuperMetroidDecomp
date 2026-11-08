using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostAppearanceDefinitions;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>A Wrecked Ship ghost appearance's colours, spawn position and flicker interval.</summary>
public static class WreckedShipGhostAppearance
{
    /// <summary>One of the sixteen colours the Wrecked Ship ghosts' fades move toward as they materialize.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the sixteen colours.</exception>
    public static RgbColor Colour(int index) =>
        (uint)index < (uint)OwnColours.Length ? OwnColours[index] : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>The spawn offset of grid position <paramref name="index"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the nine positions.</exception>
    public static Offset16<RoomPixels> SpawnOffset(int index)
    {
        Ensure.AtLeastZero(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, SpawnCount);
        return SpawnOffsets[index];
    }

    /// <summary>
    /// The flicker interval at <paramref name="index"/>; -1 at the terminator. Appearance starts at index 1 but
    /// index 0 remains valid when the caller resets its offset after the terminator.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the seventeen entries.</exception>
    public static short FlickerDuration(int index)
    {
        Ensure.AtLeastZero(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, FlickerCount);
        return FlickerDurations[index];
    }
}
