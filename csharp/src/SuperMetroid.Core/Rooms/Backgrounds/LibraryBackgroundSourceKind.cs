namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>What a library-background command reads.</summary>
public enum LibraryBackgroundSourceKind
{
    /// <summary>Nothing: a clear command fills its pages with the blank tile.</summary>
    None,

    /// <summary>A compressed background tilemap, which a decompression stages.</summary>
    Tilemap,

    /// <summary>The tilemap the list's decompressions and fills staged.</summary>
    TilemapStaging,

    /// <summary>The tilemap a boss room's enemies staged before the list runs, as Crocomire's and the Mother Brain tank's lists read it.</summary>
    EnemyTilemapStaging,

    /// <summary>One of the seven scrolling-sky tilemap pages.</summary>
    SkyPage,

    /// <summary>The 48 characters of the Tourian entrance statue's ghost.</summary>
    TourianStatueGhostCharacters,

    /// <summary>The HUD's characters, which Kraid's list copies for layer 3 while his body covers the standard ones.</summary>
    HudCharacters,
}
