namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One of the ending's graphics transfers, which its captured image replays in order (#1358).</summary>
internal enum EndingUploadKind
{
    /// <summary><c>LoadMode7</c>'s scene: both map copies and the characters of the scene its key names.</summary>
    Mode7Scene,

    /// <summary>One of Func118's sixteen flyaway chunks of the Ceres flight's characters or padded map; its key is the chunk.</summary>
    FlyawayChunk,

    /// <summary>The escape cloud OBJ sheet.</summary>
    EscapeClouds,

    /// <summary><c>$8B:D8C1</c>'s explosion and flyaway OBJ sheet.</summary>
    ExplosionObjects,

    /// <summary>One authored explosion OBJ fragment; its key is the fragment.</summary>
    ObjectFragment,

    /// <summary>The ending font's OBJ characters loaded after the explosion sheet.</summary>
    EndingFont,

    /// <summary>The credits BG font, loaded after the staff handoff's full clear.</summary>
    CreditsFont,

    /// <summary>The waiting, shooting and reward characters with the post-credits map fragments.</summary>
    CreditsCharacterArt,

    /// <summary>The credits scroll's circular tilemap, with the words it uploaded.</summary>
    CreditsTilemap,

    /// <summary>A result-panel staging tilemap uploaded to the text tilemap its key names, with the words it uploaded.</summary>
    TextTilemap,

    /// <summary><c>$8B:E504</c>'s copy of the staging map into the lower post-shot screen, with the words it copied.</summary>
    PostShotLowerTilemap,

    /// <summary>One of the reward landing's queued icon chunks; its key is the chunk.</summary>
    RewardChunk,

    /// <summary>One of the post-shot sequence's subtitle and logo transfers; its key is the transfer.</summary>
    PostShotTransfer,
}
