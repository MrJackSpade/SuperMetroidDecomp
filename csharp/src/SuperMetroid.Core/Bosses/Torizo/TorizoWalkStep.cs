namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Bomb Torizo walk instruction's table offset and facing collision probes (#633).</summary>
internal sealed record TorizoWalkStep
{
    /// <summary>Table offset.</summary>
    public required ushort TableOffset { get; init; }

    /// <summary>Collision facing right.</summary>
    public required ushort CollisionFacingRight { get; init; }

    /// <summary>Collision facing left.</summary>
    public required ushort CollisionFacingLeft { get; init; }
}
