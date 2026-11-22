namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One stage of a Magdollite arm's rise (#2343): what it shows.</summary>
/// <param name="DistanceThreshold">The rise, in whole pixels, past which the arm grows into this stage.</param>
/// <param name="BodyAnimation">The pillar the arm's body shows.</param>
/// <param name="OverlayYOffset">How far above the body the tracking overlay rides, in pixels.</param>
internal readonly partial record struct MagdollitePhaseDefinition(
    int DistanceThreshold,
    MagdolliteBodySegment BodyAnimation,
    int OverlayYOffset);
