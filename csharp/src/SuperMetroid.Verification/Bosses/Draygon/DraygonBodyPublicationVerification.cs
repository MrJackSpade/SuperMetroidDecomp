using SuperMetroid.Core.Gameplay;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon's body as a published image and placement instead of VRAM and BG2 registers (#1354).</summary>
internal static class DraygonBodyPublicationVerification
{
    /// <summary>
    /// A constructed Draygon at two body positions publishes his body image placed at his position as the frame ends (#2031),
    /// without touching the room's BG2 scroll; the capture shows the image there, its area lying where the cartridge placed
    /// it, 62 pixels left of and 64 above him.
    /// </summary>
    internal static void VerifyDraygonBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Draygon);
        DraygonEnemyState draygon = runtime.Enemies.Draygon
            ?? throw new InvalidOperationException("Draygon's room loads Draygon.");
        Offset16<BackgroundPixels> roomScroll = runtime.BackgroundScroll.BackgroundScroll;

        foreach (Position16<RoomPixels> body in new Position16<RoomPixels>[] { new(256, 160), new(352, 288) })
        {
            draygon.Body.Position = draygon.Body.Position.WithWhole(body);
            // His body is placed at his position, moved by his body graphics' displacement (#1940, #2031).
            Position16<RoomPixels> expected = body + global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Bosses.Draygon.DraygonEnemyState, global::SuperMetroid.Core.Spatial.Offset16<global::SuperMetroid.Core.Spatial.RoomPixels>>(draygon, "BodyPictureShift");
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            AssertEqual<Position16<RoomPixels>?>(expected, runtime.Display.LatchedOrdinaryRoom.Bosses.Placement, $"Draygon at {body} publishes his body's placement");
            AssertTrue(!runtime.Display.LatchedOrdinaryRoom.Bosses.Hidden, $"Draygon at {body} shows his body");
            // Each of his installed frames lies where the cartridge's layer put it: its area 62 left of and 64 above him.
            AssertEqual(new Offset16<RoomPixels>(-62, -64), BossBodySourceDefinitions.AreaFromBoss[BossBodyOwner.Draygon],
                "his body's area lay 62 pixels left of and 64 above him");
            AssertEqual(roomScroll, runtime.BackgroundScroll.BackgroundScroll, "Draygon leaves the room's BG2 scroll alone");
            _ = runtime.Display.LatchedOrdinaryRoom.Bosses.Body
                ?? throw new InvalidOperationException("Draygon publishes his body layer.");
            var drawn = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
            AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime), $"Draygon at {body} shows his body image on BG2");
            // The view shows the body row below where his body lies against the frame's camera at the screen's top (#1514, #1617).
            AssertEqual(GameplaySceneView.LayerPixelAtScreenTop(BossBodyPlacement.Shown(expected, runtime.Display.LatchedRoomFrame.Scrolls.ForegroundPosition)
                + runtime.Display.LatchedRoomFrame.Scrolls.RoomShake.Background), drawn.View.BackgroundScroll, $"Draygon at {body} places the image at his body");
            AssertTrue(drawn.View.ShownLayers.HasFlag(ScreenLayers.Background), $"Draygon at {body} shows his body layer");
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
