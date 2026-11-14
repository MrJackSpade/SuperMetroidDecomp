namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The cries the game-over Baby's animation queues, one as the first pose of each cry ends.</summary>
public enum GameOverBabySound
{
    /// <summary>The first cry group's cry, queued after the group's first cry frame.</summary>
    FirstCry,
    /// <summary>The second cry group's cry, queued after the group's first cry frame.</summary>
    SecondCry,
    /// <summary>The third cry group's cry, queued after the group's first cry frame.</summary>
    ThirdCry,
}
