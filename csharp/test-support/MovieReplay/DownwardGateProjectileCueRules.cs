using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the downward gate's typed programs match its native lists: each command and the cue it becomes.</summary>
internal static class DownwardGateProjectileCueRules
{
    /// <summary>Each command the gate's lists use and how the cue it becomes is checked.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, NativeCueCommand<DownwardGateProjectileCue>> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = NativeCueCommand<DownwardGateProjectileCue>.Exactly(new DownwardGateProjectileCue.Signal(DownwardGateProjectileCueKind.Delete)),
        [NativeEnemyProjectileInstruction.ClearFrameBehavior] = NativeCueCommand<DownwardGateProjectileCue>.Exactly(new DownwardGateProjectileCue.Signal(DownwardGateProjectileCueKind.StopSliding)),
        // Setting the slide routine is the cue that starts the gate sliding, the gate's one motion.
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = NativeCueCommand<DownwardGateProjectileCue>.Of<DownwardGateProjectileCue.Signal>(2, (operands, cue, at) =>
        {
            VerificationAssertions.AssertEqual(DownwardGateProjectileCueKind.StartSliding, cue.Kind, $"{at}: cue");
            VerificationAssertions.AssertTrue(EnemyProjectileMotionAccess.StandsFor(LittleEndianBytes.Word(operands[0], operands[1]), SingleMotion.Moving), $"{at}: pre-instruction");
        }),
        [NativeEnemyProjectileInstruction.DownwardGateSetYVelocityInstruction] = NativeCueCommand<DownwardGateProjectileCue>.Of<DownwardGateProjectileCue.SetYVelocity>(2, (operands, cue, at) =>
            VerificationAssertions.AssertEqual(LittleEndianBytes.Word(operands[0], operands[1]), cue.YVelocity.RawValue, $"{at}: Y velocity")),
    };

    /// <summary>The oracle rules for the gate's programs.</summary>
    internal static NativeEnemyProjectileCueRules<DownwardGateProjectileCue> Rules { get; } = NativeCueCommand<DownwardGateProjectileCue>.Rules(
        Commands, word => (NativeEnemyProjectileInstruction)word,
        cue => cue is DownwardGateProjectileCue.Signal { Kind: DownwardGateProjectileCueKind.Delete });
}
