using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>#641: the ceiling-tube typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MotherBrainTopTubeAnimationsVerification
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (MotherBrainTopTubeProgram Program, ushort Entry)[] Entries =
    [
        (MotherBrainTopTubeProgram.TopRight, MotherBrainTopTubeInstructionProgramDefinitions.TopRight),
        (MotherBrainTopTubeProgram.TopLeft, MotherBrainTopTubeInstructionProgramDefinitions.TopLeft),
        (MotherBrainTopTubeProgram.TopMiddleLeft, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft),
        (MotherBrainTopTubeProgram.TopMiddleRight, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight),
    ];

    /// <summary>The ceiling-tube lists use no family commands.</summary>
    private static readonly NativeEnemyProjectileCueRules<NoAnimationCue> Rules = new(
        word => null,
        (word, operands, cue, at) => throw new InvalidDataException($"{at}: the ceiling-tube lists use no family commands."));

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MotherBrainTopTubeAnimations.Programs,
        frame => MotherBrainTopTubeFrameDefinitions.PresentationKeys[frame], Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMotherBrainTopTubeAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Mother Brain ceiling-tube programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
