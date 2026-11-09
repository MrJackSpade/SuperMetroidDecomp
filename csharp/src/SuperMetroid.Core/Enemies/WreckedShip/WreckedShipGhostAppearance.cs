using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostAppearanceDefinitions;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>A Wrecked Ship ghost appearance's colours and spawn position.</summary>
public static class WreckedShipGhostAppearance
{
    /// <summary>The sixteen colours the Wrecked Ship ghosts' fades move toward as they materialize, colour zero first.</summary>
    public static IReadOnlyList<RgbColor> Colours => OwnColours;

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
}
