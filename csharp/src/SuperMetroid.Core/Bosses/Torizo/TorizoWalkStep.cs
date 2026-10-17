namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Bomb Torizo walk instruction's table offset and facing collision probes (#633).</summary>
internal sealed record TorizoWalkStep
{
    /// <summary>The walk-velocity table offset the walking cue carries: an even offset from zero to 38.</summary>
    public required int TableOffset { get; init; }

    /// <summary>The program a wall collision turns into while facing right.</summary>
    public required TorizoBodyProgram CollisionFacingRight { get; init; }

    /// <summary>The program a wall collision turns into while facing left.</summary>
    public required TorizoBodyProgram CollisionFacingLeft { get; init; }
}
