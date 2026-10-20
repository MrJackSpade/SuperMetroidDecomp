using static SuperMetroid.Verification.Enemies.Atomic.AtomicNativeListAddresses;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Atomic;

/// <summary>#641: Atomic's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class AtomicAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyAtomicAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, AtomicAnimations.Programs,
            AtomicFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Atomic programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
