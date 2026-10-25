using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The selected Mother Brain room colours; a family an older override predates is null.</summary>
internal sealed record MotherBrainRoomColours
{
    /// <summary>The fourteen flash rows.</summary>
    public required RgbColor[][] Flash { get; init; }

    /// <summary>The final room colours.</summary>
    public required RgbColor[] FinalRoom { get; init; }

    /// <summary>The phase-two attack colours.</summary>
    public required RgbColor[] PhaseTwoAttack { get; init; }

    /// <summary>The phase-two rear-leg colours.</summary>
    public required RgbColor[] PhaseTwoRearLeg { get; init; }

    /// <summary>The room-entry glass shard colours, or null when they are inherited from stock.</summary>
    public required RgbColor[]? InitialGlassShard { get; init; }

    /// <summary>The room-entry tube projectile colours, or null when they are inherited from stock.</summary>
    public required RgbColor[]? InitialTubeProjectile { get; init; }

    /// <summary>The room-light recovery frames, or null when they are inherited from stock.</summary>
    public required RgbColor[][]? RecoveryLights { get; init; }
}
