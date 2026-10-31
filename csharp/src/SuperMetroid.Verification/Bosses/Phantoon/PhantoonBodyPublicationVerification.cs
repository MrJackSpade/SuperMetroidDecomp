using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
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

        // The body frame draws on his body layer from his installed body sheet (#1466).
        EnemyFrameId frame = PhantoonBg2FrameDefinitions.Frames[1].Frame;
        body.ExtraProperties = body.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
        runtime.Enemies.Instructions.DrawBossBodyFrame(body, frame);
        BossArtworkCatalog bosses = runtime.BossArtwork.Artwork ?? throw new InvalidOperationException("The retail runtime installs boss artwork.");
        var expected = new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight];
        expected.AsSpan().Fill(BossBodySheetComposer.BlankOf(BossBodyOwner.Phantoon).PackedWord);
        if (!(bosses.Sources.PhantoonBg2Frames ?? throw new InvalidOperationException("Phantoon's BG2 frames are installed.")).TryGet(frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes))
            throw new InvalidOperationException("Phantoon's body frame has installed writes.");
        foreach (EnemyBg2TilemapWrite write in writes.Span)
            write.Words.CopyTo(expected.AsSpan(write.FirstCell));

        // Wavy fade-in: the intro wave set up and built once.
        state.BodyLayerPlacement = new Position16<BackgroundPixels>(40, 0);
        state.Parts[state.Mouth.SlotIndex].WaveAmplitude = 3072;
        state.Parts[state.Mouth.SlotIndex].WavyPhaseDelta = 8;
        state.Wave.Begin(PhantoonWaveDefinitions.IntroLength);
        state.Wave.Step(state);
        state.Wave.Step(state);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        PhantoonDisplaySnapshot fadingIn = runtime.Display.Latched.Phantoon
            ?? throw new InvalidOperationException("Phantoon publishes his display.");
        AssertEqual(new PhantoonWave(40, 3072, new ByteAngle((byte)(state.Wave.Phase / 2)), PhantoonWaveDefinitions.ShortPeriod),
            fadingIn.Wave, "the wavy fade-in publishes the wave's base scroll, amplitude, phase's angle and its mode's period");
        BossBodyLayer image = runtime.Display.Latched.Bosses.Body
            ?? throw new InvalidOperationException("Phantoon publishes his body layer.");
        AssertTrue(BossBodyWords.Of(image, BossBodyOwner.Phantoon, bosses).AsSpan().SequenceEqual(expected),
            "the published layer shows the drawn body frame over the cleared layer");
        var drawn = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime), "the capture draws the body image on BG2");

        // Swooping fade-out: the wave channel is deleted, the body stays.
        state.Parts[state.Eye.SlotIndex].WaveLength = null;
        state.Wave.Step(state);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        PhantoonDisplaySnapshot fadingOut = runtime.Display.Latched.Phantoon
            ?? throw new InvalidOperationException("Phantoon publishes his display.");
        AssertTrue(fadingOut.Wave is null, "a deleted wave publishes nothing");
        AssertTrue(ReferenceEquals(image, runtime.Display.Latched.Bosses.Body), "an unchanged body layer publishes again unchanged");
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
