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
    /// Words at <c>MagdolliteArmHeightThreshold + EnemyIndex</c> ($A8:AF55 + 64*slot).
    /// <c>.doneGrowing</c> at $A8:B259 is reached from the maximum-rise test before Y is
    /// loaded from the growth index, so it compares against the Y the enemy main loop
    /// left: <c>LDY EnemyIndex</c> at $A0:9073. The table holds nine words; every slot
    /// but zero therefore reads the bank-$A8 bytes that follow it.
    /// </summary>
    private static readonly ushort[] s_apexThresholdsByEnemySlot =
    [
        0x0000, 0x20af, 0xae9d, 0xbd0f, 0x0f86, 0x0f96, 0x0a0a, 0xfc7e,
        0xa0af, 0xbf0e, 0xa9b3, 0x0fb0, 0x1d10, 0x54ae, 0x30af, 0xd0b1,
        0xae60, 0xbc0d, 0x9d00, 0xd00f, 0xf781, 0x2004, 0xfc21, 0x01f4,
        0xf881, 0x0004, 0xf800, 0xf821, 0xd081, 0x7d00, 0x9000, 0x01c0,
    ];

    /// <summary>The threshold word <c>.doneGrowing</c> reads after the maximum rise.</summary>
    internal static ushort ApexThreshold(int enemySlot)
    {
        if ((uint)enemySlot >= (uint)s_apexThresholdsByEnemySlot.Length)
            throw new InvalidDataException(
                $"Magdollite enemy slot {enemySlot} is outside the {s_apexThresholdsByEnemySlot.Length} enemy slots.");
        return s_apexThresholdsByEnemySlot[enemySlot];
    }

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