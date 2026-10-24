namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The selected Mother Brain room colours; a family an older override predates is null.</summary>
internal sealed record MotherBrainRoomColours
{
    /// <summary>The fourteen flash rows.</summary>
    public required Bgr555[][] Flash { get; init; }

    /// <summary>The final room colours.</summary>
    public required Bgr555[] FinalRoom { get; init; }

    /// <summary>The phase-two attack colours.</summary>
    public required Bgr555[] PhaseTwoAttack { get; init; }

    /// <summary>The phase-two rear-leg colours.</summary>
    public required Bgr555[] PhaseTwoRearLeg { get; init; }

    /// <summary>The room-entry glass shard colours, or null when they are inherited from stock.</summary>
    public required Bgr555[]? InitialGlassShard { get; init; }

    /// <summary>The room-entry tube projectile colours, or null when they are inherited from stock.</summary>
    public required Bgr555[]? InitialTubeProjectile { get; init; }

    /// <summary>The room-light recovery frames, or null when they are inherited from stock.</summary>
    public required Bgr555[][]? RecoveryLights { get; init; }
}
