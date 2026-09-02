namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge addresses and pointers consumed by the pause-menu renderer.</summary>
internal static class PauseMenuRomData
{
    /// <summary>Blank equipment tilemap patch at $82:C01A.</summary>
    public const ushort BlankEquipmentTilemap = 0xc01a;

    /// <summary>Pause-screen BG character data at $B6:8000.</summary>
    public const int BackgroundTiles = 0xb68000;

    /// <summary>Pause-screen OBJ character data at $B6:C000.</summary>
    public const int ObjectTiles = 0xb6c000;

    /// <summary>Samus pause-screen OBJ character data at $9A:B200.</summary>
    public const int SamusObjectTiles = 0x9ab200;

    /// <summary>Pause-screen BG2 tilemap at $B6:E000.</summary>
    public const int BackgroundTilemap = 0xb6e000;

    /// <summary>Pause-screen button tilemap at $B6:E400.</summary>
    public const int ButtonTilemap = 0xb6e400;

    /// <summary>Pause-screen equipment tilemap at $B6:E800.</summary>
    public const int EquipmentTilemap = 0xb6e800;

    /// <summary>Pause-screen CGRAM palette at $B6:F000.</summary>
    public const int Palette = 0xb6f000;

    /// <summary>Equipment-set lookup table at $82:B257.</summary>
    public const int EquipmentSetTable = 0x82b257;

    /// <summary>Equipment tilemap-patch pointer table at $82:B25F.</summary>
    public const int EquipmentTilemapPatchPointerTable = 0x82b25f;

    /// <summary>Area map-tilemap long-pointer table at $82:964A.</summary>
    public const int AreaMapTilemapPointerTable = 0x82964a;

    /// <summary>Area-map label pointer table at $82:965F.</summary>
    public const int AreaMapLabelPointerTable = 0x82965f;

    /// <summary>Area explored-map data pointer table at $82:9717.</summary>
    public const int AreaMapDataPointerTable = 0x829717;

    /// <summary>Pause-menu spritemap-pointer table at $82:C569.</summary>
    public const int SpritemapPointerTable = 0x82c569;

    /// <summary>Item-selector animation variant pointer at $82:C0DA.</summary>
    public const int ItemSelectorAnimationVariantPointer = 0x82c0da;

    /// <summary>Item-selector animation list pointer at $82:C0EC.</summary>
    public const int ItemSelectorAnimationPointer = 0x82c0ec;

    /// <summary>Pause-screen selected-item spritemap pointer at $82:C100.</summary>
    public const int SelectedItemSpritemapPointer = 0x82c100;

    /// <summary>Item-selector animation timer byte at $82:C10C.</summary>
    public const int ItemSelectorAnimationTimer = 0x82c10c;

    /// <summary>Equipment-selector position-list pointer table at $82:C18E.</summary>
    public const int EquipmentSelectorPositionPointerTable = 0x82c18e;

    /// <summary>Equipment-selector base spritemap table pointer at $82:C1E8.</summary>
    public const int EquipmentSelectorBaseTablePointer = 0x82c1e8;
}
