namespace SuperMetroid.Core.Game;

/// <summary>One authored Magdollite rising-body phase.</summary>
internal readonly record struct MagdollitePhaseDefinition(
    ushort DistanceThreshold,
    ushort BodyInstructionList,
    ushort OverlayYOffset);

/// <summary>Compiled physical phase definitions for Magdollite's rising body.</summary>
internal static class MagdollitePhaseDefinitions
{
    /// <summary>$A8:B220, maximum whole-pixel rise before Magdollite reaches its apex.</summary>
    internal const ushort MaximumRise = 108;

    /// <summary>
    /// $A8:AF55/$A8:AF67/$A8:AF79: distance thresholds, body animation lists,
    /// and body-to-overlay Y offsets for the nine authored phases.
    /// </summary>
    internal static MagdollitePhaseDefinition Phase(int index)
    {
        if ((uint)index >= 9)
            throw new InvalidDataException(
                $"Magdollite phase {index} is outside the nine authored records.");
        // The zero-distance setup repeats the first visible pillar. Each following
        // sixteen-pixel rise selects the next six-byte frame/sleep program and moves
        // the overlay down one eight-pixel tile relative to the body.
        int visiblePhase = Math.Max(0, index - 1);
        return new((ushort)(16 * index),
            (ushort)(MagdolliteInstructionProgramDefinitions.PillarPhase0 + 6 * visiblePhase),
            (ushort)(12 + 8 * visiblePhase));
    }
}