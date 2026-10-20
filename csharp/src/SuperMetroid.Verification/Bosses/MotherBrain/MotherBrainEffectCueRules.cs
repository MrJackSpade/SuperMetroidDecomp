using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>How Mother Brain's effect programs match their native lists: each command and the cue it becomes.</summary>
internal static class MotherBrainEffectCueRules
{
    /// <summary>Each command the lists use and how the cue it becomes is checked.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, NativeCueCommand<MotherBrainEffectCue>> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Command(MotherBrainEffectCueKind.Delete)),
        [NativeEnemyProjectileInstruction.ClearPreInstruction] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Command(MotherBrainEffectCueKind.ClearPreInstruction)),
        [NativeEnemyProjectileInstruction.PreInstructionInY] = NativeCueCommand<MotherBrainEffectCue>.Of<MotherBrainEffectCue.SetPreInstruction>(2, (operands, cue, at) =>
            AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), EnemyProjectilePreInstructionAccess.Word(cue.PreInstruction), $"{at}: pre-instruction")),
        [NativeEnemyProjectileInstruction.XYRadiusInY] = NativeCueCommand<MotherBrainEffectCue>.Of<MotherBrainEffectCue.SetRadius>(2, (operands, cue, at) =>
            AssertEqual(new Extent16<RoomPixels>(operands[0], operands[1]), cue.Radius, $"{at}: X and Y radii")),
        [NativeEnemyProjectileInstruction.UsePalette0_Duplicate] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Command(MotherBrainEffectCueKind.UsePaletteZero)),
        [NativeEnemyProjectileInstruction.MotherBrainsDrool_MoveDownCPixels] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Command(MotherBrainEffectCueKind.DropDrool)),
        [NativeEnemyProjectileInstruction.MotherBrainPurpleBreath_Inactive] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Command(MotherBrainEffectCueKind.EndSmallPurpleBreath)),
    };

    /// <summary>The oracle rules for Mother Brain's effect programs.</summary>
    internal static NativeEnemyProjectileCueRules<MotherBrainEffectCue> Rules { get; } = NativeCueCommand<MotherBrainEffectCue>.Rules(
        Commands, word => (NativeEnemyProjectileInstruction)word,
        cue => cue is MotherBrainEffectCue.Command { Kind: MotherBrainEffectCueKind.Delete });
}
