using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>How the downward gate's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class DownwardGateProjectileCueRules
{
    /// <summary>Each command the gate's lists use and how the cue it becomes is checked.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, NativeCueCommand<DownwardGateProjectileCue>> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = NativeCueCommand<DownwardGateProjectileCue>.Exactly(new DownwardGateProjectileCue.Command(DownwardGateProjectileCueKind.Delete)),
        [NativeEnemyProjectileInstruction.ClearPreInstruction] = NativeCueCommand<DownwardGateProjectileCue>.Exactly(new DownwardGateProjectileCue.Command(DownwardGateProjectileCueKind.ClearPreInstruction)),
        [NativeEnemyProjectileInstruction.PreInstructionInY] = NativeCueCommand<DownwardGateProjectileCue>.Of<DownwardGateProjectileCue.SetPreInstruction>(2, (operands, cue, at) =>
            AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), EnemyProjectilePreInstructionAccess.Word(cue.PreInstruction), $"{at}: pre-instruction")),
        [NativeEnemyProjectileInstruction.DownwardGateSetYVelocityInstruction] = NativeCueCommand<DownwardGateProjectileCue>.Of<DownwardGateProjectileCue.SetYVelocity>(2, (operands, cue, at) =>
            AssertEqual(SnesLittleEndian.Word(operands[0], operands[1]), cue.YVelocity.RawValue, $"{at}: Y velocity")),
    };

    /// <summary>The oracle rules for the gate's programs.</summary>
    internal static NativeEnemyProjectileCueRules<DownwardGateProjectileCue> Rules { get; } = NativeCueCommand<DownwardGateProjectileCue>.Rules(
        Commands, word => (NativeEnemyProjectileInstruction)word,
        cue => cue is DownwardGateProjectileCue.Command { Kind: DownwardGateProjectileCueKind.Delete });
}
