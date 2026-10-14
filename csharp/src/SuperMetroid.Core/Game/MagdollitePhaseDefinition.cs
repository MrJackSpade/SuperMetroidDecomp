namespace SuperMetroid.Core.Game;

/// <summary>One authored Magdollite rising-body phase.</summary>
internal readonly record struct MagdollitePhaseDefinition(
    ushort DistanceThreshold,
    ushort BodyInstructionList,
    ushort OverlayYOffset);
