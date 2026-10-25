namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The byte extents of the installed ending artwork the screens show (#1416).</summary>
public static class EndingArtworkExtentDefinitions
{
    /// <summary>The bytes of one complete Mode 7 floor lane, also the full reward character sheet.</summary>
    public const int FloorBytes = 0x4000;

    /// <summary>The bytes of each of the flyaway's sixteen floor chunks.</summary>
    public const int FlyawayChunkBytes = 0x0800;

    /// <summary>The flyaway map bytes kept before the rest of the map is filled with the blank tile.</summary>
    public const int FlyawayMapDataBytes = 0x0300;

    /// <summary>The blank floor tile that fills the flyaway map after its kept bytes.</summary>
    public const byte FlyawayBlankTile = 0x8c;

    /// <summary>The bytes of the explosion and flyaway sprite sheet.</summary>
    public const int ExplosionObjectBytes = 0x6000;

    /// <summary>The bytes of each explosion tilemap fragment and of the post-credits transformation tilemap.</summary>
    public const int ObjectFragmentBytes = 0x0800;

    /// <summary>The bytes the ending font must cover before its upload.</summary>
    public const int ObjectFragmentLimit = 0x1000;

    /// <summary>The bytes of the ending font's characters the credits show.</summary>
    public const int FontCharacterBytes = 0x1000;

    /// <summary>The bytes of the first waiting-Samus frame.</summary>
    public const int WaitingCharacterBytes = 0x2000;

    /// <summary>The bytes of the post-credits shooting sprite characters.</summary>
    public const int ShootingCharacterBytes = 0x4000;

    /// <summary>The bytes of the complete 32-by-32 waiting-Samus tilemap.</summary>
    public const int WaitingTilemapBytes = 0x0800;

    /// <summary>The bytes of the small post-credits transformation characters.</summary>
    public const int PostCreditsFragmentABytes = 0x0100;
}
