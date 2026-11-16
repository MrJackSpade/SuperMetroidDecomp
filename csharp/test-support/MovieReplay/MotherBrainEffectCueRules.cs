using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How Mother Brain's effect programs match their native lists: each command and the cue it becomes.</summary>
internal static class MotherBrainEffectCueRules
{
    /// <summary>Each command the lists use and how the cue it becomes is checked.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, NativeCueCommand<MotherBrainEffectCue>> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Signal(MotherBrainEffectCueKind.Delete)),
        [NativeEnemyProjectileInstruction.ClearFrameBehavior] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Signal(MotherBrainEffectCueKind.StopMoving)),
        // The effects set one routine by list: the released drool's fall.
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = NativeCueCommand<MotherBrainEffectCue>.Of<MotherBrainEffectCue.Signal>(2, (operands, cue, at) =>
        {
            VerificationAssertions.AssertEqual(MotherBrainEffectCueKind.ReleaseDrool, cue.Kind, $"{at}: cue");
            VerificationAssertions.AssertEqual<Enum?>(MotherBrainProjectileMotion.DroolFalling, EnemyProjectileMotionAccess.FromWord(LittleEndianBytes.Word(operands[0], operands[1])), $"{at}: pre-instruction");
        }),
        [NativeEnemyProjectileInstruction.XYRadiusInY] = NativeCueCommand<MotherBrainEffectCue>.Of<MotherBrainEffectCue.SetRadius>(2, (operands, cue, at) =>
            VerificationAssertions.AssertEqual(new Extent16<RoomPixels>(operands[0], operands[1]), cue.Radius, $"{at}: X and Y radii")),
        [NativeEnemyProjectileInstruction.MotherBrainsDrool_MoveDownCPixels] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Signal(MotherBrainEffectCueKind.DropDrool)),
        [NativeEnemyProjectileInstruction.MotherBrainPurpleBreath_Inactive] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Signal(MotherBrainEffectCueKind.EndSmallPurpleBreath)),
    };

    /// <summary>The oracle rules for Mother Brain's effect programs.</summary>
    internal static NativeEnemyProjectileCueRules<MotherBrainEffectCue> Rules { get; } = NativeCueCommand<MotherBrainEffectCue>.Rules(
        Commands, word => (NativeEnemyProjectileInstruction)word,
        cue => cue is MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.Delete }) with
    {
        // The onion ring's impact frames are room pictures that carry their sheet and colours (#2260): its palette switch has no event.
        Omitted = word => (NativeEnemyProjectileInstruction)word == NativeEnemyProjectileInstruction.UsePalette0_Duplicate,
    };
}
