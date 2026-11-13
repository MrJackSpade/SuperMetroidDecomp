using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Samus.Artwork.SamusRenderingFixtureVerification;
using static SuperMetroid.Verification.Samus.Artwork.SamusRenderingVerification;

namespace SuperMetroid.Verification.Samus.Drawing;

/// <summary>Checks that Samus's Speed Booster echoes are published wherever they lie, for the display to clip (#1625).</summary>
internal static class SpeedBoosterEchoDrawVerification
{
    /// <summary>
    /// Draws one echo above the view and one below it: both are published at their wrapped screen positions with the
    /// picture the body last drew, the second echo first, rather than culled.
    /// </summary>
    internal static void VerifySpeedBoosterEchoesPublishedOffView()
    {
        var bus = new TestAddressSpace();
        SeedPoseOneSamusData(bus);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            AnimationFrame = 0,
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0480, 0x0086)),
        };
        BindSyntheticSamusRendering(bus, samus);
        SamusPresentation.InitializeAnimation(samus);
        var camera = new Position16<RoomPixels>(0x0400, 0x0040);
        var body = new SpriteFrameBuffer();
        body.BeginFrame();
        samus.Presentation.Draw(samus, body, cameraPosition: camera);
        AssertTrue(samus.Presentation.DrawnFrame is not null, "the body has drawn a picture for the echoes to repeat");

        samus.HorizontalSpeed.SpeedBoostCounter = SamusSpeedBoostCounter.AtFullStage;
        var above = new Position16<RoomPixels>(0x0480, 0x0010);
        var below = new Position16<RoomPixels>(0x0490, 0x0140);
        AssertTrue(samus.HorizontalSpeed.CaptureSpeedEchoPosition(0, above), "the first echo captures a position above the view");
        AssertTrue(samus.HorizontalSpeed.CaptureSpeedEchoPosition(4, below), "the second echo captures a position below the view");
        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);

        var echoes = new SpriteFrameBuffer();
        echoes.BeginFrame();
        samus.Presentation.DrawSpeedBoosterEchoes(samus, echoes, camera);
        echoes.FinalizeFrame();
        SpriteDraw[] drawn = [.. echoes.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusEcho)];
        AssertEqual(2, drawn.Length, "both echoes are published, though neither lies in the view");
        AssertEqual(new SpriteSource(SpriteSourceKind.SamusEcho, 1), drawn[0].Source, "the second echo is drawn first");
        AssertEqual(Position16<ScreenPixels>.Of(CoordinateAxis.Wrap(below.X - camera.X), CoordinateAxis.Wrap(below.Y - graphicsYOffset - camera.Y)),
            drawn[0].Origin, "the echo below the view is published where it lies");
        AssertEqual(new SpriteSource(SpriteSourceKind.SamusEcho, 0), drawn[1].Source, "the first echo is drawn second");
        AssertEqual(Position16<ScreenPixels>.Of(CoordinateAxis.Wrap(above.X - camera.X), CoordinateAxis.Wrap(above.Y - graphicsYOffset - camera.Y)),
            drawn[1].Origin, "the echo above the view is published where it lies");
        AssertTrue(CoordinateAxis.AsSigned(drawn[1].Origin.Y) < 0 && drawn[0].Origin.Y >= 224,
            "one echo lies above the view's top and the other below its bottom");
        Console.WriteLine("Speed Booster echoes: published above and below the view for the display to clip.");
    }
}
