using static SuperMetroid.Verification.Enemies.Shutters.ShutterNativeListAddresses;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Shutters;

/// <summary>#641: the shutters' and Kamer's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class ShutterAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyShutterAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, ShutterAnimations.Programs,
            ShutterFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  shutter programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
