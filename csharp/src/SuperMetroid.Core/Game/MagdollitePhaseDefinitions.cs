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
    private static readonly MagdollitePhaseDefinition[] Phases =
    [
        new(0x0000, 0xaddc, 0x000c),
        new(0x0010, 0xaddc, 0x000c),
        new(0x0020, 0xade2, 0x0014),
        new(0x0030, 0xade8, 0x001c),
        new(0x0040, 0xadee, 0x0024),
        new(0x0050, 0xadf4, 0x002c),
        new(0x0060, 0xadfa, 0x0034),
        new(0x0070, 0xae00, 0x003c),
        new(0x0080, 0xae06, 0x0044),
    ];

    internal static MagdollitePhaseDefinition Phase(int index)
    {
        if ((uint)index >= Phases.Length)
            throw new InvalidDataException(
                $"Magdollite phase {index} is outside the nine authored records.");
        return Phases[index];
    }
}
