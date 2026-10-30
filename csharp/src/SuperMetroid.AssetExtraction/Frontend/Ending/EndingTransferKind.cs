namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The ending's character and map transfers the import executes to compose its sprite pages (#1467).</summary>
internal enum EndingTransferKind
{
    /// <summary>The escape cloud OBJ sheet.</summary>
    EscapeClouds,

    /// <summary>The explosion and flyaway OBJ sheet.</summary>
    ExplosionObjects,

    /// <summary>One explosion map, which shares the OBJ page's last characters.</summary>
    ExplosionMap,

    /// <summary>The ending font's OBJ characters loaded after the explosion sheet.</summary>
    EndingFont,

    /// <summary>The waiting, shooting and reward characters with the post-credits maps.</summary>
    CreditsCharacterArt,

    /// <summary>One of the post-shot's subtitle and logo parts.</summary>
    PostShotPart,
}
