namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One of the loads the ending's floor is read from (#1358); the ending's other pictures are selected by scene state (#1467).</summary>
internal enum EndingUploadKind
{
    /// <summary><c>LoadFloor</c>'s scene: both map copies and the characters of the scene its key names.</summary>
    FloorSceneTiles,

    /// <summary>One of Func118's sixteen flyaway chunks of the Ceres flight's characters or padded map; its key is the chunk.</summary>
    FlyawayChunk,

    /// <summary>The post-credits art, whose reward and shooting characters replace the floor's.</summary>
    CreditsCharacterArt,

    /// <summary>One of the reward landing's queued icon chunks; its key is the chunk.</summary>
    RewardChunk,
}
