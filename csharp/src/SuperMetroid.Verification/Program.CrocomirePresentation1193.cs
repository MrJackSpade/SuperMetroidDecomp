using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
internal static partial class Program
{
    private static int VerifyCrocomirePresentation(bool spikes)
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var maps = RepositoryInstallation.Maps;
        game.BindMapPresentation(maps);
        game.BindGameplayBasePalettes(installation.LoadGameplayBasePalettes());
        game.BindStandardObjectArt(installation.LoadStandardObjects());
        game.BindIntroCinematicArt(RepositoryInstallation.IntroArtwork);
        game.BindSamusBodyArt(RepositoryInstallation.SamusBody);
        game.BindEndingMode7Art(installation.LoadEndingMode7Art());
        game.BindEndingObjectArt(installation.LoadEndingObjectArt());
        game.BindEndingPaletteArt(installation.LoadEndingPalettes());
        game.BindRoomCharacterArt(installation.LoadRoomCharacters());
        game.BindRoomPaletteArt(installation.LoadRoomPalettes());
        game.BindRoomMetatileArt(installation.LoadRoomMetatiles());
        game.BindRoomVisualLayouts(installation.LoadRoomVisualLayouts());
        game.BindRoomPlmShotBlockVisuals(installation.LoadRoomPlmShotBlockVisuals());
        game.BindRoomPlmGrappleBlockVisuals(installation.LoadRoomPlmGrappleBlockVisuals());
        game.BindRoomPlmStationVisuals(installation.LoadRoomPlmStationVisuals());
        game.BindRoomPlmBlueDoorVisuals(installation.LoadRoomPlmBlueDoorVisuals());
        game.BindRoomPlmColoredDoorVisuals(installation.LoadRoomPlmColoredDoorVisuals());
        game.BindRoomPlmGreyDoorVisuals(installation.LoadRoomPlmGreyDoorVisuals());
        game.BindRoomPlmEyeDoorVisuals(installation.LoadRoomPlmEyeDoorVisuals());
        game.BindRoomPlmMotherBrainGlassVisuals(installation.LoadRoomPlmMotherBrainGlassVisuals());
        game.BindRoomPlmNoobTubeVisuals(installation.LoadRoomPlmNoobTubeVisuals());
        game.BindRoomPlmDownwardGateVisuals(installation.LoadRoomPlmDownwardGateVisuals());
        game.BindRoomPlmElevatorPlatformVisuals(installation.LoadRoomPlmElevatorPlatformVisuals());
        game.BindRoomPlmEscapeGateVisuals(installation.LoadRoomPlmEscapeGateVisuals());
        game.BindRoomPlmBombTorizoHandVisuals(installation.LoadRoomPlmBombTorizoHandVisuals());
        game.BindRoomPlmDraygonCannonVisuals(installation.LoadRoomPlmDraygonCannonVisuals());
        game.BindRoomPlmChozoStatueVisuals(installation.LoadRoomPlmChozoStatueVisuals());
        game.BindRoomPlmLinkedRestoreVisuals(installation.LoadRoomPlmLinkedRestoreVisuals());
        game.BindRoomPlmTourianAccessVisuals(installation.LoadRoomPlmTourianAccessVisuals());
        game.BindRoomPlmSpeedBoosterVisuals(installation.LoadRoomPlmSpeedBoosterVisuals());
        game.BindRoomPlmMaridiaElevatubeVisuals(installation.LoadRoomPlmMaridiaElevatubeVisuals());
        game.BindRoomPlmSporeSpawnCeilingVisuals(installation.LoadRoomPlmSporeSpawnCeilingVisuals());
        game.BindRoomPlmSamusEaterVisuals(installation.LoadRoomPlmSamusEaterVisuals());
        game.BindRoomPlmBotwoonWallVisuals(installation.LoadRoomPlmBotwoonWallVisuals());
        game.BindRoomPlmKraidVisuals(installation.LoadRoomPlmKraidVisuals());
        game.BindRoomPlmCrocomireVisuals(installation.LoadRoomPlmCrocomireVisuals());
        game.BindRoomPlmMotherBrainFakeDeathVisuals(installation.LoadRoomPlmMotherBrainFakeDeathVisuals());
        game.BindRoomPlmCollectibleVisuals(installation.LoadRoomPlmCollectibleVisuals());
        game.BindRoomPlmDynamicCollectibleArt(installation.LoadRoomPlmDynamicCollectibleArt());
        game.BindXrayRevealVisuals(installation.LoadXrayRevealVisuals());
        game.BindRoomBackgroundTilemapArt(installation.LoadRoomBackgroundTilemaps());
        game.BindRoomSkyTilemapArt(installation.LoadRoomSkyTilemaps());
        var projectiles=RepositoryInstallation.Projectiles;
        game.BindProjectileCompositions(projectiles.Catalog);
        game.BindProjectileFrameBindings(projectiles.FrameBindings);
        game.BindBeamArtwork(projectiles.BeamTiles);
        game.BindEnemyTileArtwork(RepositoryInstallation.EnemyTiles);
        game.BindTrailArtwork(projectiles.Trails);
        game.BindChargeFlarePlacement(projectiles.FlarePlacement);
        game.BindChargeFlareCompositions(projectiles.FlareCompositions);
        game.BindGrappleArtwork(projectiles.GrappleTiles);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(0xa98d);
        var boss = runtime.Enemies.Crocomire!;
        var death = runtime.Enemies.CrocomireDeath!;
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.Health = samus.MaxHealth = 999;
        samus.XPosition = 600;
        samus.YPosition = 120;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        samus.InputLocked = false;
        runtime.Camera!.SetPosition(512, 0);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game, SuperMetroidGameState.MainGameplay);
        AssertEqual(RoomMainCallback.CrocomireRoomShaking, runtime.ActiveRoom!.State.MainCallback,
            "Actual Crocomire room selects native shaking callback");
        if (!spikes)
        {
            boss.DeathSequenceIndex = CrocomireDeathPhases.RumbleHiddenWall;
            boss.Body.Properties = boss.Body.Properties.Without(EnemyProperties.Invisible);
            boss.StepCounter = 6;
            death.RumbleYOffset = 0;
            death.RumbleCooldown = 10;
            death.RumbleDelta = 1;
        }
        var renderer = new CartridgeAudioRenderer(installation.LoadAudio());
        int[] sources = [0x879d84, 0x879e04, 0x879e84, 0x879e04, 0x879d84];
        int sourceIndex = 0;
        ushort previousScroll = 0;
        for (int frame = 0; frame < (spikes ? 34 : 6); frame++)
        {
            var result = game.Step(0);
            renderer.RenderFrame(result.AudioCommands);
            game.SetAudioAcknowledgements(renderer.ReadAcknowledgements());
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
                ushort expected = unchecked((ushort)(runtime.BackgroundScroll.Layer1YPosition +
                    runtime.BackgroundScroll.Bg1YOffset + death.RumbleYOffset));
                AssertEqual(expected, runtime.BackgroundScroll.Bg1VerticalScroll,
                    $"Comeback frame {frame} applies current enemy rumble to foreground scroll");
                if (frame != 0)
                {
                    AssertEqual(previousScroll, runtime.DisplayedGameplayPpu.Bg1VerticalScroll,
                        "Rumble becomes visible at the following accepted NMI");
                    var layer = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
                    AssertEqual(previousScroll, layer.Registers.Bg1Y,
                        "Production foreground composition receives the displayed rumble scroll");
                }
                previousScroll = expected;
            }
        }
        if (spikes)
        {
            VerifyCrocomireSpikeCompatibility(runtime, bus, maps.RoomFxAnimatedTiles);
            runtime.RoomSpikes.LoadRoom(bus, runtime.ActiveRoom.State.FxPointer,
                runtime.ActiveDoor!.Pointer, runtime.ActiveRoom.AreaIndex);
            typeof(SamusXrayState).GetProperty(nameof(SamusXrayState.SuspendedSubsystems))!
                .SetValue(samus.Xray, XraySuspendedSubsystems.AnimatedTiles);
            game.Step(0);
            AssertEqual(false, runtime.VramWrites.Entries.Any(write => write.EncodedVramDestination == 0x3d60),
                "X-ray's animation-disable word suspends spikes at the actual runtime seam");
            typeof(SamusXrayState).GetProperty(nameof(SamusXrayState.SuspendedSubsystems))!
                .SetValue(samus.Xray, XraySuspendedSubsystems.None);
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
            var roomMain = typeof(SuperMetroidRuntime).GetMethod("RunCrocomireComebackRoomMain", flags)!;
            var scroll = runtime.BackgroundScroll;
            scroll.Layer1YPosition = 100;
            scroll.Bg1YOffset = 3;
            scroll.CalculateScrollsAndUpdates();
            ushort bg2 = scroll.Bg2VerticalScroll;
            death.RumbleYOffset = unchecked((ushort)-4);
            roomMain.Invoke(runtime, null);
            AssertEqual((ushort)99, scroll.Bg1VerticalScroll, "Native rumble includes BG1's ordinary Y offset");
            AssertEqual(bg2, scroll.Bg2VerticalScroll, "Hidden-wall rumble leaves BG2 unchanged");
            AssertEqual((ushort)100, scroll.Layer1YPosition, "Rumble does not move the camera or streaming origin");
            boss.Body.Properties = boss.Body.Properties.With(EnemyProperties.Invisible);
            scroll.CalculateScrollsAndUpdates();
            roomMain.Invoke(runtime, null);
            AssertEqual((ushort)103, scroll.Bg1VerticalScroll, "Invisible body suppresses the native room callback");
            boss.Body.Properties = boss.Body.Properties.Without(EnemyProperties.Invisible);
            boss.DeathSequenceIndex = CrocomireDeathPhases.BreakSpikeWall;
            death.RumbleYOffset = 0x8080;
            scroll.CalculateScrollsAndUpdates();
            roomMain.Invoke(runtime, null);
            AssertEqual((ushort)103, scroll.Bg1VerticalScroll, "Leaving phase 40 restores ordinary scroll and excludes the terminator word");
        }
        Console.WriteLine(spikes ? "Crocomire spike cadence and NMI artwork verified." : "Crocomire comeback scroll and NMI ordering verified.");
        return 0;
    }

    private static void VerifyCrocomireSpikeCompatibility(SuperMetroidRuntime runtime,
        ISnesAddressSpace bus, RoomFxAnimatedTileAtlas stock)
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var png = new MemoryStream(RoomFxAnimatedTileAtlasExtractor.Extract(rom));
        var image = IndexedPng.Read(png, RoomFxAnimatedTileAtlasFormat.Width, 8);
        int oldWidth = RoomFxAnimatedTileAtlasFormat.PreSpikesWidth;
        var pixels = new byte[oldWidth * 8];
        for (int row = 0; row < 8; row++)
            image.Pixels.AsSpan(row * image.Width, oldWidth).CopyTo(pixels.AsSpan(row * oldWidth));
        pixels[0] ^= 1;
        using var legacyPng = new MemoryStream();
        IndexedPng.Write(legacyPng, oldWidth, 8, pixels, SnesGraphics.DiagnosticPalette(4));
        legacyPng.Position = 0;
        var migrated = RoomFxAnimatedTileAtlas.Load(legacyPng, stock);
        int oldSource = RoomFxAnimatedTileArtworkDefinitions.MaridiaSandCeilingFirstSource;
        AssertTrue(stock.TryResolve(oldSource, 0x40, out var original) &&
            migrated.TryResolve(oldSource, 0x40, out var edited) && !original.Span.SequenceEqual(edited.Span),
            "Pre-spike PNG retains the user's existing edited prefix");
        foreach (int source in new[] { 0x879d84, 0x879e04, 0x879e84 })
        {
            AssertTrue(migrated.TryResolve(source, 0x80, out var bytes), "Legacy PNG inherits new stock spike art");
            for (int i = 0; i < bytes.Length; i++)
                AssertEqual(rom.ReadByte(source + i), bytes.Span[i], "Inherited spike bytes match the pinned cartridge");
        }

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
        var fields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [typeof(SuperMetroidRuntime)])!;
        // Runtime fields published after the spike owner are absent from that layout too.
        string[] laterRuntimeFields = ["<RoomMainScratch>k__BackingField", "_pendingLoaderSamusPlacement", "_suspendedFrameTail"];
        FieldInfo[] preSpikeFields = fields
            .Where(field => !laterRuntimeFields.Contains(field.Name))
            .ToArray();
        AssertTrue(LegacyLayout(typeof(SuperMetroidRuntime), fields, preSpikeFields.Where(field => field.Name != "_roomSpikes"))
            .SequenceEqual(preSpikeFields.Where(field => field.Name != "_roomSpikes")),
            "Legacy runtime layout omits only the new spike owner");
        typeof(SuperMetroidRuntime).GetField("_roomSpikes", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(runtime, null);
        var restarted = new VramWriteQueue();
        runtime.RoomSpikes.Step(bus, runtime.Vram, restarted);
        AssertEqual(0x879d84, restarted.Entries.Single().SourceAddress,
            "Absent legacy spike state restarts the current room's selected object");
    }
}
