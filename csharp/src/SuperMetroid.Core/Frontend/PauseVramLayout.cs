using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The pause screen's VRAM allocation (#630). Pause keeps its own PPU image; its BG3 plane
/// retains the gameplay HUD rows at the gameplay HUD tilemap's location.
/// </summary>
[PpuLayoutBoundary("catalog: pause screen VRAM")]
public static class PauseVramLayout
{
    /// <summary>VMADD $0000-$0FFF: the shared map characters.</summary>
    public static readonly PpuLayoutCell MapCharacters = new(nameof(MapCharacters), PpuLayoutRegion.Vram,
        0x0000, Assets.MapTileAtlasFormat.ByteCount / sizeof(ushort), "4-bpp BG characters (map atlas)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $1000-$1FFF: the pause interface characters uploaded from the pause UI atlas.</summary>
    public static readonly PpuLayoutCell InterfaceCharacters = new(nameof(InterfaceCharacters), PpuLayoutRegion.Vram,
        Assets.PauseTileAtlasFormat.DestinationByte / sizeof(ushort), Assets.PauseTileAtlasFormat.ByteCount / sizeof(ushort),
        "4-bpp BG characters (pause interface)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $2000-$2FFF (byte $4000): the map-object OBJ characters uploaded from <c>$B6:C000</c>.</summary>
    public static readonly PpuLayoutCell ObjectCharacters = new(nameof(ObjectCharacters), PpuLayoutRegion.Vram,
        Assets.MapSpriteFormat.PauseDestination / sizeof(ushort), Assets.MapSpriteFormat.ByteCount / sizeof(ushort),
        "4-bpp OBJ characters (map objects)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $3000-$37FF: the map and equipment pages' BG1 64x32 tilemap.</summary>
    public static readonly PpuLayoutCell Bg1Tilemap = new(nameof(Bg1Tilemap), PpuLayoutRegion.Vram,
        PauseMenuLayout.Bg1TilemapWord, 0x0800, "BG tilemap words (64x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $3800-$3BFF: the pause frame's BG2 32x32 tilemap.</summary>
    public static readonly PpuLayoutCell Bg2Tilemap = new(nameof(Bg2Tilemap), PpuLayoutRegion.Vram,
        PauseMenuLayout.Bg2TilemapWord, 0x0400, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $3B20-$3B5F: the two mutable button-label rows the NMI DMA queued from WRAM <c>$7E:3640</c>.</summary>
    public static readonly PpuLayoutCell ButtonRows = new(nameof(ButtonRows), PpuLayoutRegion.Vram,
        0x3b20, 0x0040, "BG tilemap words (two 32-word rows)", Bg2Tilemap);

    /// <summary>VMADD $5800-$5BFF: the gameplay BG3 tilemap pause keeps resident (SetupPPUForPauseMenu sets BG3SC=$58).</summary>
    public static readonly PpuLayoutCell RetainedHudTilemap = new(nameof(RetainedHudTilemap), PpuLayoutRegion.Vram,
        GameplayVramLayout.HudTilemapWord, 0x0400, "BG3 tilemap words (32x32)", GameplayVramLayout.HudTilemap);

    /// <summary>VMADD $5820-$587F: the mutable HUD rows whose accepted NMI DMA pause mirrors.</summary>
    public static readonly PpuLayoutCell AcceptedHudRows = new(nameof(AcceptedHudRows), PpuLayoutRegion.Vram,
        HudState.VramDestination, HudState.MutableByteCount / sizeof(ushort), "BG3 tilemap words", RetainedHudTilemap);

    /// <summary>VMADD $5880-$5FFF: the FX rows <c>QueueClearingOfFxTilemap</c> ($80:A211) fills below the HUD.</summary>
    public static readonly PpuLayoutCell Bg3FxRows = new(nameof(Bg3FxRows), PpuLayoutRegion.Vram,
        0x5880, 0x0780, "BG3 tilemap words", GameplayVramLayout.HudTilemap);
}

/// <summary>Typed view of the pause screen's VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: pause screen VRAM")]
public readonly struct PauseVram(SnesVram vram)
{
    /// <summary>The shared map characters.</summary>
    public VramRegion MapCharacters => new(vram, PauseVramLayout.MapCharacters);

    /// <summary>The pause interface characters.</summary>
    public VramRegion InterfaceCharacters => new(vram, PauseVramLayout.InterfaceCharacters);

    /// <summary>The map-object OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, PauseVramLayout.ObjectCharacters);

    /// <summary>The pause frame's BG2 tilemap.</summary>
    public VramRegion Bg2Tilemap => new(vram, PauseVramLayout.Bg2Tilemap);

    /// <summary>The map/equipment BG1 tilemap.</summary>
    public VramRegion Bg1Tilemap => new(vram, PauseVramLayout.Bg1Tilemap);

    /// <summary>The button-label rows.</summary>
    public VramRegion ButtonRows => new(vram, PauseVramLayout.ButtonRows);

    /// <summary>The retained gameplay BG3 tilemap.</summary>
    public VramRegion RetainedHudTilemap => new(vram, PauseVramLayout.RetainedHudTilemap);

    /// <summary>The mutable HUD rows.</summary>
    public VramRegion AcceptedHudRows => new(vram, PauseVramLayout.AcceptedHudRows);

    /// <summary>The cleared FX rows.</summary>
    public VramRegion Bg3FxRows => new(vram, PauseVramLayout.Bg3FxRows);
}
