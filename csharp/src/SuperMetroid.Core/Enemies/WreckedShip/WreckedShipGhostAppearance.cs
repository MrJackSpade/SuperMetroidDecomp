using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostAppearanceDefinitions;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>A Wrecked Ship ghost appearance's colours and spawn position.</summary>
public static class WreckedShipGhostAppearance
{
    /// <summary>The sixteen colours the Wrecked Ship ghosts' fades move toward as they materialize, colour zero first.</summary>
    public static IReadOnlyList<RgbColor> Colours => OwnColours;

    /// <summary>Where the ghost appears around Samus when she has been moving as <paramref name="motion"/> says.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="motion"/> names no way of moving.</exception>
    public static Offset16<RoomPixels> SpawnOffset(SamusMotion motion) => Ensure.Listed(SpawnOffsets, motion);
}
