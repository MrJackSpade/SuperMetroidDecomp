namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One authored Magdollite rising-body phase.</summary>
internal readonly record struct MagdollitePhaseDefinition(
    ushort DistanceThreshold,
    MagdolliteProgram BodyProgram,
    ushort OverlayYOffset);
