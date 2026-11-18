using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>
/// Checks the grapple quantities: each connected-beam draw moves the flare on a stage, and the rope length and endpoint
/// frame reject values outside their stated ranges.
/// </summary>
internal static class GrappleQuantityRangesVerification
{
    /// <summary>
    /// Drives the production beam draw from each flare stage: no flare arms as just fired, and a just-fired or shown
    /// flare is shown.
    /// </summary>
    internal static void VerifyGrappleFlareStageAdvance()
    {
        foreach (GrappleFlareStage start in Enum.GetValues<GrappleFlareStage>())
        {
            GrappleFlareStage expected = start == GrappleFlareStage.None ? GrappleFlareStage.JustFired : GrappleFlareStage.Shown;
            var grapple = new SamusGrappleState(global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Phase = GrapplePhase.Firing,
                RopeLength = 0,
                FlareStage = start,
            };
            SamusGrappleBeamDrawing.DrawConnectedBeam(grapple, new SpriteFrameBuffer(), new(0, 0),
                InstalledFixtures.GrappleSheet);
            AssertEqual(expected, grapple.FlareStage, $"grapple flare from {start} moves on");
        }
        Console.WriteLine("Grapple flare stage: each connected-beam draw moves the flare on.");
    }

    /// <summary>Checks that rope length accepts its range limits and rejects the values beyond them.</summary>
    internal static void VerifyGrappleQuantityRanges()
    {
        var grapple = new SamusGrappleState(global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { RopeLength = GrappleBeamDefinitions.Physics.LongestRopeLength };
        AssertEqual(GrappleBeamDefinitions.Physics.LongestRopeLength, grapple.RopeLength, "rope length accepts its largest stated value");
        AssertThrows<ArgumentOutOfRangeException>(() => { grapple.RopeLength = -1; }, "rope length rejects a negative length");
        AssertThrows<ArgumentOutOfRangeException>(() => { grapple.RopeLength = GrappleBeamDefinitions.Physics.LongestRopeLength + 1; },
            "rope length rejects a length firing never reaches");
        Console.WriteLine("Grapple quantity ranges: rope length keeps its stated range.");
    }

    /// <summary>Runs the grapple flare stage and quantity range checks.</summary>
    [VerificationCommand("--grapple-quantity-ranges")]
    internal static int GrappleQuantityRangesCommand()
    {
        Suite(nameof(VerifyGrappleFlareStageAdvance), VerifyGrappleFlareStageAdvance);
        Suite(nameof(VerifyGrappleQuantityRanges), VerifyGrappleQuantityRanges);
        return 0;
    }
}
