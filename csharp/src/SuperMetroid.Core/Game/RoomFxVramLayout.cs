using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

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

    /// <summary>
    /// VMADD $4800-$501F: the scrolling-sky rows queued by <c>$88:A61B</c> into the BG2 tilemap. A
    /// row pair at the bottom position spills one 32-word row into BG1's first row, as natively.
    /// </summary>
    public static readonly PpuLayoutCell ScrollingSkyRows = new(nameof(ScrollingSkyRows), PpuLayoutRegion.Vram,
        GameplayVramLayout.Bg2TilemapWord, 0x0820, "BG tilemap words (32-wide rows)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5820-$58DF: the three mutable HUD rows uploaded from <c>$7E:C608</c> by <c>$80:9CC3</c>.</summary>
    public static readonly PpuLayoutCell HudMutableRows = new(nameof(HudMutableRows), PpuLayoutRegion.Vram,
        HudState.VramDestination, HudState.MutableTileCount, "BG3 tilemap words", GameplayVramLayout.HudTilemap);
}

/// <summary>Typed view of room-effect and HUD tilemap VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: room-effect and HUD tilemap VRAM")]
public readonly struct RoomFxVram(SnesVram vram)
{
    /// <summary>The cleared layer-3 effect area.</summary>
    public VramRegion Layer3Area => new(vram, RoomFxVramLayout.Layer3Area);

    /// <summary>The room effect's layer-3 tilemap.</summary>
    public VramRegion Layer3Tilemap => new(vram, RoomFxVramLayout.Layer3Tilemap);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the scrolling-sky rows.</summary>
    public static VramDmaDestination ScrollingSkyDma(int wordOffset) => VramDmaDestination.At(RoomFxVramLayout.ScrollingSkyRows, wordOffset);

    /// <summary>Queued-DMA destination of the mutable HUD rows.</summary>
    public static VramDmaDestination HudMutableRowsDma => VramDmaDestination.At(RoomFxVramLayout.HudMutableRows);
}
