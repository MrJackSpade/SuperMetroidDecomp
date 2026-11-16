using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Projectiles;
using HorizontalDirection = SuperMetroid.Core.Spatial.HorizontalDirection;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the Ki-Hunter acid's typed programs match its native lists: each command and the cue it becomes (#1843).</summary>
internal static class KiHunterAcidSpitCueRules
{
    /// <summary>Each command the lists use and how the cue it becomes is checked.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, NativeCueCommand<KiHunterAcidSpitCue>> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = NativeCueCommand<KiHunterAcidSpitCue>.Exactly(new KiHunterAcidSpitCue.Signal(KiHunterAcidSpitCueKind.Delete)),
        [NativeEnemyProjectileInstruction.ClearFrameBehavior] = NativeCueCommand<KiHunterAcidSpitCue>.Exactly(new KiHunterAcidSpitCue.Signal(KiHunterAcidSpitCueKind.StopMoving)),
        // Setting a launch routine is the cue that launches the acid the way that routine moves it.
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = new(2, (operands, cue, at) =>
            VerificationAssertions.AssertEqual<Enum?>(cue switch
            {
                KiHunterAcidSpitCue.Launch { Direction: HorizontalDirection.Left } => KiHunterAcidMotion.LaunchingLeft,
                KiHunterAcidSpitCue.Launch { Direction: HorizontalDirection.Right } => KiHunterAcidMotion.LaunchingRight,
                KiHunterAcidSpitCue.Signal => null,
                _ => throw new InvalidOperationException($"{at}: undefined {nameof(KiHunterAcidSpitCue)} {cue}."),
            }, EnemyProjectileMotionAccess.FromWord(LittleEndianBytes.Word(operands[0], operands[1])), $"{at}: pre-instruction")),
    };

    /// <summary>The oracle rules for the acid's programs.</summary>
    internal static NativeEnemyProjectileCueRules<KiHunterAcidSpitCue> Rules { get; } = NativeCueCommand<KiHunterAcidSpitCue>.Rules(
        Commands, word => (NativeEnemyProjectileInstruction)word, cue => cue is KiHunterAcidSpitCue.Signal { Kind: KiHunterAcidSpitCueKind.Delete });
}
