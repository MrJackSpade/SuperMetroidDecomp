namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Bomb Torizo walk cue's velocity and facing collision probes (#633, #1891).</summary>
internal sealed record TorizoWalkStep
{
    /// <summary>The whole pixels the step moves the Torizo, negative to the left.</summary>
    public required int Velocity { get; init; }

    /// <summary>The animation a wall collision turns into while facing right.</summary>
    public required TorizoAnimation CollisionFacingRight { get; init; }

    /// <summary>The animation a wall collision turns into while facing left.</summary>
    public required TorizoAnimation CollisionFacingLeft { get; init; }
}
