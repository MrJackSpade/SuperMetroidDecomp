namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The cries the game-over Baby's animation queues, one as the first pose of each cry ends.</summary>
public enum GameOverBabySound
{
    /// <summary>The Baby's high cry, queued by the first cry group after its first cry frame.</summary>
    HighCry,
    /// <summary>The Baby's hitched cry, queued by the second cry group after its first cry frame.</summary>
    HitchedCry,
    /// <summary>The Baby's low cry, queued by the third cry group after its first cry frame.</summary>
    LowCry,
}
