using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks grapple endpoint placement, wrapped subtraction, and OAM across cameras and coordinates.</summary>
internal static class GrappleEndpointGeometryVerification
{
    /// <summary>Checks endpoint placement and attributes across camera and coordinate boundaries.</summary>
    internal static void VerifyGrappleEndpointGeometry()
    {
        // The rope's one segment is published too (#1587), so only the endpoint's draw is read.
        // Probe both sides of the camera subtraction as well as positions above
        // and below the view, where the endpoint is still published.
        int cases = 0;
        foreach (ushort camera in new ushort[] { 0, 128, 65535 })
        foreach (int offsetX in new[] { -1, 0, 4, 255, 256 })
        foreach (int offsetY in new[] { -1, 0, 3, 4, 255, 256 })
        {
            ushort endpointX = unchecked((ushort)(camera + offsetX));
            ushort endpointY = unchecked((ushort)(camera + offsetY));
            var grapple = new SamusGrappleState
            {
                Phase = GrapplePhase.Firing, RopeLength = 8,
                BeamStart = new(camera, camera),
                Anchor = new(endpointX, endpointY), PointAnimationTimer = WrappingCountdown.Start(5),
            };
            var drawn = new SpriteFrameBuffer();
            drawn.BeginFrame();
            SamusGrappleBeamDrawing.DrawConnectedBeam(grapple, drawn, new(camera, camera), InstalledFixtures.GrappleSheet);
            drawn.FinalizeFrame();
            OamBuffer oam = SpriteFrameBuffer.DrawnOam(frame =>
            {
                foreach (SpriteDraw draw in drawn.Draws)
                    if (draw.Source.Kind == SpriteSourceKind.SamusGrappleEndpoint)
                        frame.Add(draw);
            });
            string context = $"camera {camera}, offsets {offsetX}/{offsetY}";
            // The endpoint is published wherever it lies; the display clips it to the view (#1541).
            AssertEqual(4, (oam.LastFinalizedSpriteCount * 4), "Endpoint is published wherever it lies: " + context);
            // The endpoint is centred on the anchor in every pose, including when unsigned world coordinates wrap; the
            // swinging draw's borrow into the half-character offset is gone (#1588).
            ushort x = unchecked((ushort)(endpointX - camera - 4));
            ushort y = unchecked((ushort)(endpointY - camera - 4));
            AssertEqual((byte)x, oam.LowTable[0], "Native endpoint X: " + context);
            AssertEqual((byte)y, oam.LowTable[1], "Native endpoint Y: " + context);
            AssertEqual((byte)((LittleEndianBytes.HighByte(x)) & 1), oam.HighTable[0], "Native endpoint high-X bit: " + context);
            AssertEqual((byte)0x20, oam.LowTable[2], "Native endpoint character");
            AssertEqual((byte)0x3a, oam.LowTable[3], "Native endpoint palette/priority");
            cases++;
        }
        Suite(nameof(VerifyGrappleEndpointActor), () => VerifyGrappleEndpointActor());
        Console.WriteLine($"Grapple endpoint geometry: {cases} camera/wrapped-coordinate cases and the actual actor pass in each pose place the endpoint centred on the anchor with native OAM attributes.");
    }

    /// <summary>Checks the full gameplay actor drawing pass publishes the endpoint in every pose.</summary>
    private static void VerifyGrappleEndpointActor()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var samus = runtime.Player.IntroducedSamus;
        foreach (SamusPoseId pose in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.GrappleSwingRightPose, SamusPoseId.GrappleSwingLeftPose })
        {
            samus.Pose = pose;
            samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
            SamusPresentation.InitializeAnimation(samus);
            samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
            samus.Grapple.RopeLength = 8;
            samus.Grapple.Anchor = samus.Grapple.Anchor.WithX(samus.Position.X.Whole);
            samus.Grapple.Anchor = samus.Grapple.Anchor.WithY(unchecked((ushort)(runtime.Room.LoadedCamera.PixelPosition.Y - 1)));
            samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(samus.Position.X.Whole);
            samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(samus.Position.Y.Whole);
            runtime.Sprites.BeginFrame();
            runtime.ActorDrawing.DrawGameplayActors(runtime.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            runtime.Sprites.FinalizeFrame();
            ImageSpriteDraw[] endpoints = [.. runtime.Sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusGrappleEndpoint).Cast<ImageSpriteDraw>()];
            // The endpoint just above the view is published in every pose; the display clips it (#1541).
            AssertEqual(1, endpoints.Length, $"Actual gameplay actor publishes the endpoint in pose {(int)SamusPoseNumbers.Number(pose):X2}");
            // It is centred on the anchor in every pose, the swinging ones included (#1588).
            AssertEqual(new Position16<ScreenPixels>(100, CoordinateAxis.Wrap(-1)), endpoints[0].Origin,
                $"Actual gameplay actor centres the endpoint on the anchor in pose {(int)SamusPoseNumbers.Number(pose):X2}");
        }
    }
}
