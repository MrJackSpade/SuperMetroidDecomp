using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>How Mother Brain's effect programs match their native lists: each command and the cue it becomes.</summary>
internal static class MotherBrainEffectCueRules
{
    /// <summary>Each command the lists use, the cue it becomes and its operand byte count.</summary>
    private static readonly Dictionary<EnemyProjectileInstruction, (MotherBrainEffectCueKind Kind, int OperandBytes)> Commands = new()
    {
        [EnemyProjectileInstruction.Delete] = (MotherBrainEffectCueKind.Delete, 0),
        [EnemyProjectileInstruction.ClearPreInstruction] = (MotherBrainEffectCueKind.ClearPreInstruction, 0),
        [EnemyProjectileInstruction.PreInstructionInY] = (MotherBrainEffectCueKind.SetPreInstruction, 2),
        [EnemyProjectileInstruction.XYRadiusInY] = (MotherBrainEffectCueKind.SetRadius, 2),
        [EnemyProjectileInstruction.UsePalette0_Duplicate] = (MotherBrainEffectCueKind.UsePaletteZero, 0),
        [EnemyProjectileInstruction.MotherBrainsDrool_MoveDownCPixels] = (MotherBrainEffectCueKind.DropDrool, 0),
        [EnemyProjectileInstruction.MotherBrainPurpleBreath_Inactive] = (MotherBrainEffectCueKind.EndSmallPurpleBreath, 0),
    };

    /// <summary>The oracle rules for Mother Brain's effect programs.</summary>
    internal static NativeEnemyProjectileCueRules<MotherBrainEffectCue> Rules { get; } = new(
        word => Commands.TryGetValue((EnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            AssertEqual(Commands[(EnemyProjectileInstruction)word].Kind, cue.Kind, $"{at}: cue");
            if (cue.Kind == MotherBrainEffectCueKind.SetPreInstruction)
                AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), (ushort)cue.PreInstruction, $"{at}: pre-instruction");
            else if (cue.Kind == MotherBrainEffectCueKind.SetRadius)
                AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), cue.Value, $"{at}: packed radii");
        },
        cue => cue.Kind == MotherBrainEffectCueKind.Delete);
}
