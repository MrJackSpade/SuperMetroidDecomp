using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.Powamp.PowampNativeListAddresses;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>
/// #641: Powamp's typed animations (#1600), the body's and the balloon's (#2553), each segment matched to its native bank-$A8
/// list and each animation to the lists' control flow.
/// </summary>
internal static class PowampAnimationsVerification
{
    /// <summary>
    /// Every segment of each family matches its native list, and every animation its lists' control flow: the balloon's
    /// poses through the spritemap of the installed frame each shows.
    /// </summary>
    internal static void VerifyPowampAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int beats = NativeEnemyAnimationOracle.CompareAll(cartridge, Bank, BodyEntries,
            PowampAnimations.BodySegments, PowampAnimations.BodyStartingAt, PowampFrameToolingDefinitions.Spritemap, NoOwnerInstructions)
            + NativeEnemyAnimationOracle.CompareAll(cartridge, Bank, BalloonEntries,
            PowampAnimations.BalloonSegments, PowampAnimations.BalloonStartingAt, PowampFrameToolingDefinitions.BalloonSpritemap, NoOwnerInstructions);
        Console.WriteLine($"  Powamp animations: {BodyEntries.Length + BalloonEntries.Length} segments ({beats} beats) and "
            + $"{PowampAnimations.BodyStartingAt.Count + PowampAnimations.BalloonStartingAt.Count} animations match bank $A8.");
    }

    /// <summary>The cue rules of lists that use no owner instructions.</summary>
    private static NativeEnemyCueRules<NoAnimationCue> NoOwnerInstructions { get; } = new(
        word => null,
        (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
}
