using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Powamp.PowampNativeListAddresses;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>#641: Powamp's typed animations (#1600), each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class PowampAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyPowampAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            PowampAnimations.Segments, PowampAnimations.StartingAt, PowampFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        Console.WriteLine($"  Powamp animations: {Entries.Length} segments ({beats} beats) and {PowampAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
