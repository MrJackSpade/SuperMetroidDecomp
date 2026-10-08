namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge addresses and pointers consumed by the pause-menu renderer.</summary>
internal static class PauseMenuRomData
{

    /// <summary>Pause-screen BG2 tilemap at $B6:E000.</summary>
    public const int BackgroundTilemap = 0xb6e000;

    /// <summary>Pause-screen button tilemap at $B6:E400.</summary>
    public const int ButtonTilemap = 0xb6e400;

    /// <summary>Pause-screen equipment tilemap at $B6:E800.</summary>
    public const int EquipmentTilemap = 0xb6e800;

    /// <summary>Equipment tilemap-patch pointer table at $82:B25F.</summary>
    public const int EquipmentTilemapPatchPointerTable = 0x82b25f;

    /// <summary>Area-map label pointer table at $82:965F.</summary>
    public const int AreaMapLabelPointerTable = 0x82965f;

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
