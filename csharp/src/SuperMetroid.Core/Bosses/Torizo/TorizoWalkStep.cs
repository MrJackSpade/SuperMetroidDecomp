namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Bomb Torizo walk cue's step and facing collision probes (#633).</summary>
internal sealed record TorizoWalkStep
{
    /// <summary>The walking step the cue carries, one of <see cref="BombTorizoMovement.WalkStepCount"/> steps.</summary>
    public required int Step { get; init; }

    /// <summary>The animation a wall collision turns into while facing right.</summary>
    public required TorizoBodySegment CollisionFacingRight { get; init; }

    /// <summary>The animation a wall collision turns into while facing left.</summary>
    public required TorizoBodySegment CollisionFacingLeft { get; init; }
}
