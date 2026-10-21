
namespace SuperMetroid.Core.Hud;

/// <summary>The HUD tilemap VRAM gameplay writes (#630).</summary>
[PpuLayoutBoundary("catalog: HUD tilemap VRAM")]
public static class HudVramLayout
{

    /// <summary>VMADD $5820-$58DF: the three mutable HUD rows uploaded from <c>$7E:C608</c> by <c>$80:9CC3</c>.</summary>
    public static readonly PpuLayoutCell HudMutableRows = new(nameof(HudMutableRows), PpuLayoutRegion.Vram,
        HudState.VramDestination, HudState.MutableTileCount, "BG3 tilemap words", GameplayVramLayout.HudTilemap);
}
