using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Bosses;
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

using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks the body frame Kraid publishes on BG2 instead of writing tilemap pages into VRAM (#1352).</summary>
internal static class KraidBodyPublicationVerification
{
    /// <summary>
    /// Constructed Kraid states publish their body placed at Kraid's position (#2031): the risen whole body, then the open
    /// mouth's head over it; once his death hides it, the body shows nothing and BG2 does not show the room again.
    /// </summary>
    internal static void VerifyKraidBodyPublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Kraid);
        KraidEnemyState state = runtime.Enemies.Kraid
            ?? throw new InvalidOperationException("Kraid's room loads Kraid.");
        // The retail runtime draws Kraid's body from the installation's body sheet, composed from these frames.
        BossSourceArtwork art = InstalledFixtures.BossArtwork;
        ushort[] working = KraidBodyFixtures.WorkingMap(art);
        ReadOnlySpan<ushort> upper = working.AsSpan(0, KraidWorkingMapDefinitions.PageCells);
        ReadOnlySpan<ushort> lower = working.AsSpan(KraidWorkingMapDefinitions.LowerPageFirstCell, KraidWorkingMapDefinitions.PageCells);
        AssertTrue(state.ShowsBodyLayer, "Kraid's body layer shows when his room loads");

        // Risen: his body shown as it now appears, with the head it rises with before his head shows a pose, placed at his
        // position (#1515, #1540, #2031).
        RoomEnemyPlace body = runtime.Enemies.KraidBehavior.Parts.Body;
        state.BodyAppearance = KraidBodyAppearance.BehindSprites;
        state.ShowBody();
        state.DisplayedHeadFrame = null;
        body.Position = body.Position.WithWhole(new(320, 88));
        GameplayLatchedBossBody risen = Published(runtime);
        AssertTrue((risen.Body?.Frames.Select(frame => frame.Name) ?? []).SequenceEqual(["Body", "RestingHead"]),
            "the risen body shows his body and the head it rises with");
        AssertTrue(Words(risen, art).AsSpan().SequenceEqual(KraidBodyFixtures.Tilemap(upper, [], lower)),
            "the risen body publishes both working pages, blank elsewhere");
        AssertTrue(risen.Placement == body.Position.Whole, "the body publishes placed where Kraid lies in the room");
        var drawn = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(BossBodyDisplay.ShowsLatchedBody(drawn, runtime), "the capture draws the published body on the background");

        // Mouth open: his head's current pose shows over his body (#1515).
        state.DisplayedHeadFrame = KraidHeadFrame.Open;
        body.Position = body.Position.WithWhole(new(300, 96));
        GameplayLatchedBossBody roaring = Published(runtime);
        ReadOnlySpan<ushort> head = (art.KraidBackground ?? throw new InvalidOperationException("Kraid art installs his BG2."))
            .HeadWords(KraidHeadFrameDefinitions.PresentationKey(KraidHeadFrame.Open));
        AssertTrue(Words(roaring, art).AsSpan().SequenceEqual(KraidBodyFixtures.Tilemap(upper, head, lower)),
            "the open mouth publishes its head over the upper page");
        AssertTrue(roaring.Placement == body.Position.Whole, "the roaring body publishes placed where he moved in the room");

        // Dead: the death clear keeps BG2 as Kraid's map, every word blank; the room's own BG2 never returns (#1427).
        state.BodyAppearance = KraidBodyAppearance.Hidden;
        GameplayLatchedBossBody cleared = Published(runtime);
        AssertTrue(Words(cleared, art).All(word => word == KraidWorkingMapDefinitions.BlankTile.PackedWord),
            "the cleared body publishes only the blank tile");
        var dead = (GameplaySceneLayer)GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()).Layers[0];
        AssertTrue(BossBodyDisplay.ShowsLatchedBody(dead, runtime), "BG2 shows the cleared map, not the room's");
        Console.WriteLine("  Kraid body: risen, roaring and cleared frames publish where Kraid's body lies in the room.");
    }

    /// <summary>Kraid's body as the next accepted NMI latches it.</summary>
    private static GameplayLatchedBossBody Published(SuperMetroidRuntime runtime)
    {
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        GameplayLatchedBossBody bosses = runtime.Display.LatchedOrdinaryRoom.Bosses;
        return bosses.Body is not null && bosses.Placement is not null ? bosses
            : throw new InvalidOperationException("Kraid owning BG2 publishes his body.");
    }

    /// <summary>The tilemap entry each cell of the latched Kraid body shows, from the frames <paramref name="art"/> installs.</summary>
    private static ushort[] Words(GameplayLatchedBossBody bosses, BossSourceArtwork art) =>
        BossBodyWords.Of(bosses.Body ?? throw new InvalidOperationException("Kraid owning BG2 publishes his body."), BossBodyOwner.Kraid, art);

    /// <summary>Runs the Kraid body publication check.</summary>
    [VerificationCommand("--kraid-body")]
    internal static int KraidBodyCommand()
    {
        Suite(nameof(VerifyKraidBodyPublication), () => VerifyKraidBodyPublication());
        return 0;
    }
}
