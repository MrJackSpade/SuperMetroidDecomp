namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One authored Magdollite rising-body phase.</summary>
internal readonly record struct MagdollitePhaseDefinition(
    int DistanceThreshold,
    MagdolliteProgram BodyProgram,
    int OverlayYOffset);
