using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Checks Phantoon's body and wave as published models instead of VRAM and HDMA tables (#1353).</summary>
internal static class PhantoonBodyPublicationVerification
{
    /// <summary>
    /// A constructed Phantoon in his wavy fade-in publishes the body frame drawn into his enemy BG2 image and the wave's
    /// mode, phase, amplitude and base scroll; swooping out, the wave is gone while the body stays published. His fades
    /// show through the body palette in the frame's CGRAM snapshot.
    /// </summary>
    internal static void VerifyPhantoonBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Phantoon);
        PhantoonEnemyState state = runtime.Enemies.PhantoonBehavior.Combat.Phantoon
            ?? throw new InvalidOperationException("Phantoon's room loads Phantoon.");
        RoomEnemySlot body = runtime.Enemies.SlotTable.Slots[0];

        // The body frame draws into the enemy BG2 image through the installed frame's writes.
        EnemyFrameId frame = PhantoonBg2FrameDefinitions.Frames[1].Frame;
        body.ExtraProperties = body.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
        runtime.Enemies.Instructions.ApplyInstalledEnemyBg2Frame(body, frame);
        var expected = new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight];
        expected.AsSpan().Fill(PhantoonBg2FrameDefinitions.BlankTile);
        EnemyBg2FrameLookup lookup = ((IEnemyBg2Frames)body.Entity!).FindBg2Frame(frame, runtime.Enemies.Room)
            ?? throw new InvalidOperationException("Phantoon's body frame has installed writes.");
        foreach (EnemyBg2TilemapWrite write in lookup.Writes.Span)
            write.Tiles.Span.CopyTo(expected.AsSpan(write.FirstCell));

        // Wavy fade-in: the intro wave set up and built once.
        state.Bg2Scroll = new Position16<Bg2Pixels>(40, 0);
        state.Parts[state.Mouth.SlotIndex].WaveAmplitude = 3072;
        state.Parts[state.Mouth.SlotIndex].WavyPhaseDelta = 8;
        state.Wave.Begin(PhantoonWaveDefinitions.IntroMode);
        state.Wave.Step(state);
        state.Wave.Step(state);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        PhantoonDisplaySnapshot fadingIn = runtime.Display.Latched.Phantoon
            ?? throw new InvalidOperationException("Phantoon publishes his display.");
        AssertEqual(new PhantoonWave(PhantoonWaveDefinitions.IntroMode, state.Wave.Phase, 3072, 40), fadingIn.Wave,
            "the wavy fade-in publishes the wave's mode, phase, amplitude and base scroll");
        EnemyBg2ImageSnapshot image = runtime.Display.Latched.Bosses.EnemyBg2
            ?? throw new InvalidOperationException("Phantoon publishes his enemy BG2 image.");
        AssertTrue(image.Words.AsSpan().SequenceEqual(expected), "the published image holds the drawn body frame over the cleared image");
        var drawn = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime), "the capture draws the body image on BG2");

        // Swooping fade-out: the wave channel is deleted, the body stays.
        state.Parts[state.Eye.SlotIndex].WaveMode = 0;
        state.Wave.Step(state);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        PhantoonDisplaySnapshot fadingOut = runtime.Display.Latched.Phantoon
            ?? throw new InvalidOperationException("Phantoon publishes his display.");
        AssertTrue(fadingOut.Wave is null, "a deleted wave publishes nothing");
        AssertTrue(ReferenceEquals(image, runtime.Display.Latched.Bosses.EnemyBg2), "an unchanged body image publishes again unchanged");
        Console.WriteLine("  Phantoon body: the body image and wave publish through the wavy fade-in and the swooping fade-out.");
    }

    /// <summary>Runs the Phantoon body publication check.</summary>
    [VerificationCommand("--phantoon-body")]
    internal static int PhantoonBodyCommand()
    {
        Suite(nameof(VerifyPhantoonBodyPublication), () => VerifyPhantoonBodyPublication());
        return 0;
    }
}
