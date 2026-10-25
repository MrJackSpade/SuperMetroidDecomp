namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The sound the game-over Baby's animation queues after a frame.</summary>
public enum GameOverBabySound
{
    /// <summary>No sound follows this frame.</summary>
    None,
    /// <summary>The first cry group's cry, queued after the group's first cry frame.</summary>
    FirstCry,
    /// <summary>The second cry group's cry, queued after the group's first cry frame.</summary>
    SecondCry,
    /// <summary>The third cry group's cry, queued after the group's first cry frame.</summary>
    ThirdCry,
}
