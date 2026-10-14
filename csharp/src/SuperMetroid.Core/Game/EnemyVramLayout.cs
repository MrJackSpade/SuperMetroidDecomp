namespace SuperMetroid.Core.Game;

/// <summary>VRAM that enemy and boss routines write directly or through the NMI queue (#630).</summary>
[PpuLayoutBoundary("catalog: enemy and boss VRAM")]
public static class EnemyVramLayout
{
    /// <summary>
    /// VMADD $4000-$4FFF: Kraid's private 64x64 BG2 tilemap selected by BG2SC=$43. It overwrites
    /// the HUD characters and the ordinary BG2 tilemap until Kraid's death restores them.
    /// </summary>
    public static readonly PpuLayoutCell KraidBg2Tilemap = new(nameof(KraidBg2Tilemap), PpuLayoutRegion.Vram,
        KraidBackgroundRomData.LiveBg2TilemapWord, 0x1000, "BG tilemap words (64x64)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $4000-$43FF: the upper visible 32x32 page of Kraid's BG2 (<c>$A7:C874</c>), also the head target.</summary>
    public static readonly PpuLayoutCell KraidTopPage = new(nameof(KraidTopPage), PpuLayoutRegion.Vram,
        KraidBackgroundRomData.LiveBg2TilemapWord, KraidBackgroundRomData.VisiblePageWords, "BG tilemap words (32x32)", KraidBg2Tilemap);

    /// <summary>VMADD $4800-$4BFF: the lower visible 32x32 page of Kraid's BG2 (<c>$A7:C8B6</c>).</summary>
    public static readonly PpuLayoutCell KraidBottomPage = new(nameof(KraidBottomPage), PpuLayoutRegion.Vram,
        0x4800, KraidBackgroundRomData.VisiblePageWords, "BG tilemap words (32x32)", KraidBg2Tilemap);

    /// <summary>
    /// VMADD $4000-$47FF: the standard BG3 characters Kraid's death restores in four $0400-byte
    /// quarters (<c>$A7:C777-$C7EF</c>) after his private BG2 map overwrote them.
    /// </summary>
    public static readonly PpuLayoutCell KraidBg3Restore = new(nameof(KraidBg3Restore), PpuLayoutRegion.Vram,
        KraidBackgroundRomData.StandardBg3VramWord,
        KraidBackgroundRomData.StandardBg3TransferCount * KraidBackgroundRomData.StandardBg3TransferBytes / sizeof(ushort),
        "2-bpp BG3 characters", GameplayVramLayout.HudCharacters);

    /// <summary>
    /// VMADD $4800-$4FFF: the enemy BG2 image that boss routines stage at <c>$7E:2000</c> and
    /// upload over the room's BG2 tilemap (Phantoon, Draygon, Crocomire, Mother Brain).
    /// </summary>
    public static readonly PpuLayoutCell EnemyBg2Tilemap = new(nameof(EnemyBg2Tilemap), PpuLayoutRegion.Vram,
        Assets.EnemyBg2FrameLayout.VramBase, CrocomireDeathState.Bg2WorkingWordCount, "BG tilemap words (32x64)",
        GameplayVramLayout.Bg2Tilemap);

    /// <summary>
    /// VMADD $6000-$7FFF: the OBJ table as Crocomire's skeleton uploads address it, by atlas
    /// tile relative to the OBSEL base (<c>$A4:99CB</c>/<c>$99D9</c>).
    /// </summary>
    public static readonly PpuLayoutCell CrocomireSkeletonTiles = new(nameof(CrocomireSkeletonTiles), PpuLayoutRegion.Vram,
        Assets.CrocomireSkeletonTransferDefinitions.ObselBaseWord, 0x2000, "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>
    /// VMADD $6C00-$7FFF: the enemy graphics-set character area that room loading fills from
    /// each enemy's tile data (<c>$A0:8C6C</c>, VRAM byte $D800 onward).
    /// </summary>
    public static readonly PpuLayoutCell EnemyTiles = new(nameof(EnemyTiles), PpuLayoutRegion.Vram,
        RoomEnemyRomLayout.VramByteBase / sizeof(ushort), 0x1400, "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6D00-$6FFF: the Evir characters Draygon's opening uploads (<c>$A5:8745</c>).</summary>
    public static readonly PpuLayoutCell DraygonEvirTiles = new(nameof(DraygonEvirTiles), PpuLayoutRegion.Vram,
        DraygonIntroPresentationDefinitions.EvirTilesVramByteAddress / sizeof(ushort),
        DraygonIntroPresentationDefinitions.EvirTilesByteCount / sizeof(ushort), "4-bpp OBJ characters", EnemyTiles);

    /// <summary>VMADD $3F00-$3FFF: the Kraid room's backdrop BG characters uploaded from <c>$A7:A716</c>.</summary>
    public static readonly PpuLayoutCell KraidRoomBackgroundTiles = new(nameof(KraidRoomBackgroundTiles), PpuLayoutRegion.Vram,
        KraidBackgroundRomData.RoomBackgroundTileVramWord, KraidBackgroundRomData.RoomBackgroundTileBytes / sizeof(ushort),
        "4-bpp BG characters", GameplayVramLayout.BackgroundCharacters);

    /// <summary>VMADD $50CB-$50D3: the BG1 row receiving the English EMERGENCY title (<c>DrawEmergencyText</c>, $A6:C136).</summary>
    public static readonly PpuLayoutCell CeresEmergencyText = new(nameof(CeresEmergencyText), PpuLayoutRegion.Vram,
        Assets.CeresEscapeOverlayTilemapDefinitions.EmergencyDestination, Assets.CeresEscapeOverlayTilemapDefinitions.Emergency.WordCount,
        "BG tilemap words", GameplayVramLayout.Bg1Tilemap);
}
