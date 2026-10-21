using static SuperMetroid.Verification.Areas.Tourian.TourianEntranceStatueNativeListAddresses;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>#641: the Tourian entrance statue's typed programs, each matched step by step to its native bank-$AA list.</summary>
internal static class TourianEntranceStatueAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyTourianEntranceStatueAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, TourianEntranceStatueAnimations.Programs,
            frame => EnemyFrames.Installed.Spritemap(TourianEntranceStatueFrameDefinitions.ShownFrames.Of(frame)),
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Tourian entrance statue programs: {Entries.Length} programs ({steps} steps) match bank $AA.");
    }
}
