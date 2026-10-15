namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Observable result of one bank-$94 room-block movement scan.</summary>
public readonly record struct BlockMoveResult(
    int AcceptedDisplacement,
    bool Collided,
    SolidEnemyCollisionResult? EnemyCollision = null)
{
    /// <summary>
    /// True when bank $90 selected downward movement and neither terrain nor a solid enemy
    /// stopped it. This is the condition that publishes solid-vertical result two
    /// (falling); an unobstructed upward carry instead publishes result zero (no change).
    /// </summary>
    public bool IsUnobstructedDownwardMovement =>
        !Collided && AcceptedDisplacement >= 0;

    /// <summary>
    /// The result when bank $90 skips <c>MoveSamus_Horizontally</c> entirely, as
    /// <c>Samus_Jumping_Movement</c> does with no direction held ($90:902B). No probe,
    /// block scan, or post-move slope alignment runs.
    /// </summary>
    public static BlockMoveResult NotMoved => new(0, false);
}
