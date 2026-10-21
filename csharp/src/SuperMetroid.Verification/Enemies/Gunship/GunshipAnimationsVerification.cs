using static SuperMetroid.Verification.Enemies.Gunship.GunshipNativeListAddresses;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>#1290: the gunship's hull and entrance pad typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class GunshipAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyGunshipAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, GunshipAnimations.Programs,
            GunshipFrameDefinitionsConstants.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  gunship programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
