namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One stage of a Magdollite arm's rise (#2343): what it shows and the stages above and below it.</summary>
/// <param name="DistanceThreshold">The rise, in whole pixels, past which the arm grows into this stage.</param>
/// <param name="BodyAnimation">The pillar the arm's body shows.</param>
/// <param name="OverlayYOffset">How far above the body the tracking overlay rides, in pixels.</param>
/// <param name="Higher">The stage the arm grows into next; none for its last stage.</param>
/// <param name="Lower">The stage the arm sinks back into; none for the setup.</param>
internal readonly record struct MagdollitePhaseDefinition(
    int DistanceThreshold,
    MagdolliteSegment BodyAnimation,
    int OverlayYOffset,
    MagdolliteRiseStage? Higher,
    MagdolliteRiseStage? Lower);
