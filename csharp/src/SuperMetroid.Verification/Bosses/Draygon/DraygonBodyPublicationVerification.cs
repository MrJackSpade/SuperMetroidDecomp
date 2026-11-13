using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.Draygon;
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
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Draygon);
        DraygonEnemyState draygon = runtime.Enemies.DraygonBehavior.Parts.Draygon
            ?? throw new InvalidOperationException("Draygon's room loads Draygon.");
        Position16<BackgroundPixels> roomScroll = runtime.BackgroundScroll.BackgroundScroll;

        foreach (Position16<RoomPixels> body in new Position16<RoomPixels>[] { new(256, 160), new(352, 288) })
        {
            draygon.Body.Position = draygon.Body.Position.WithWhole(body);
            runtime.ActorDrawing.DrawGameplayActors(runtime.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            // His body lies in the room at his body's position, moved by his body graphics' displacement (#1617).
            var expected = new Position16<RoomPixels>(
                unchecked((ushort)(body.X + DraygonBackgroundData.HorizontalOrigin - draygon.BodyGraphicsDisplacement.X)),
                unchecked((ushort)(body.Y + DraygonBackgroundData.VerticalOrigin - draygon.BodyGraphicsDisplacement.Y)));
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            DraygonBodyPlacement placement = runtime.Display.Latched.Bosses.DraygonBody
                ?? throw new InvalidOperationException("Draygon publishes his body placement.");
            AssertEqual(new DraygonBodyPlacement(body, expected, false), placement, $"Draygon at {body} publishes his body position and placement");
            AssertEqual(roomScroll, runtime.BackgroundScroll.BackgroundScroll, "Draygon leaves the room's BG2 scroll alone");
            _ = runtime.Display.Latched.Bosses.Body
                ?? throw new InvalidOperationException("Draygon publishes his body layer.");
            var drawn = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
            AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime), $"Draygon at {body} shows his body image on BG2");
            // The view shows the body row below where his body lies against the frame's camera at the screen's top (#1514, #1617).
            AssertEqual(GameplaySceneView.ShownAtScreenTop(BossBodyPlacement.Shown(expected, runtime.Display.Latched.Scrolls.ForegroundPosition)
                + runtime.Display.Latched.Scrolls.RoomShake.Background), drawn.View.BackgroundScroll, $"Draygon at {body} places the image at his body");
            AssertEqual(DraygonBodyBand.ShownLines(placement, runtime.Display.Latched.Scrolls.ForegroundPosition),
                new ScreenLineBand(drawn.View.BackgroundFirstLine, drawn.View.BackgroundEndLine), $"Draygon at {body} shows BG2 on the band of lines around his body");
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
