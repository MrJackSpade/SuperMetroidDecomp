using static SuperMetroid.Verification.Enemies.MorphBallEye.MorphBallEyeNativeListAddresses;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.MorphBallEye;

/// <summary>#641: the Morph Ball eye's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class MorphBallEyeAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMorphBallEyeAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, MorphBallEyeAnimations.Programs,
            MorphBallEyeFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Morph Ball eye programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
