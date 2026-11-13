using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Mother Brain ceiling-tube ROM oracle (#641) walks: each program's entry point, and the
/// family that binds the typed programs to them. Only verification reads them; the port runs typed programs.
/// </summary>
internal static class MotherBrainTopTubeNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MotherBrainTopTubeSegment>[] Entries =
    [
        new NativeProgramEntry<MotherBrainTopTubeSegment>(MotherBrainTopTubeSegment.TopRight, MotherBrainTopTubeInstructionProgramDefinitions.TopRight),
        new NativeProgramEntry<MotherBrainTopTubeSegment>(MotherBrainTopTubeSegment.TopLeft, MotherBrainTopTubeInstructionProgramDefinitions.TopLeft),
        new NativeProgramEntry<MotherBrainTopTubeSegment>(MotherBrainTopTubeSegment.TopMiddleLeft, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft),
        new NativeProgramEntry<MotherBrainTopTubeSegment>(MotherBrainTopTubeSegment.TopMiddleRight, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight),
    ];

    /// <summary>The ceiling-tube lists use no family commands.</summary>
    private static readonly NativeEnemyProjectileCueRules<NoAnimationCue> Rules = new(
        word => null,
        (word, operands, cue, at) => throw new InvalidDataException($"{at}: the ceiling-tube lists use no family commands."));

    /// <summary>The family's typed segments and animations beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MotherBrainTopTubeAnimations.Segments, MotherBrainTopTubeAnimations.StartingAt,
        frame => EnemyProjectileProgramFrameSources.Operand(MotherBrainTopTubeFrameDefinitions.ShownProjectileFrames[frame]), Rules);
}
