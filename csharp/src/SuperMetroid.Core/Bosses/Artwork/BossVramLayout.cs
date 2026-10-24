using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>VRAM that boss routines write directly or through the NMI queue (#630, #1395).</summary>
[PpuLayoutBoundary("catalog: boss VRAM")]
public static class BossVramLayout
{
    /// <summary>
    /// VMADD $4000-$47FF: the standard BG3 characters Kraid's death restores in four $0400-byte
    /// quarters (<c>$A7:C777-$C7EF</c>) after his private BG2 map overwrote them.
    /// </summary>
    public static readonly PpuLayoutCell KraidBg3Restore = new(nameof(KraidBg3Restore), PpuLayoutRegion.Vram,
        KraidBackgroundRomData.StandardBg3VramWord,
        KraidBackgroundRomData.StandardBg3QuarterCount * KraidBackgroundRomData.StandardBg3QuarterBytes / sizeof(ushort),
        "2-bpp BG3 characters", GameplayVramLayout.HudCharacters);

    /// <summary>VMADD $6D00-$6FFF: the Evir characters Draygon's opening uploads (<c>$A5:8745</c>).</summary>
    public static readonly PpuLayoutCell DraygonEvirTiles = new(nameof(DraygonEvirTiles), PpuLayoutRegion.Vram,
        DraygonIntroPresentationDefinitions.EvirTilesVramByteAddress / sizeof(ushort),
        DraygonIntroPresentationDefinitions.EvirTilesByteCount / sizeof(ushort), "4-bpp OBJ characters", EnemyVramLayout.EnemyTiles);

    /// <summary>VMADD $3F00-$3FFF: the Kraid room's backdrop BG characters uploaded from <c>$A7:A716</c>.</summary>
    public static readonly PpuLayoutCell KraidRoomBackgroundTiles = new(nameof(KraidRoomBackgroundTiles), PpuLayoutRegion.Vram,
        KraidBackgroundRomData.RoomBackgroundTileVramWord, KraidBackgroundRomData.RoomBackgroundTileBytes / sizeof(ushort),
        "4-bpp BG characters", GameplayVramLayout.BackgroundCharacters);
}
