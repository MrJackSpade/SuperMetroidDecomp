using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Fx;

/// <summary>Room-effect, scrolling-sky and HUD tilemap VRAM written by gameplay (#630).</summary>
[PpuLayoutBoundary("catalog: room-effect and HUD tilemap VRAM")]
public static class RoomFxVramLayout
{
    /// <summary>
    /// VMADD $5880-$5FFF: the layer-3 effect area that <c>Clear_FX_Tilemap</c> ($82:E566) and
    /// <c>ClearFXTilemap</c> ($80:A29C) fill before every room's effect upload.
    /// </summary>
    public static readonly PpuLayoutCell Layer3Area = new(nameof(Layer3Area), PpuLayoutRegion.Vram,
        RoomFxRomData.Layer3.ClearDestinationWord, RoomFxRomData.Layer3.ClearWordCount, "BG3 tilemap words", GameplayVramLayout.HudTilemap);

    /// <summary>VMADD $5BE0-$5FFF: where a room effect's bank-$8A layer-3 tilemap is uploaded.</summary>
    public static readonly PpuLayoutCell Layer3Tilemap = new(nameof(Layer3Tilemap), PpuLayoutRegion.Vram,
        RoomFxRomData.Layer3.TilemapDestinationWord, 0x0420, "BG3 tilemap words", Layer3Area);

    /// <summary>VMADD $5820-$58DF: the three mutable HUD rows uploaded from <c>$7E:C608</c> by <c>$80:9CC3</c>.</summary>
    public static readonly PpuLayoutCell HudMutableRows = new(nameof(HudMutableRows), PpuLayoutRegion.Vram,
        HudState.VramDestination, HudState.MutableTileCount, "BG3 tilemap words", GameplayVramLayout.HudTilemap);
}
