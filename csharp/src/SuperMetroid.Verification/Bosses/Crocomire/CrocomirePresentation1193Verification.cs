using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Bosses.Crocomire;

using SuperMetroid.Verification.Harness;
/// <summary>Runtime checks for Crocomire's spike tile cadence and hidden-wall camera rumble.</summary>
internal static class CrocomirePresentation1193Verification
{
    /// <summary>Runs the retail Crocomire room and checks either NMI spike uploads or displayed BG1 rumble ordering.</summary>
    internal static int VerifyCrocomirePresentation(bool spikes)
    {
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps));
        var maps = InstalledFixtures.Maps;
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Crocomire);
        var boss = runtime.Enemies.BossEncounters.Crocomire;
        var death = boss?.Death;
        var samus = runtime.Player.IntroducedSamus;
        PlaceSamusInArena();

        void PlaceSamusInArena()
        {
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.Health = samus.MaxHealth = 999;
            samus.Position = samus.Position.WithWhole(new(600, 120));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.CommitPoseHistory();
            samus.Control.Unlock();
            runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(512, 0));
        }
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        AssertEqual(RoomMainBehavior.CrocomireRoomShaking, runtime.Room.LoadedRoom.State.MainBehavior,
            "Actual Crocomire room selects native shaking callback");
        if (!spikes)
        {
            (boss ?? throw new InvalidOperationException("Verification requires boss.")).DeathStage = CrocomireDeathStages.Only<CrocomireDeathStage.RumbleHiddenWall>();
            boss.Body.Presence = boss.Body.Presence.Without(EnemyPresence.Hidden);
            boss.RumbleOscillation = 0;
            boss.RumbleTowardExtreme = true;
            (death ?? throw new InvalidOperationException("Verification requires death.")).RumbleYOffset = 0;
            death.RumbleRepeatsLeft = 10;
        }
        int[] sources = [0x879d84, 0x879e04, 0x879e84, 0x879e04, 0x879d84];
        int sourceIndex = 0;
        int previousScroll = 0;
        for (int frame = 0; frame < (spikes ? 34 : 6); frame++)
        {
            ArtworkAnimation spikeTiles = BlockAnimationFrames.Of(BlockAnimation.HorizontalSpikes);
            ArtworkFrame? before = runtime.Room.LoadedScope.ArtworkFrames.TryShown(spikeTiles, out ArtworkFrame previous) ? previous : null;
            game.Step(0);
            if (spikes)
            {
                AssertTrue(runtime.Room.LoadedScope.ArtworkFrames.TryShown(spikeTiles, out ArtworkFrame shown),
                    $"Crocomire spikes show a frame from tick {frame}");
                AssertEqual(frame % 8 == 0, before != shown, $"Crocomire spikes show their next frame only at tick {frame} of each eight");
                if (frame % 8 == 0)
                {
                    AssertEqual(sources[sourceIndex++], AnimatedTileFrameArtwork.SourceOf(shown), "Spikes follow 0/1/2/1/0 ping-pong sequence");
                    AssertTrue(InstalledFixtures.ImportOf(maps).BlockAnimationAtlas.TryResolve(AnimatedTileFrameArtwork.SourceOf(shown), 0x80, out _),
                        "Installed spike frame of four 4bpp characters exists");
                }
            }
            else
            {
                // The camera stands at the room's top, so the rumble's upward swing leaves a negative row (#2384).
                int expected = runtime.BackgroundScroll.ForegroundPosition.Y +
                    (death ?? throw new InvalidOperationException("Verification requires death.")).RumbleYOffset;
                AssertEqual(expected, runtime.BackgroundScroll.ForegroundScroll.Y,
                    $"Comeback frame {frame} applies current enemy rumble to foreground scroll");
                if (frame != 0)
                {
                    AssertEqual(previousScroll, runtime.Display.DisplayedGameplayScrolls.ForegroundScroll.Y,
                        "Rumble becomes visible at the following accepted NMI");
                    var layer = runtime.PublishDisplayFrame().OrdinaryBase.Room;
                    // The view shows the foreground row below the displayed camera row at the screen's top (#1514).
                    AssertEqual(previousScroll, (int)layer.View.ForegroundScroll.Y,
                        "Production foreground composition receives the displayed rumble scroll as a plain signed row");
                }
                previousScroll = expected;
            }
        }
        if (spikes)
        {
            VerifyCrocomireSpikeGraphRestoration(runtime);
            // A room load builds the room's spikes afresh, with their first frame pending.
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Crocomire);
            PlaceSamusInArena();
            PrivateState.Set(samus.Xray, nameof(SamusXrayState.SuspendedSubsystems), XraySuspendedSubsystems.BlockAnimations);
            game.Step(0);
            ArtworkAnimation spikeTiles = BlockAnimationFrames.Of(BlockAnimation.HorizontalSpikes);
            AssertEqual(false, runtime.Room.LoadedScope.ArtworkFrames.TryShown(spikeTiles, out _),
                "X-ray's animation-disable word suspends spikes at the actual runtime seam");
            PrivateState.Set(samus.Xray, nameof(SamusXrayState.SuspendedSubsystems), XraySuspendedSubsystems.None);
            game.Step(0);
            AssertTrue(runtime.Room.LoadedScope.ArtworkFrames.TryShown(spikeTiles, out ArtworkFrame resumed) &&
                    resumed == BlockAnimationLoops.Frames(BlockAnimation.HorizontalSpikes)[0].Frame,
                "Re-enabled spikes resume the pending first frame");
            var unselected = new RoomBlockAnimations(null, null);
            var cleared = new ArtworkFrameSelections();
            unselected.Step(areaBossDefeated: false, cleared);
            AssertTrue(!cleared.TryShown(spikeTiles, out _), "A room without selected spikes builds an empty spike owner");
        }
        else
        {
            var scroll = runtime.BackgroundScroll;
            scroll.ForegroundPosition = scroll.ForegroundPosition.WithY(100);
            scroll.CalculateLayerScrolls();
            short bg2 = scroll.BackgroundScroll.Y;
            (death ?? throw new InvalidOperationException("Verification requires death.")).RumbleYOffset = -4;
            runtime.RoomMains.RunCrocomireComebackRoomMain(boss ?? throw new InvalidOperationException("Verification requires boss."));
            AssertEqual(96, scroll.ForegroundScroll.Y, "The rumble shifts the foreground's view from the camera");
            AssertEqual(bg2, scroll.BackgroundScroll.Y, "Hidden-wall rumble leaves BG2 unchanged");
            AssertEqual((ushort)100, scroll.ForegroundPosition.Y, "Rumble does not move the camera or streaming origin");
            // With the camera at the room's top, the rumble's upward swing takes the view above the room (#2384): the view's
            // row is the plain sum, negative, and the frame shows the foreground's signed row with no wrap.
            scroll.ForegroundPosition = scroll.ForegroundPosition.WithY(0);
            scroll.CalculateLayerScrolls();
            runtime.RoomMains.RunCrocomireComebackRoomMain(boss);
            AssertEqual(-4, scroll.ForegroundScroll.Y, "The rumble above the room's top leaves a negative foreground view row");
            runtime.FrameBoundary.PublishWhileLoading();
            var risen = runtime.PublishDisplayFrame().OrdinaryBase.Room;
            AssertEqual(-4, (int)risen.View.ForegroundScroll.Y,
                "The frame shows the risen view's signed row at the screen's top");
            scroll.ForegroundPosition = scroll.ForegroundPosition.WithY(100);
            boss.Body.Presence = boss.Body.Presence.With(EnemyPresence.Hidden);
            scroll.CalculateLayerScrolls();
            runtime.RoomMains.RunCrocomireComebackRoomMain(boss);
            AssertEqual(100, scroll.ForegroundScroll.Y, "Invisible body suppresses the native room callback");
            boss.Body.Presence = boss.Body.Presence.Without(EnemyPresence.Hidden);
            boss.DeathStage = CrocomireDeathStages.Only<CrocomireDeathStage.BreakSpikeWall>();
            death.RumbleYOffset = 0x8080;
            scroll.CalculateLayerScrolls();
            runtime.RoomMains.RunCrocomireComebackRoomMain(boss);
            AssertEqual(100, scroll.ForegroundScroll.Y, "Leaving phase 40 restores ordinary scroll and excludes the terminator word");
        }
        Console.WriteLine(spikes ? "Crocomire spike cadence and NMI artwork verified." : "Crocomire comeback scroll and NMI ordering verified.");
        return 0;
    }

    /// <summary>Checks that the debugger graph restores the room's animated spike cadence.</summary>
    private static void VerifyCrocomireSpikeGraphRestoration(SuperMetroidRuntime runtime)
    {
        using var graph = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(graph, runtime.BlockAnimations);
        graph.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<RoomBlockAnimations>(graph);
        var expected = new ArtworkFrameSelections();
        var actual = new ArtworkFrameSelections();
        for (int tick = 0; tick < 8; tick++)
        {
            runtime.BlockAnimations.Step(areaBossDefeated: false, expected);
            restored.Step(areaBossDefeated: false, actual);
            ArtworkAnimation spikeAnimation = BlockAnimationFrames.Of(BlockAnimation.HorizontalSpikes);
            AssertTrue(expected.TryShown(spikeAnimation, out ArtworkFrame expectedFrame) == actual.TryShown(spikeAnimation, out ArtworkFrame actualFrame) &&
                expectedFrame == actualFrame, "Current debugger graph retains exact spike cadence");
        }
        // Debugger states are build-pinned (#1270); an older graph without spike state is rejected, not migrated.
    }

    /// <summary>Runs the spike animation and NMI artwork verification mode.</summary>
    [VerificationCommand("--crocomire-spike-animation")]
    internal static int CrocomireSpikeAnimationCommand()
    {
        return VerifyCrocomirePresentation(true);
    }

    /// <summary>Runs the hidden-wall rumble and foreground scroll verification mode.</summary>
    [VerificationCommand("--crocomire-comeback-rumble")]
    internal static int CrocomireComebackRumbleCommand()
    {
        return VerifyCrocomirePresentation(false);
    }
}
