using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>How the downward gate's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class DownwardGateProjectileCueRules
{
    /// <summary>Each command the gate's lists use, the cue it becomes and its operand byte count.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, (DownwardGateProjectileCueKind Kind, int OperandBytes)> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = (DownwardGateProjectileCueKind.Delete, 0),
        [NativeEnemyProjectileInstruction.ClearPreInstruction] = (DownwardGateProjectileCueKind.ClearPreInstruction, 0),
        [NativeEnemyProjectileInstruction.PreInstructionInY] = (DownwardGateProjectileCueKind.SetPreInstruction, 2),
        [NativeEnemyProjectileInstruction.DownwardGateSetYVelocityInstruction] = (DownwardGateProjectileCueKind.SetYVelocity, 2),
    };

    /// <summary>The oracle rules for the gate's programs.</summary>
    internal static NativeEnemyProjectileCueRules<DownwardGateProjectileCue> Rules { get; } = new(
        word => Commands.TryGetValue((NativeEnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            AssertEqual(Commands[(NativeEnemyProjectileInstruction)word].Kind, cue.Kind, $"{at}: cue");
            if (cue.Kind == DownwardGateProjectileCueKind.SetPreInstruction)
                AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), EnemyProjectilePreInstructionAccess.Word(cue.PreInstruction), $"{at}: pre-instruction");
            else if (cue.Kind == DownwardGateProjectileCueKind.SetYVelocity)
                AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), cue.YVelocity.RawValue, $"{at}: Y velocity");
        },
        cue => cue.Kind == DownwardGateProjectileCueKind.Delete);
}
