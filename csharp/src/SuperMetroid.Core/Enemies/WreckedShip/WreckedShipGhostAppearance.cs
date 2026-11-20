using static SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostAppearanceDefinitions;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>A Wrecked Ship ghost appearance's spawn position.</summary>
public static class WreckedShipGhostAppearance
{
    /// <summary>Where the ghost appears around Samus when she has been moving as <paramref name="motion"/> says.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="motion"/> names no way of moving.</exception>
    public static Offset16<RoomPixels> SpawnOffset(SamusMotion motion) => Ensure.Listed(SpawnOffsets, motion);
}
