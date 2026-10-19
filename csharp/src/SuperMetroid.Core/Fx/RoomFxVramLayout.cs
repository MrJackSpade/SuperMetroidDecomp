using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Fx;

/// <summary>Room-effect, scrolling-sky and HUD tilemap VRAM written by gameplay (#630).</summary>
[PpuLayoutBoundary("catalog: room-effect and HUD tilemap VRAM")]
public static class RoomFxVramLayout
{

    /// <summary>VMADD $5820-$58DF: the three mutable HUD rows uploaded from <c>$7E:C608</c> by <c>$80:9CC3</c>.</summary>
    public static readonly PpuLayoutCell HudMutableRows = new(nameof(HudMutableRows), PpuLayoutRegion.Vram,
        HudState.VramDestination, HudState.MutableTileCount, "BG3 tilemap words", GameplayVramLayout.HudTilemap);
}
