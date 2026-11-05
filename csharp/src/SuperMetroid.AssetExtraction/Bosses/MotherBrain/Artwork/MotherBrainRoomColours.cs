using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The selected Mother Brain room colours.</summary>
internal sealed record MotherBrainRoomColours
{
    /// <summary>The fourteen flash frames.</summary>
    public required RgbColor[][] Flash { get; init; }

    /// <summary>The final room colours.</summary>
    public required RgbColor[] FinalRoom { get; init; }

    /// <summary>The phase-two attack colours.</summary>
    public required RgbColor[] PhaseTwoAttack { get; init; }

    /// <summary>The phase-two rear-leg colours.</summary>
    public required RgbColor[] PhaseTwoRearLeg { get; init; }

    /// <summary>The room-entry glass shard colours.</summary>
    public required RgbColor[] InitialGlassShard { get; init; }

    /// <summary>The room-entry tube projectile colours.</summary>
    public required RgbColor[] InitialTubeProjectile { get; init; }

    /// <summary>The room-light recovery frames.</summary>
    public required RgbColor[][] RecoveryLights { get; init; }
}
