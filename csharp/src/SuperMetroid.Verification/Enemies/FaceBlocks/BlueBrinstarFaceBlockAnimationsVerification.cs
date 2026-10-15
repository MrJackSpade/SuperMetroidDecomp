using static SuperMetroid.Verification.Enemies.FaceBlocks.BlueBrinstarFaceBlockNativeListAddresses;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.FaceBlocks;

/// <summary>#641: the Blue Brinstar face block's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class BlueBrinstarFaceBlockAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyBlueBrinstarFaceBlockAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, BlueBrinstarFaceBlockAnimations.Programs,
            BlueBrinstarFaceBlockFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Blue Brinstar face block programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
