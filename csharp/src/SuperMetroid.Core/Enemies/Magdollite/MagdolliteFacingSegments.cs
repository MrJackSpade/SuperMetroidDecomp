namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>The segments one facing of the Magdollite starts or plays through (#2557), each by its part in the facing's animations.</summary>
internal sealed record MagdolliteFacingSegments
{
    /// <summary>The head's idle bob.</summary>
    public required MagdolliteSegment Idle { get; init; }

    /// <summary>A throw's wind-up.</summary>
    public required MagdolliteSegment Throw { get; init; }

    /// <summary>One gob of lava the throw flings.</summary>
    public required MagdolliteSegment LavaFling { get; init; }

    /// <summary>The throw's finish.</summary>
    public required MagdolliteSegment ThrowFinish { get; init; }

    /// <summary>The head sinking as the submerge starts.</summary>
    public required MagdolliteSegment Submerge { get; init; }

    /// <summary>One pixel of the pillar rising.</summary>
    public required MagdolliteSegment RaisePillar { get; init; }

    /// <summary>The pillar raised, the submerge finishing.</summary>
    public required MagdolliteSegment PillarRaised { get; init; }

    /// <summary>An emerge's start.</summary>
    public required MagdolliteSegment Emerge { get; init; }

    /// <summary>One pixel of the pillar lowering.</summary>
    public required MagdolliteSegment LowerPillar { get; init; }

    /// <summary>The head surfacing as the emerge finishes.</summary>
    public required MagdolliteSegment Surfacing { get; init; }
}
