using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Gameplay;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon's body as a published image and placement instead of VRAM and BG2 registers (#1354).</summary>
internal static class DraygonBodyPublicationVerification
{
    /// <summary>
    /// A constructed Draygon at two body positions publishes his body image and the placement the drawing hook derives
    /// from the final camera, without touching the room's BG2 scroll; the capture shows the image at that placement.
    /// </summary>
    internal static void VerifyDraygonBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
        DraygonEnemyState draygon = runtime.Enemies.DraygonBehavior.Parts.Draygon
            ?? throw new InvalidOperationException("Draygon's room loads Draygon.");
        Position16<BackgroundPixels> roomScroll = runtime.BackgroundScroll.Bg2Scroll;

        foreach (Position16<RoomPixels> body in new Position16<RoomPixels>[] { new(256, 160), new(352, 288) })
        {
            draygon.Body.Position = draygon.Body.Position.WithWhole(body);
            runtime.ActorDrawing.DrawGameplayActors(runtime.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            Position16<RoomPixels> camera = runtime.Room.LoadedCamera.PixelPosition;
            var expected = new Position16<BackgroundPixels>(
                unchecked((ushort)(draygon.BodyGraphicsDisplacement.X + camera.X - body.X - 450)),
                unchecked((ushort)(draygon.BodyGraphicsDisplacement.Y + camera.Y - body.Y - 192)));
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            DraygonBodyPlacement placement = runtime.Display.Latched.Bosses.DraygonBody
                ?? throw new InvalidOperationException("Draygon publishes his body placement.");
            AssertEqual(new DraygonBodyPlacement(body, expected, false), placement, $"Draygon at {body} publishes his body position and placement");
            AssertEqual(roomScroll, runtime.BackgroundScroll.Bg2Scroll, "Draygon leaves the room's BG2 scroll alone");
            _ = runtime.Display.Latched.Bosses.EnemyBg2
                ?? throw new InvalidOperationException("Draygon publishes his enemy BG2 image.");
            var drawn = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
            AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime), $"Draygon at {body} shows his body image on BG2");
            AssertEqual(expected + runtime.Display.Latched.Scrolls.RoomShake.Bg2, drawn.View.BackgroundScroll, $"Draygon at {body} places the image at his body");
            AssertEqual(placement.VisibleLines(runtime.Display.Latched.Scrolls.Layer1Position),
                new DraygonBodyScanlines(drawn.View.BackgroundFirstLine, drawn.View.BackgroundEndLine), $"Draygon at {body} shows BG2 on the scanlines around his body");
        }
        Console.WriteLine("  Draygon body: the body image publishes at two body positions through Draygon's own placement.");
    }

    /// <summary>Runs the Draygon body publication check.</summary>
    [VerificationCommand("--draygon-body")]
    internal static int DraygonBodyCommand()
    {
        Suite(nameof(VerifyDraygonBodyPublication), () => VerifyDraygonBodyPublication());
        return 0;
    }
}
