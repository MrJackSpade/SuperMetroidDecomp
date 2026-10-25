using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>#641: the ceiling-tube typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MotherBrainTopTubeAnimationsVerification
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MotherBrainTopTubeProgram>[] Entries =
    [
        new NativeProgramEntry<MotherBrainTopTubeProgram>(MotherBrainTopTubeProgram.TopRight, MotherBrainTopTubeInstructionProgramDefinitions.TopRight),
        new NativeProgramEntry<MotherBrainTopTubeProgram>(MotherBrainTopTubeProgram.TopLeft, MotherBrainTopTubeInstructionProgramDefinitions.TopLeft),
        new NativeProgramEntry<MotherBrainTopTubeProgram>(MotherBrainTopTubeProgram.TopMiddleLeft, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft),
        new NativeProgramEntry<MotherBrainTopTubeProgram>(MotherBrainTopTubeProgram.TopMiddleRight, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight),
    ];

    /// <summary>The ceiling-tube lists use no family commands.</summary>
    private static readonly NativeEnemyProjectileCueRules<NoAnimationCue> Rules = new(
        word => null,
        (word, operands, cue, at) => throw new InvalidDataException($"{at}: the ceiling-tube lists use no family commands."));

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MotherBrainTopTubeAnimations.Programs,
        frame => EnemyProjectileProgramFrameSources.Operand(MotherBrainTopTubeFrameDefinitions.ShownProjectileFrames[frame]), Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMotherBrainTopTubeAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Mother Brain ceiling-tube programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
