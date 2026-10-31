namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Bomb Torizo walk instruction's step and facing collision probes (#633).</summary>
internal sealed record TorizoWalkStep
{
    /// <summary>The walking step the cue carries, one of <see cref="BombTorizoMovement.WalkStepCount"/> steps.</summary>
    public required int Step { get; init; }

    /// <summary>The program a wall collision turns into while facing right.</summary>
    public required TorizoBodyProgram CollisionFacingRight { get; init; }

    /// <summary>The program a wall collision turns into while facing left.</summary>
    public required TorizoBodyProgram CollisionFacingLeft { get; init; }
}
