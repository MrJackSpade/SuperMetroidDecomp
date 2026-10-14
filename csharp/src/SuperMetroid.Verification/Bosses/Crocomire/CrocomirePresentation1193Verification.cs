using System.Reflection;
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
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Diagnostics;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
/// <summary>Runtime checks for Crocomire's spike tile cadence and hidden-wall camera rumble.</summary>
internal static class CrocomirePresentation1193Verification
{
    /// <summary>Runs the retail Crocomire room and checks either NMI spike uploads or displayed BG1 rumble ordering.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static int VerifyCrocomirePresentation(bool spikes)
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var maps = InstalledFixtures.Maps;
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        game.CreateGameplayRuntime(false);
        var runtime=game.runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(0xa98d);
        var boss = runtime.Enemies.CrocomireBehavior.Crocomire;
        var death = runtime.Enemies.CrocomireBehavior.CrocomireDeath;
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Health = samus.MaxHealth = 999;
        samus.Position = samus.Position.WithWhole(new(600, 120));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        samus.InputLocked = false;
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(512, 0));
        game.GameState = SuperMetroidGameState.MainGameplay;
        AssertEqual(RoomMainCallback.CrocomireRoomShaking, runtime.Room.LoadedRoom.State.MainCallback,
            "Actual Crocomire room selects native shaking callback");
        if (!spikes)
        {
            (boss ?? throw new InvalidOperationException("Verification requires boss.")).DeathSequenceIndex = CrocomireDeathPhase.RumbleHiddenWall;
            boss.Body.Properties = boss.Body.Properties.Without(EnemyProperties.Invisible);
            boss.RumbleTableOffset = 6;
            (death ?? throw new InvalidOperationException("Verification requires death.")).RumbleYOffset = 0;
            death.RumbleCooldown = NativeCountdown.Start(10);
            death.RumbleDelta = 1;
        }
        int[] sources = [0x879d84, 0x879e04, 0x879e84, 0x879e04, 0x879d84];
        int sourceIndex = 0;
        ushort previousScroll = 0;
        for (int frame = 0; frame < (spikes ? 34 : 6); frame++)
        {
            var result = game.Step(0);
            if (spikes)
            {
                var uploads = runtime.VramWrites.Entries.Where(write => write.EncodedVramDestination == 0x3d60).ToArray();
                AssertEqual(frame % 8 == 0 ? 1 : 0, uploads.Length,
                    $"Crocomire spikes queue one native frame at tick {frame}");
                if (uploads.Length != 0)
                {
                    AssertEqual(sources[sourceIndex++], uploads[0].SourceAddress, "Spikes follow 0/1/2/1/0 ping-pong sequence");
                    AssertEqual((ushort)0x80, uploads[0].SizeInBytes, "Spikes transfer four 4bpp characters");
                }
                if (frame != 0)
                {
                    int displayedSource = sources[(frame - 1) / 8];
                    AssertTrue(maps.RoomFxAnimatedTiles.TryResolve(displayedSource, 0x80, out var bytes),
                        "Installed spike frame exists");
                    for (int i = 0; i < bytes.Length; i++)
                        AssertEqual(bytes.Span[i], runtime.Vram.ReadByte(0x3d60 * 2 + i), "NMI displays exact spike artwork bytes");
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
                    var layer = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
                    AssertEqual(previousScroll, layer.Registers.Bg1Scroll.Y,
                        "Production foreground composition receives the displayed rumble scroll");
                }
                previousScroll = expected;
            }
        }
        if (spikes)
        {
            VerifyCrocomireSpikeCompatibility(runtime, bus, maps.RoomFxAnimatedTiles);
            runtime.RoomSpikes.LoadRoom(bus, runtime.Room.LoadedRoom.State.FxPointer,
                (runtime.Room.ActiveDoor ?? throw new InvalidOperationException("Verification requires runtime.Room.ActiveDoor.")).Pointer, runtime.Room.LoadedRoom.AreaIndex);
            samus.Xray.SuspendedSubsystems = XraySuspendedSubsystems.AnimatedTiles;
            game.Step(0);
            AssertEqual(false, runtime.VramWrites.Entries.Any(write => write.EncodedVramDestination == 0x3d60),
                "X-ray's animation-disable word suspends spikes at the actual runtime seam");
            samus.Xray.SuspendedSubsystems = XraySuspendedSubsystems.None;
            game.Step(0);
            AssertEqual(0x879d84, runtime.VramWrites.Entries.Single(write => write.EncodedVramDestination == 0x3d60).SourceAddress,
                "Re-enabled spikes resume the pending first frame");
            runtime.RoomSpikes.LoadRoom(bus, 0, 0, AreaId.Norfair);
            var clearedWrites = new VramWriteQueue();
            runtime.RoomSpikes.Step(bus, runtime.Vram, clearedWrites);
            AssertEqual(0, clearedWrites.Entries.Count, "Room replacement without selected spikes clears their owner");
        }
        else
        {
            var scroll = runtime.BackgroundScroll;
            scroll.Layer1Position = scroll.Layer1Position.WithY(100);
            scroll.Bg1Offset = scroll.Bg1Offset.WithY(3);
            scroll.CalculateScrollsAndUpdates();
            ushort bg2 = scroll.Bg2Scroll.Y;
            (death ?? throw new InvalidOperationException("Verification requires death.")).RumbleYOffset = unchecked((ushort)-4);
            runtime.RoomMains.RunCrocomireComebackRoomMain();
            AssertEqual((ushort)99, scroll.Bg1Scroll.Y, "Native rumble includes BG1's ordinary Y offset");
            AssertEqual(bg2, scroll.Bg2Scroll.Y, "Hidden-wall rumble leaves BG2 unchanged");
            AssertEqual((ushort)100, scroll.Layer1Position.Y, "Rumble does not move the camera or streaming origin");
            (boss ?? throw new InvalidOperationException("Verification requires boss.")).Body.Properties = boss.Body.Properties.With(EnemyProperties.Invisible);
            scroll.CalculateScrollsAndUpdates();
            runtime.RoomMains.RunCrocomireComebackRoomMain();
            AssertEqual((ushort)103, scroll.Bg1Scroll.Y, "Invisible body suppresses the native room callback");
            boss.Body.Properties = boss.Body.Properties.Without(EnemyProperties.Invisible);
            boss.DeathSequenceIndex = CrocomireDeathPhase.BreakSpikeWall;
            death.RumbleYOffset = 0x8080;
            scroll.CalculateScrollsAndUpdates();
            runtime.RoomMains.RunCrocomireComebackRoomMain();
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
        ISnesAddressSpace bus, RoomFxAnimatedTileAtlas stock)
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
        var migrated = RoomFxAnimatedTileAtlas.Load(legacyPng, stock);
        int oldSource = RoomFxAnimatedTileArtworkDefinitions.MaridiaSandCeilingFirstSource;
        AssertTrue(stock.TryResolve(oldSource, 0x40, out var original) &&
            migrated.TryResolve(oldSource, 0x40, out var edited) && !original.Span.SequenceEqual(edited.Span),
            "Pre-spike PNG retains the user's existing edited prefix");
        AssertInheritedSpikeArtworkMatchesCartridge(rom, migrated);

        using var graph = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(graph, runtime.RoomSpikes);
        graph.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<RoomSpikeAnimatedTilesState>(graph);
        for (int tick = 0; tick < 8; tick++)
        {
            var expected = new VramWriteQueue();
            var actual = new VramWriteQueue();
            runtime.RoomSpikes.Step(bus, runtime.Vram, expected);
            restored.Step(bus, runtime.Vram, actual);
            AssertTrue(expected.Entries.SequenceEqual(actual.Entries), "Current debugger graph retains exact spike cadence");
        }
        runtime.RoomSpikes = new RoomSpikeAnimatedTilesState();
        var restarted = new VramWriteQueue();
        runtime.RoomSpikes.Step(bus, runtime.Vram, restarted);
        AssertEqual(0x879d84, restarted.Entries.Single().SourceAddress,
            "Absent legacy spike state restarts the current room's selected object");
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
