using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Assets;
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
        ReadOnlySpan<ushort> upper = working.AsSpan(0, KraidWorkingMapDefinitions.PageCells);
        ReadOnlySpan<ushort> lower = working.AsSpan(KraidWorkingMapDefinitions.LowerPageFirstCell, KraidWorkingMapDefinitions.PageCells);
        AssertTrue(state.ShowsBodyLayer, "Kraid's body layer shows when his room loads");

        // Risen: both halves of his body shown, at the placement his body position computed.
        state.ShowUpperBody();
        state.ShowLowerBody();
        state.BodyLayerPlacement = new Position16<BackgroundPixels>(320, 88);
        GameplayLatchedBossBackgrounds risen = Published(runtime);
        AssertEqual(new KraidBodyFrame(KraidBodyAppearance.BehindSprites, null, KraidBodyAppearance.BehindSprites), state.ShownBody,
            "the risen body shows both halves");
        AssertTrue(Words(risen, art).AsSpan().SequenceEqual(KraidBodyFixtures.Tilemap(upper, [], lower)),
            "the risen body publishes both working pages, blank elsewhere");
        AssertTrue(risen.KraidBodyPlacement == state.BodyLayerPlacement, "the body publishes at Kraid's body layer placement");
        var drawn = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime) && drawn.View.BackgroundMapSize == KraidBackgroundDefinitions.BodyPlaneSize,
            "the capture draws the published body on BG2 at its map size");

        // Mouth open: the head frame shows over the upper page, which a later upload would cover.
        state.ShowHead(KraidHeadFrame.Open);
        state.BodyLayerPlacement = new Position16<BackgroundPixels>(300, 96);
        GameplayLatchedBossBackgrounds roaring = Published(runtime);
        ReadOnlySpan<ushort> head = (art.Sources.KraidBackground ?? throw new InvalidOperationException("Kraid art installs his BG2."))
            .HeadWords(KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Open));
        AssertTrue(Words(roaring, art).AsSpan().SequenceEqual(KraidBodyFixtures.Tilemap(upper, head, lower)),
            "the open mouth publishes its head over the upper page");
        AssertTrue(roaring.KraidBodyPlacement == state.BodyLayerPlacement, "the roaring body publishes at its moved placement");

        // Dead: the death clear keeps BG2 as Kraid's map, every word blank; the room's own BG2 never returns (#1427).
        state.ShownBody = KraidBodyFrame.Hidden;
        GameplayLatchedBossBackgrounds cleared = Published(runtime);
        AssertTrue(Words(cleared, art).All(word => word == KraidWorkingMapDefinitions.BlankTile.PackedWord),
            "the cleared body publishes only the blank tile");
        var dead = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(BossBodyDisplay.ShowsLatchedBody(dead, runtime), "BG2 shows the cleared map, not the room's");
        Console.WriteLine("  Kraid body: risen, roaring and cleared frames publish at Kraid's body layer placement.");
    }

    /// <summary>Kraid's body as the next accepted NMI latches it.</summary>
    private static GameplayLatchedBossBackgrounds Published(SuperMetroidRuntime runtime)
    {
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        GameplayLatchedBossBackgrounds bosses = runtime.Display.Latched.Bosses;
        return bosses.Body is not null && bosses.KraidBodyPlacement is not null ? bosses
            : throw new InvalidOperationException("Kraid owning BG2 publishes his body.");
    }

    /// <summary>The tilemap entry each cell of the latched Kraid body shows, from the frames <paramref name="art"/> installs.</summary>
    private static ushort[] Words(GameplayLatchedBossBackgrounds bosses, BossArtworkCatalog art) =>
        BossBodyWords.Of(bosses.Body ?? throw new InvalidOperationException("Kraid owning BG2 publishes his body."), BossBodyOwner.Kraid, art);

    /// <summary>Runs the Kraid body publication check.</summary>
    [VerificationCommand("--kraid-body")]
    internal static int KraidBodyCommand()
    {
        Suite(nameof(VerifyKraidBodyPublication), () => VerifyKraidBodyPublication());
        return 0;
    }
}
