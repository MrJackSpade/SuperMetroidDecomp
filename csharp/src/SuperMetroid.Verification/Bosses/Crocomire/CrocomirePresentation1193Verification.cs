using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
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
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Bosses.Crocomire;

using SuperMetroid.Verification.Harness;
/// <summary>Runtime checks for Crocomire's spike tile cadence and hidden-wall camera rumble.</summary>
internal static class CrocomirePresentation1193Verification
{
    /// <summary>Runs the retail Crocomire room and checks either NMI spike uploads or displayed BG1 rumble ordering.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static int VerifyCrocomirePresentation(bool spikes)
    {
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        var maps = InstalledFixtures.Maps;
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Crocomire);
        var boss = runtime.Enemies.CrocomireBehavior.Access.Crocomire;
        var death = runtime.Enemies.CrocomireBehavior.Access.CrocomireDeath;
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
        game.GameState = SuperMetroidGameState.MainGameplay;
        AssertEqual(RoomMainCallback.CrocomireRoomShaking, runtime.Room.LoadedRoom.State.MainCallback,
            "Actual Crocomire room selects native shaking callback");
        if (!spikes)
        {
            (boss ?? throw new InvalidOperationException("Verification requires boss.")).DeathSequenceIndex = CrocomireDeathPhase.RumbleHiddenWall;
            boss.Body.Properties = boss.Body.Properties.Without(EnemyProperties.Invisible);
            boss.RumbleTableOffset = 6;
            (death ?? throw new InvalidOperationException("Verification requires death.")).RumbleYOffset = 0;
            death.RumbleCooldown = WrappingCountdown.Start(10);
            death.RumbleDelta = 1;
        }
        int[] sources = [0x879d84, 0x879e04, 0x879e84, 0x879e04, 0x879d84];
        int sourceIndex = 0;
        ushort previousScroll = 0;
        for (int frame = 0; frame < (spikes ? 34 : 6); frame++)
        {
            ArtworkAnimation spikeTiles = AnimatedTileFrames.Of(AnimatedTileObject.HorizontalSpikes);
            ArtworkFrame? before = runtime.Room.LoadedScope.ArtworkFrames.TryShown(spikeTiles, out ArtworkFrame previous) ? previous : null;
            var result = game.Step(0);
            if (spikes)
            {
                AssertTrue(runtime.Room.LoadedScope.ArtworkFrames.TryShown(spikeTiles, out ArtworkFrame shown),
                    $"Crocomire spikes show a frame from tick {frame}");
                AssertEqual(frame % 8 == 0, before != shown, $"Crocomire spikes show their next frame only at tick {frame} of each eight");
                if (frame % 8 == 0)
                {
                    AssertEqual(sources[sourceIndex++], AnimatedTileFrameArtwork.SourceOf(shown), "Spikes follow 0/1/2/1/0 ping-pong sequence");
                    AssertTrue(maps.RoomFxAnimatedTiles.TryResolve(AnimatedTileFrameArtwork.SourceOf(shown), 0x80, out _),
                        "Installed spike frame of four 4bpp characters exists");
                }
            }
            else
            {
                ushort expected = unchecked((ushort)(runtime.BackgroundScroll.Layer1Position.Y +
                    runtime.BackgroundScroll.Bg1Offset.Y + (death ?? throw new InvalidOperationException("Verification requires death.")).RumbleYOffset));
                AssertEqual(expected, runtime.BackgroundScroll.Bg1Scroll.Y,
                    $"Comeback frame {frame} applies current enemy rumble to foreground scroll");
                if (frame != 0)
                {
                    AssertEqual(previousScroll, runtime.Display.DisplayedGameplayPpu.Bg1Scroll.Y,
                        "Rumble becomes visible at the following accepted NMI");
                    var layer = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame()).Layers[0];
                    AssertEqual(previousScroll, layer.View.Bg1Scroll.Y,
                        "Production foreground composition receives the displayed rumble scroll");
                }
                previousScroll = expected;
            }
        }
        if (spikes)
        {
            VerifyCrocomireSpikeCompatibility(runtime, maps.RoomFxAnimatedTiles);
            // A room load builds the room's spikes afresh, with their first frame pending.
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Crocomire);
            PlaceSamusInArena();
            samus.Xray.SuspendedSubsystems = XraySuspendedSubsystems.AnimatedTiles;
            game.Step(0);
            ArtworkAnimation spikeTiles = AnimatedTileFrames.Of(AnimatedTileObject.HorizontalSpikes);
            AssertEqual(false, runtime.Room.LoadedScope.ArtworkFrames.TryShown(spikeTiles, out _),
                "X-ray's animation-disable word suspends spikes at the actual runtime seam");
            samus.Xray.SuspendedSubsystems = XraySuspendedSubsystems.None;
            game.Step(0);
            AssertTrue(runtime.Room.LoadedScope.ArtworkFrames.TryShown(spikeTiles, out ArtworkFrame resumed) &&
                    resumed == AnimatedTileFrames.Frame(AnimatedTileObject.HorizontalSpikes, 0),
                "Re-enabled spikes resume the pending first frame");
            var unselected = new RoomSpikeAnimatedTilesState(null, null, AreaId.Norfair);
            var cleared = new ArtworkFrameSelections();
            unselected.Step(cleared);
            AssertTrue(!cleared.TryShown(spikeTiles, out _), "A room without selected spikes builds an empty spike owner");
        }
        else
        {
            var scroll = runtime.BackgroundScroll;
            scroll.Layer1Position = scroll.Layer1Position.WithY(100);
            scroll.Bg1Offset = scroll.Bg1Offset.WithY(3);
            scroll.CalculateScrollRegisters();
            ushort bg2 = scroll.Bg2Scroll.Y;
            (death ?? throw new InvalidOperationException("Verification requires death.")).RumbleYOffset = -4;
            runtime.RoomMains.RunCrocomireComebackRoomMain(runtime.Room.LoadedScope);
            AssertEqual((ushort)99, scroll.Bg1Scroll.Y, "Native rumble includes BG1's ordinary Y offset");
            AssertEqual(bg2, scroll.Bg2Scroll.Y, "Hidden-wall rumble leaves BG2 unchanged");
            AssertEqual((ushort)100, scroll.Layer1Position.Y, "Rumble does not move the camera or streaming origin");
            (boss ?? throw new InvalidOperationException("Verification requires boss.")).Body.Properties = boss.Body.Properties.With(EnemyProperties.Invisible);
            scroll.CalculateScrollRegisters();
            runtime.RoomMains.RunCrocomireComebackRoomMain(runtime.Room.LoadedScope);
            AssertEqual((ushort)103, scroll.Bg1Scroll.Y, "Invisible body suppresses the native room callback");
            boss.Body.Properties = boss.Body.Properties.Without(EnemyProperties.Invisible);
            boss.DeathSequenceIndex = CrocomireDeathPhase.BreakSpikeWall;
            death.RumbleYOffset = 0x8080;
            scroll.CalculateScrollRegisters();
            runtime.RoomMains.RunCrocomireComebackRoomMain(runtime.Room.LoadedScope);
            AssertEqual((ushort)103, scroll.Bg1Scroll.Y, "Leaving phase 40 restores ordinary scroll and excludes the terminator word");
        }
        Console.WriteLine(spikes ? "Crocomire spike cadence and NMI artwork verified." : "Crocomire comeback scroll and NMI ordering verified.");
        return 0;
    }

    /// <summary>Checks that a pre-spike PNG inherits every new stock spike tile byte-for-byte from the cartridge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the bank-$87 spike tile sources a legacy room-FX PNG inherits")]
    private static void AssertInheritedSpikeArtworkMatchesCartridge(ISnesAddressSpace rom, RoomFxAnimatedTileAtlas migrated)
    {
        foreach (int source in new[] { 0x879d84, 0x879e04, 0x879e84 })
        {
            AssertTrue(migrated.TryResolve(source, 0x80, out var bytes), "Legacy PNG inherits new stock spike art");
            for (int i = 0; i < bytes.Length; i++)
                AssertEqual(rom.ReadCartridgeByte(source + i), bytes.Span[i], "Inherited spike bytes match the pinned cartridge");
        }
    }

    /// <summary>Checks legacy PNG inheritance and debugger graph restoration for the room's animated spike tiles.</summary>
    private static void VerifyCrocomireSpikeCompatibility(SuperMetroidRuntime runtime,
        RoomFxAnimatedTileAtlas stock)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var png = new MemoryStream(RoomFxAnimatedTileAtlasExtractor.Extract(rom));
        var image = IndexedPng.Read(png, new(RoomFxAnimatedTileAtlasFormat.Width, 8));
        int oldWidth = RoomFxAnimatedTileAtlasFormat.PreSpikesWidth;
        var pixels = new byte[oldWidth * 8];
        for (int row = 0; row < 8; row++)
            image.Pixels.AsSpan(row * image.Size.X, oldWidth).CopyTo(pixels.AsSpan(row * oldWidth));
        pixels[0] ^= 1;
        using var legacyPng = new MemoryStream();
        IndexedPng.Write(legacyPng, new(checked((ushort)oldWidth), 8), pixels, SnesGraphics.DiagnosticPalette(4));
        legacyPng.Position = 0;
        var migrated = RoomFxAnimatedTileAtlasPng.Read(legacyPng, stock);
        int oldSource = RoomFxAnimatedTileArtworkDefinitions.MaridiaSandCeilingFirstSource;
        AssertTrue(stock.TryResolve(oldSource, 0x40, out var original) &&
            migrated.TryResolve(oldSource, 0x40, out var edited) && !original.Span.SequenceEqual(edited.Span),
            "Pre-spike PNG retains the user's existing edited prefix");
        AssertInheritedSpikeArtworkMatchesCartridge(rom, migrated);

        using var graph = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(graph, runtime.RoomSpikes);
        graph.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<RoomSpikeAnimatedTilesState>(graph);
        var expected = new ArtworkFrameSelections();
        var actual = new ArtworkFrameSelections();
        for (int tick = 0; tick < 8; tick++)
        {
            runtime.RoomSpikes.Step(expected);
            restored.Step(actual);
            ArtworkAnimation spikeAnimation = AnimatedTileFrames.Of(AnimatedTileObject.HorizontalSpikes);
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
