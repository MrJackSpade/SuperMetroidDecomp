namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Compiled physical phase definitions for Magdollite's rising body.</summary>
internal static class MagdollitePhaseDefinitions
{
    /// <summary>
    /// Maximum whole-pixel rise before Magdollite reaches its apex; whether the arm grows a final phase there is
    /// <see cref="MagdolliteApexGrowth"/>'s.
    /// </summary>
    internal const int MaximumRise = 108;

    /// <summary>
    /// The distance thresholds, body animation lists, and body-to-overlay Y offsets for the
    /// nine authored phases; the body shows the narrow pillars and the three- to eight-segment stacks in rise order. The zero-distance setup repeats the first visible pillar. Each following sixteen-pixel rise
    /// selects the next pillar-stack animation and moves the overlay down one eight-pixel tile relative to the body.
    /// </summary>
    internal static readonly MagdollitePhaseDefinition[] Phases =
    [
        new(0, MagdolliteSegment.PillarPhase0, 12),
        new(16, MagdolliteSegment.PillarPhase0, 12),
        new(32, MagdolliteSegment.PillarPhase1, 20),
        new(48, MagdolliteSegment.PillarPhase2, 28),
        new(64, MagdolliteSegment.PillarPhase3, 36),
        new(80, MagdolliteSegment.PillarPhase4, 44),
        new(96, MagdolliteSegment.PillarPhase5, 52),
        new(112, MagdolliteSegment.PillarPhase6, 60),
        new(128, MagdolliteSegment.PillarPhase7, 68),
    ];
}
