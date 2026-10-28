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

    /// <summary>
    /// $A8:AF55/$A8:AF67/$A8:AF79: distance thresholds, body animation lists, and body-to-overlay Y offsets for the
    /// nine authored phases; the body shows the narrow pillars and the three- to eight-segment stacks in rise order. The zero-distance setup repeats the first visible pillar. Each following sixteen-pixel rise
    /// selects the next pillar-stack program and moves the overlay down one eight-pixel tile relative to the body.
    /// </summary>
    internal static readonly MagdollitePhaseDefinition[] Phases =
    [
        new(0, MagdolliteProgram.PillarPhase0, 12),
        new(16, MagdolliteProgram.PillarPhase0, 12),
        new(32, MagdolliteProgram.PillarPhase1, 20),
        new(48, MagdolliteProgram.PillarPhase2, 28),
        new(64, MagdolliteProgram.PillarPhase3, 36),
        new(80, MagdolliteProgram.PillarPhase4, 44),
        new(96, MagdolliteProgram.PillarPhase5, 52),
        new(112, MagdolliteProgram.PillarPhase6, 60),
        new(128, MagdolliteProgram.PillarPhase7, 68),
    ];
}
