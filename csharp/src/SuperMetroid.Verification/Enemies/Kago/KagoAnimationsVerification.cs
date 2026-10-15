using static SuperMetroid.Verification.Enemies.Kago.KagoNativeListAddresses;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>#641: Kago's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class KagoAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyKagoAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, KagoAnimations.Programs,
            KagoFrameDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Kago programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
