namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Compiled physical phase definitions for Magdollite's rising body.</summary>
internal static class MagdollitePhaseDefinitions
{
    /// <summary>$A8:B220, maximum whole-pixel rise before Magdollite reaches its apex.</summary>
    internal const ushort MaximumRise = 108;

    /// <summary>
    /// Whether an arm in each enemy slot grows one final phase on reaching <see cref="MaximumRise"/>. The cartridge's
    /// <c>.doneGrowing</c> at $A8:B259 is reached from the maximum-rise test before Y is loaded from the growth
    /// index, so it compares the rise with the word at <c>MagdolliteArmHeightThreshold + EnemyIndex</c>
    /// ($A8:AF55 + 64*slot), using the Y the enemy main loop left (<c>LDY EnemyIndex</c> at $A0:9073). The
    /// threshold table holds nine words, so every slot but zero reads the bank-$A8 bytes that follow it. The rise is
    /// then 108 to 112 pixels, and each of those words gives the same answer for every such rise.
    /// </summary>
    internal static readonly bool[] s_apexGrowsByEnemySlot =
    [
        true, false, true, true, false, false, false, true,
        true, true, true, false, false, false, false, true,
        true, true, true, true, true, false, true, false,
        true, true, true, true, true, false, true, false,
    ];

    /// <summary>Whether an arm in <paramref name="enemySlot"/> grows one final phase on reaching the maximum rise.</summary>
    internal static bool ApexGrowsFinalPhase(int enemySlot)
    {
        if ((uint)enemySlot >= (uint)s_apexGrowsByEnemySlot.Length)
            throw new InvalidDataException(
                $"Magdollite enemy slot {enemySlot} is outside the {s_apexGrowsByEnemySlot.Length} enemy slots.");
        return s_apexGrowsByEnemySlot[enemySlot];
    }

    /// <summary>The narrow pillars and the three- to eight-segment stacks the rising body shows, in rise order.</summary>
    private static readonly MagdolliteProgram[] PillarPrograms =
    [
        MagdolliteProgram.PillarPhase0, MagdolliteProgram.PillarPhase1, MagdolliteProgram.PillarPhase2, MagdolliteProgram.PillarPhase3,
        MagdolliteProgram.PillarPhase4, MagdolliteProgram.PillarPhase5, MagdolliteProgram.PillarPhase6, MagdolliteProgram.PillarPhase7,
    ];

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
        // sixteen-pixel rise selects the next pillar-stack program and moves
        // the overlay down one eight-pixel tile relative to the body.
        int visiblePhase = Math.Max(0, index - 1);
        return new((ushort)(16 * index),
            PillarPrograms[visiblePhase],
            (ushort)(12 + 8 * visiblePhase));
    }
}
