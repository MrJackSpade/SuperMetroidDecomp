using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks the body frame Kraid publishes on BG2 instead of writing tilemap pages into VRAM (#1352).</summary>
internal static class KraidBodyPublicationVerification
{
    /// <summary>
    /// Constructed Kraid states publish their body frame at Kraid's BG2 scroll: the risen body's two pages, then the open
    /// mouth's head over the upper page; once his death releases BG2, no body publishes and BG2 shows the room again.
    /// </summary>
    internal static void VerifyKraidBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Kraid);
        KraidEnemyState state = runtime.Enemies.KraidBehavior.Access.Kraid
            ?? throw new InvalidOperationException("Kraid's room loads Kraid.");
        BossArtworkCatalog art = runtime.BossArtwork.Artwork
            ?? throw new InvalidOperationException("The retail runtime installs boss artwork.");
        ushort[] working = KraidBodyFixtures.WorkingMap(art);
        ReadOnlySpan<ushort> upper = working.AsSpan(0, KraidBackgroundRomData.VisiblePageWords);
        ReadOnlySpan<ushort> lower = working.AsSpan(KraidBackgroundRomData.WorkingLowerHalfWord, KraidBackgroundRomData.VisiblePageWords);
        AssertTrue(state.OwnsBg2Tilemap, "Kraid takes BG2 when his room loads");

        // Risen: both body pages uploaded, at the scroll his body position computed.
        KraidBackground.TransferKraidTopTilemap(state);
        KraidBackground.TransferKraidBottomTilemap(state);
        state.Bg2Scroll = new Position16<Bg2Pixels>(320, 88);
        BossBg2Layer risen = Published(runtime);
        AssertEqual(new KraidBodyFrame(KraidBodyPage.Body, null, KraidBodyPage.Body), state.DisplayedBody, "the risen body shows both pages");
        AssertTrue(risen.Tilemap.Words.AsSpan().SequenceEqual(KraidBodyFixtures.Tilemap(upper, [], lower)),
            "the risen body publishes both working pages, blank elsewhere");
        AssertEqual(state.Bg2Scroll, risen.Scroll, "the body publishes at Kraid's BG2 scroll");
        var drawn = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(ReferenceEquals(risen.Tilemap, drawn.Rooms.BossBg2) && drawn.Registers.Bg2MapSize == risen.Tilemap.Size,
            "the capture draws the published body on BG2 at its map size");

        // Mouth open: the head frame shows over the upper page, which a later upload would cover.
        KraidHead.TransferKraidHeadTilemap(state, KraidHeadFrame.Open);
        state.Bg2Scroll = new Position16<Bg2Pixels>(300, 96);
        BossBg2Layer roaring = Published(runtime);
        ReadOnlySpan<ushort> head = (art.KraidBackground ?? throw new InvalidOperationException("Kraid art installs his BG2."))
            .HeadWords(KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Open));
        AssertTrue(roaring.Tilemap.Words.AsSpan().SequenceEqual(KraidBodyFixtures.Tilemap(upper, head, lower)),
            "the open mouth publishes its head over the upper page");
        AssertEqual(state.Bg2Scroll, roaring.Scroll, "the roaring body publishes at its moved scroll");

        // Dying: the death clear releases BG2 to the room.
        state.OwnsBg2Tilemap = false;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(runtime.Display.Latched.Bosses.KraidBody is null, "a released body publishes nothing");
        var released = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(released.Rooms.BossBg2 is null, "BG2 shows the room once Kraid releases it");
        Console.WriteLine("  Kraid body: risen, roaring and released frames publish at Kraid's BG2 scroll.");
    }

    /// <summary>Kraid's body as the next accepted NMI latches it.</summary>
    private static BossBg2Layer Published(SuperMetroidRuntime runtime)
    {
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        return runtime.Display.Latched.Bosses.KraidBody ?? throw new InvalidOperationException("Kraid owning BG2 publishes his body.");
    }

    /// <summary>Runs the Kraid body publication check.</summary>
    [VerificationCommand("--kraid-body")]
    internal static int KraidBodyCommand()
    {
        Suite(nameof(VerifyKraidBodyPublication), () => VerifyKraidBodyPublication());
        return 0;
    }
}
