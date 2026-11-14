using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Decodes what Kraid's fingernails, Crocomire's volley and the opening cinematic's egg shell read past their data from
/// the player's cartridge (#1500): it runs each read the way the original game does, over the cartridge, for every game
/// situation that reaches it, and keeps only the result.
/// </summary>
internal static class BossAndIntroOverreadDecoding
{
    /// <summary>Decodes every read into the installed document.</summary>
    internal static BossAndIntroOverreadDocument Decode(IImportCartridgeSource cartridge)
    {
        Ensure.NotNull(cartridge);
        return new BossAndIntroOverreadDocument(
            BossAndIntroOverreadFormat.Version,
            KraidNailPastLedges(cartridge),
            CartridgeWordReads.SignedWord(cartridge.ReadCartridgeWord(
                BossAndIntroOverreadSourceDefinitions.CrocomireVolleyGradients + CrocomireProjectileDefinitions.VolleyShotCount * sizeof(ushort))),
            ShellFragmentFallPastCurve(cartridge));
    }

    /// <summary>The nail contour record the walk reads once a nail lies above all four ledges: the one that follows them.</summary>
    private static BossAndIntroOverreadDocument.ContourRecord KraidNailPastLedges(IImportCartridgeSource cartridge)
    {
        int record = BossAndIntroOverreadSourceDefinitions.KraidNailContour +
            KraidNailContourDefinitions.LedgeCount * BossAndIntroOverreadSourceDefinitions.KraidNailContourRecordBytes;
        return new BossAndIntroOverreadDocument.ContourRecord(
            CartridgeWordReads.SignedWord(cartridge.ReadCartridgeWord(record)),
            CartridgeWordReads.SignedWord(cartridge.ReadCartridgeWord(record + BossAndIntroOverreadSourceDefinitions.KraidNailContourBoundOffset)));
    }

    /// <summary>
    /// The gravity steps the shell fragments read past their curve: each fragment falls from its spawn by the cartridge's
    /// steps, a step per frame fallen, until the floor test deletes it, and the steps past the curve that any fragment
    /// reads are kept.
    /// </summary>
    /// <exception cref="InvalidDataException">A fragment would still be airborne when its fall count wraps.</exception>
    private static List<BossAndIntroOverreadDocument.FallStep> ShellFragmentFallPastCurve(IImportCartridgeSource cartridge)
    {
        int framesRead = 0;
        foreach (IntroEggShellFragment fragment in IntroEggEffectDefinitions.ShellFragments)
        {
            var y = new FixedCoordinate(IntroEggMotion.FragmentInitialPosition(fragment).Y, 0);
            int frame = 0;
            bool landed = false;
            while (!landed)
            {
                if (frame == BossAndIntroOverreadSourceDefinitions.ShellFragmentFrameLimit)
                    throw new InvalidDataException($"Egg-shell fragment {fragment} is still airborne when its fall count wraps.");
                y = GravityStep(cartridge, frame).AddTo(y.Whole, y.Fraction);
                frame++;
                landed = CoordinateAxis.Difference(y.Whole, IntroEggMotionDefinitions.FragmentFloorY) >= 0;
            }
            framesRead = Math.Max(framesRead, frame);
        }
        return [.. Enumerable.Range(IntroEggMotionDefinitions.FragmentCurveFrames, Math.Max(0, framesRead - IntroEggMotionDefinitions.FragmentCurveFrames))
            .Select(frame => GravityStep(cartridge, frame))
            .Select(step => new BossAndIntroOverreadDocument.FallStep(step.WholePixels, step.Fraction))];
    }

    /// <summary>The gravity step a fragment falls by on <paramref name="frame"/>, read from the cartridge.</summary>
    private static SignedSixteenSixteen GravityStep(IImportCartridgeSource cartridge, int frame)
    {
        int step = BossAndIntroOverreadSourceDefinitions.ShellFragmentGravity + frame * BossAndIntroOverreadSourceDefinitions.ShellFragmentGravityStepBytes;
        return SignedSixteenSixteen.FromParts(
            CartridgeWordReads.SignedWord(cartridge.ReadCartridgeWord(step)),
            cartridge.ReadCartridgeWord(step + BossAndIntroOverreadSourceDefinitions.ShellFragmentGravityFractionOffset));
    }
}
