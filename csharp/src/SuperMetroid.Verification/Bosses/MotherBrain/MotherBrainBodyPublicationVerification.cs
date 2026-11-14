using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

using SuperMetroid.Core.Enemies.BossBodies;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain's body as a published image and placement instead of VRAM and BG2 registers (#1356).</summary>
internal static class MotherBrainBodyPublicationVerification
{
    /// <summary>
    /// In phase one Mother Brain publishes no body, leaving the pipe background; her phase-two transition publishes her
    /// staged body image, placed where she holds it in the room without touching the room's own background; her death flicker
    /// publishes the body hidden.
    /// </summary>
    internal static void VerifyMotherBrainBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room loads Mother Brain.");
        Position16<BackgroundPixels> roomScroll = runtime.BackgroundScroll.BackgroundScroll;

        // Phase one: the body is only staged; the room's pipe background shows.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(runtime.Display.Latched.Bosses.Body is null && runtime.Display.Latched.Bosses.MotherBrainBody is null,
            "phase one publishes no body over the pipe background");

        // Phase two: the transition reveals her body as it stands (#1515); her posture scroll places it.
        ushort[] staged = Words(runtime.Enemies.Presentation.RequireBossBody().Publish());
        runtime.Enemies.MotherBrainBehavior.Ascent.SetupMotherBrainPhaseTwoGraphics(state);
        state.BodyOrigin = new Position16<RoomPixels>(27, 217);
        state.PlacesBodyLayer = true;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        BossBodyLayer image = runtime.Display.Latched.Bosses.Body
            ?? throw new InvalidOperationException("The phase-two transition publishes her body image.");
        AssertTrue(Words(image).SequenceEqual(staged), "phase two publishes her body as it stood");
        // Her posture places her body in the room, and the frame publishes it there (#1617, #1802).
        AssertEqual(new MotherBrainBodyPlacement(state.BodyOrigin, false),
            runtime.Display.Latched.Bosses.MotherBrainBody, "phase two publishes her body where her posture places it");
        AssertEqual(roomScroll, runtime.BackgroundScroll.BackgroundScroll, "Mother Brain leaves the room's BG2 scroll alone");
        var drawn = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        // The view shows her body's area where it lies against the frame's camera, one row below at the screen's top (#1514, #1802).
        AssertEqual(GameplaySceneView.LayerPixelAtScreenTop(BossBodyPlacement.Shown(state.BodyOrigin, runtime.Display.Latched.Scrolls.ForegroundPosition)
            + runtime.Display.Latched.Scrolls.RoomShake.Background), drawn.View.BackgroundScroll, "her body shows where it lies in the room");
        AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime), "the capture draws her body image");
        AssertTrue(drawn.View.ShownLayers.HasFlag(ScreenLayers.Background), "her shown body keeps BG2 on the main screen");

        // Death: the flicker hides her body.
        state.DeathHidesBodyLayer = true;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(new MotherBrainBodyPlacement(state.BodyOrigin, true),
            runtime.Display.Latched.Bosses.MotherBrainBody,
            "the death flicker publishes her body hidden");
        var hidden = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(!hidden.View.ShownLayers.HasFlag(ScreenLayers.Background), "a hidden body leaves BG2 off the main screen");
        Console.WriteLine("  Mother Brain body: phase one shows the pipes; phase two publishes her placed body; death hides it.");
    }

    /// <summary>Runs the Mother Brain body publication check.</summary>
    [VerificationCommand("--mother-brain-body")]
    internal static int MotherBrainBodyCommand()
    {
        Suite(nameof(VerifyMotherBrainBodyPublication), () => VerifyMotherBrainBodyPublication());
        return 0;
    }

    /// <summary>
    /// The tilemap entry each cell of Mother Brain's <paramref name="layer"/> shows, from the installation's frames her
    /// installed body sheet is composed from (#1466, #1583).
    /// </summary>
    private static ushort[] Words(BossBodyLayer layer) =>
        BossBodyWords.Of(layer, BossBodyOwner.MotherBrain, InstalledFixtures.BossArtwork);
}
