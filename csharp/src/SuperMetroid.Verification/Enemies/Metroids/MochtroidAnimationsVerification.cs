using static SuperMetroid.Verification.Enemies.Metroids.MochtroidNativeListAddresses;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>#641: Mochtroid's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class MochtroidAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMochtroidAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, MochtroidAnimations.Programs,
            MochtroidFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Mochtroid programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
