using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Shaktool;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How Shaktool's attack-circle programs match their native lists: each command and the cue it becomes (#1843).</summary>
internal static class ShaktoolCircleCueRules
{
    /// <summary>Each command the lists use and how the cue it becomes is checked.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, NativeCueCommand<ShaktoolCircleCue>> Commands = new()
    {
        // Setting the trailing routine is the cue that starts a middle or back circle following the front one.
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = new(2, (operands, cue, at) =>
        {
            VerificationAssertions.AssertEqual(ShaktoolCircleCue.StartTrailing, cue, $"{at}: cue");
            VerificationAssertions.AssertEqual<Enum?>(ShaktoolCircleMotion.Trailing,
                EnemyProjectileMotionAccess.FromWord(LittleEndianBytes.Word(operands[0], operands[1])), $"{at}: pre-instruction");
        }),
    };

    /// <summary>The oracle rules for the circles' programs.</summary>
    internal static NativeEnemyProjectileCueRules<ShaktoolCircleCue> Rules { get; } = NativeCueCommand<ShaktoolCircleCue>.Rules(
        Commands, word => (NativeEnemyProjectileInstruction)word);
}
