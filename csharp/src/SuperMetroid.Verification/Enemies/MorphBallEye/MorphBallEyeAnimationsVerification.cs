using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Enemies.MorphBallEye.MorphBallEyeNativeListAddresses;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.MorphBallEye;

/// <summary>
/// #641: the Morph Ball eye's typed animations (#1600), each segment matched to its native bank-$A8 list and each animation to
/// the lists' control flow, and its aim frames (#2070) to the native list it aimed with.
/// </summary>
internal static class MorphBallEyeAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMorphBallEyeAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            MorphBallEyeAnimations.Segments, MorphBallEyeAnimations.StartingAt, MorphBallEyeFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                word => null,
                (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions.")));
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        // The eye restarted its aim list every update at the frame its sixteenth of the turn numbered, so each frame's
        // duration never ran out: the k-th frame is the pose the k-th arc shows, and the list loops on itself.
        IReadOnlyList<EnemyFrameId> aims = MorphBallEyeAnimations.AimFrames.InTurnOrder;
        for (int arc = 0; arc < aims.Count; arc++)
        {
            int frame = (int)new SnesAddress(Bank, (ushort)(AimList + 4 * arc));
            AssertEqual(NativeEnemyListDefinitions.WordKind.Frame, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(frame)), $"aim list entry {arc} is a frame");
            AssertEqual(cartridge.ReadCartridgeWord(frame + 2), MorphBallEyeFrameToolingDefinitions.Spritemap(aims[arc]), $"arc {arc} aims as the native list's frame {arc}");
        }
        int afterAims = (int)new SnesAddress(Bank, (ushort)(AimList + 4 * aims.Count));
        AssertEqual(NativeEnemyListDefinitions.WordKind.GoTo, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(afterAims)), "the aim list ends in a goto");
        AssertEqual(AimList, cartridge.ReadCartridgeWord(afterAims + 2), "the aim list loops on itself");
        Console.WriteLine($"  Morph Ball eye animations: {Entries.Length} segments ({beats} beats), {MorphBallEyeAnimations.StartingAt.Count} animations and {aims.Count} aim arcs match bank $A8.");
    }
}
