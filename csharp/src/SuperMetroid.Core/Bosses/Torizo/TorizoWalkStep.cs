namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Bomb Torizo walk instruction's table offset and facing collision probes (#633).</summary>
internal sealed record TorizoWalkStep
{
    /// <summary>Table offset.</summary>
    public required ushort TableOffset { get; init; }

    /// <summary>The program a wall collision turns into while facing right.</summary>
    public required TorizoBodyProgram CollisionFacingRight { get; init; }

    /// <summary>The program a wall collision turns into while facing left.</summary>
    public required TorizoBodyProgram CollisionFacingLeft { get; init; }
}
