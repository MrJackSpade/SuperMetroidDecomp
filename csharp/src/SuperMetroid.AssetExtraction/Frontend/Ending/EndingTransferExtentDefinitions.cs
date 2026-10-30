namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The byte extents of the ending's character and map transfers the import executes to compose its pictures and sheets (#1467).</summary>
internal static class EndingTransferExtentDefinitions
{
    /// <summary>The bytes of the explosion and flyaway sprite sheet.</summary>
    internal const int ExplosionObjectBytes = 0x6000;

    /// <summary>The bytes of each explosion tilemap fragment and of the post-credits transformation tilemap.</summary>
    internal const int ObjectFragmentBytes = 0x0800;

    /// <summary>The bytes the ending font must cover before its upload.</summary>
    internal const int ObjectFragmentLimit = 0x1000;

    /// <summary>The bytes of the ending font's characters the credits show.</summary>
    internal const int FontCharacterBytes = 0x1000;

    /// <summary>The bytes of the first waiting-Samus frame.</summary>
    internal const int WaitingCharacterBytes = 0x2000;

    /// <summary>The bytes of the post-credits shooting sprite characters.</summary>
    internal const int ShootingCharacterBytes = 0x4000;

    /// <summary>The bytes of the complete 32-by-32 waiting-Samus tilemap.</summary>
    internal const int WaitingTilemapBytes = 0x0800;

    /// <summary>The bytes of the small post-credits transformation characters.</summary>
    internal const int PostCreditsFragmentABytes = 0x0100;
}
