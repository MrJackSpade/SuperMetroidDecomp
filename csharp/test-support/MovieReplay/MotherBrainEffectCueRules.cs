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
        [NativeEnemyProjectileInstruction.ClearFrameBehavior] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Signal(MotherBrainEffectCueKind.ClearFrameBehavior)),
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = NativeCueCommand<MotherBrainEffectCue>.Of<MotherBrainEffectCue.SetFrameBehavior>(2, (operands, cue, at) =>
            VerificationAssertions.AssertEqual(cue.FrameBehavior, EnemyProjectileFrameBehaviorAccess.FromWord(LittleEndianBytes.Word(operands[0], operands[1])), $"{at}: pre-instruction")),
        [NativeEnemyProjectileInstruction.XYRadiusInY] = NativeCueCommand<MotherBrainEffectCue>.Of<MotherBrainEffectCue.SetRadius>(2, (operands, cue, at) =>
            VerificationAssertions.AssertEqual(new Extent16<RoomPixels>(operands[0], operands[1]), cue.Radius, $"{at}: X and Y radii")),
        [NativeEnemyProjectileInstruction.UsePalette0_Duplicate] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Signal(MotherBrainEffectCueKind.DrawFromSharedSheet)),
        [NativeEnemyProjectileInstruction.MotherBrainsDrool_MoveDownCPixels] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Signal(MotherBrainEffectCueKind.DropDrool)),
        [NativeEnemyProjectileInstruction.MotherBrainPurpleBreath_Inactive] = NativeCueCommand<MotherBrainEffectCue>.Exactly(new MotherBrainEffectCue.Signal(MotherBrainEffectCueKind.EndSmallPurpleBreath)),
    };

    /// <summary>The oracle rules for Mother Brain's effect programs.</summary>
    internal static NativeEnemyProjectileCueRules<MotherBrainEffectCue> Rules { get; } = NativeCueCommand<MotherBrainEffectCue>.Rules(
        Commands, word => (NativeEnemyProjectileInstruction)word,
        cue => cue is MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.Delete });
}
