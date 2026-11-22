using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the Ki-Hunter acid's typed programs match its native lists: each command and the cue it becomes (#1843).</summary>
internal static class KiHunterAcidSpitCueRules
{
    /// <summary>Each command the lists use and how the cue it becomes is checked.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, NativeCueCommand<KiHunterAcidSpitCue>> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = NativeCueCommand<KiHunterAcidSpitCue>.Exactly(new KiHunterAcidSpitCue.Signal(KiHunterAcidSpitCueKind.Delete)),
        [NativeEnemyProjectileInstruction.ClearFrameBehavior] = NativeCueCommand<KiHunterAcidSpitCue>.Exactly(new KiHunterAcidSpitCue.Signal(KiHunterAcidSpitCueKind.StopMoving)),
        // Setting a launch routine is the cue that launches the acid (#2338): either way's routine stands for the launching
        // motion the cue starts, and the way it flies is the one its Ki-Hunter spat it, which its state keeps (#2553), so the
        // cue carries none.
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = new(2, (operands, cue, at) =>
        {
            ushort routine = LittleEndianBytes.Word(operands[0], operands[1]);
            switch (cue)
            {
                case KiHunterAcidSpitCue.Launch:
                    VerificationAssertions.AssertTrue(EnemyProjectileMotionAccess.StandsFor(routine, KiHunterAcidMotion.Launching), $"{at}: pre-instruction");
                    break;
                case KiHunterAcidSpitCue.Signal:
                    VerificationAssertions.AssertTrue(EnemyProjectileMotionAccess.StandsForHoldingStill(routine), $"{at}: pre-instruction");
                    break;
                default:
                    throw new InvalidOperationException($"{at}: undefined {nameof(KiHunterAcidSpitCue)} {cue}.");
            }
        }),
    };

    /// <summary>The oracle rules for the acid's programs.</summary>
    internal static NativeEnemyProjectileCueRules<KiHunterAcidSpitCue> Rules { get; } = NativeCueCommand<KiHunterAcidSpitCue>.Rules(
        Commands, word => (NativeEnemyProjectileInstruction)word, cue => cue is KiHunterAcidSpitCue.Signal { Kind: KiHunterAcidSpitCueKind.Delete });
}
