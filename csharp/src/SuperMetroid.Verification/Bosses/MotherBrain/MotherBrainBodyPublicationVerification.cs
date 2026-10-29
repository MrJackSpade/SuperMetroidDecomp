using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
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
    /// staged body image, placed by her own scroll on a 32x32 map without touching BG2's registers; her death flicker
    /// publishes the body hidden.
    /// </summary>
    internal static void VerifyMotherBrainBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room loads Mother Brain.");
        Position16<BackgroundPixels> roomScroll = runtime.BackgroundScroll.Bg2Scroll;

        // Phase one: the body is only staged; the room's pipe background shows.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(runtime.Display.Latched.Bosses.Body is null && runtime.Display.Latched.Bosses.MotherBrainBody is null,
            "phase one publishes no body over the pipe background");

        // Phase two: the transition publishes the staged body's first page; her posture scroll places it.
        BossBodySheets sheets = runtime.BossArtwork.BodySheets ?? throw new InvalidOperationException("The retail runtime installs boss body sheets.");
        ushort[] staged = Words((state.StagedBody ?? throw new InvalidOperationException("Mother Brain stages her body.")).Publish(sheets), runtime)
            [..MotherBrainBodyDefinitions.ShownCells];
        runtime.Enemies.MotherBrainBehavior.Ascent.SetupMotherBrainPhaseTwoGraphics(state);
        state.BodyLayerPlacement = new Position16<BackgroundPixels>(unchecked((ushort)-27), unchecked((ushort)-217));
        state.PlacesBodyLayer = true;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        BossBodyLayer image = runtime.Display.Latched.Bosses.Body
            ?? throw new InvalidOperationException("The phase-two transition publishes her body image.");
        AssertTrue(Words(image, runtime).AsSpan(0, staged.Length).SequenceEqual(staged), "phase two publishes the staged body image");
        AssertEqual(new MotherBrainBodyPlacement(state.BodyLayerPlacement, false), runtime.Display.Latched.Bosses.MotherBrainBody,
            "phase two publishes her body at her posture scroll");
        AssertEqual(roomScroll, runtime.BackgroundScroll.Bg2Scroll, "Mother Brain leaves the room's BG2 scroll alone");
        var drawn = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertEqual(new Extent16<TilemapCells>(32, 32), drawn.View.BackgroundMapSize, "her body shows on one 32x32 screen");
        AssertEqual(state.BodyLayerPlacement + runtime.Display.Latched.Scrolls.RoomShake.Bg2, drawn.View.BackgroundScroll, "her scroll places her body");
        AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime), "the capture draws her body image");
        AssertTrue(drawn.View.MainScreenLayers.HasFlag(ScreenLayers.Background), "her shown body keeps BG2 on the main screen");

        // Death: the flicker hides her body.
        state.DeathHidesBodyLayer = true;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(new MotherBrainBodyPlacement(state.BodyLayerPlacement, true), runtime.Display.Latched.Bosses.MotherBrainBody,
            "the death flicker publishes her body hidden");
        var hidden = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(!hidden.View.MainScreenLayers.HasFlag(ScreenLayers.Background), "a hidden body leaves BG2 off the main screen");
        Console.WriteLine("  Mother Brain body: phase one shows the pipes; phase two publishes her placed body; death hides it.");
    }

    /// <summary>Runs the Mother Brain body publication check.</summary>
    [VerificationCommand("--mother-brain-body")]
    internal static int MotherBrainBodyCommand()
    {
        Suite(nameof(VerifyMotherBrainBodyPublication), () => VerifyMotherBrainBodyPublication());
        return 0;
    }

    /// <summary>The tilemap entry each cell of Mother Brain's <paramref name="layer"/> shows, from the frames the runtime's artwork installs (#1466).</summary>
    private static ushort[] Words(BossBodyLayer layer, SuperMetroidRuntime runtime) =>
        BossBodyWords.Of(layer, BossBodyOwner.MotherBrain, runtime.BossArtwork.Artwork
            ?? throw new InvalidOperationException("The runtime installs boss artwork."));
}
