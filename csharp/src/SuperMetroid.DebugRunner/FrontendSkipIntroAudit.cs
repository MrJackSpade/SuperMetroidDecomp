using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// ROM-backed proof that the host option bypasses only the story cinematic and rejoins the
/// ordinary Ceres new-game path. Keeping this out of Program.cs avoids expanding its already
/// large front-end capture dispatcher for a configuration-boundary regression.
/// </summary>
internal static class FrontendSkipIntroAudit
{
    public static int Run(string romPath, string outputPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        int saveRamChangeCount = 0;
        game.SaveRamChanged += () => saveRamChangeCount++;

        FrontendFrame frame = game.Step(0);
        frame = game.Step((ushort)SnesButton.Start);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.Phase == nameof(TitleSequencePhase.TitleScreen),
            maximumFrames: 120,
            "title montage skip did not reach the title screen");

        frame = game.Step((ushort)SnesButton.Start);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.FileSelectMenus,
            maximumFrames: 120,
            "title screen did not reach file select");
        for (int frameIndex = 0; frameIndex < 16; frameIndex++)
            frame = game.Step(0);

        frame = game.Step((ushort)SnesButton.A);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.GameOptionsMenu,
            maximumFrames: 180,
            "fresh file did not reach the game-options screen");
        for (int frameIndex = 0; frameIndex < 16; frameIndex++)
            frame = game.Step(0);

        // Accept the options menu exactly as the playable keyboard host does. Every frame
        // is inspected so an accidental one-frame construction of IntroCinematic cannot be
        // hidden by the eventual arrival at Ceres.
        frame = game.Step((ushort)SnesButton.A);
        for (int frameIndex = 0;
            frameIndex < 180 && frame.GameState == SuperMetroidGameState.GameOptionsMenu;
            frameIndex++)
        {
            frame = game.Step(0);
            if (frame.GameState == SuperMetroidGameState.IntroCinematic)
                throw new InvalidOperationException("Skip option entered IntroCinematic.");
        }
        if (frame.GameState != SuperMetroidGameState.SetUpNewGame)
        {
            throw new InvalidOperationException(
                $"Skip option reached {frame.GameState}, not SetUpNewGame.");
        }

        // State $1F performs the authentic room/load-station initialization on the next
        // dispatcher call. Native state seven then fades complete gameplay/OAM frames in
        // before state $20 continues the bank-$86 elevator wait and descent.
        frame = game.Step(0);
        if (frame.GameState != SuperMetroidGameState.MainGameplayFadeIn)
            throw new InvalidOperationException("New-game setup did not enter the Ceres gameplay fade.");
        SuperMetroidSaveSlot saved = new SuperMetroidSaveRam(bus).ReadSlot(0)
            ?? throw new InvalidDataException("Ceres setup did not produce a valid slot-A SRAM checkpoint.");
        if (saved.Area != 6 || saved.SaveStation != 0 || saved.Health != 99)
        {
            throw new InvalidDataException(
                $"Ceres checkpoint decoded as area {saved.Area}, station {saved.SaveStation}, " +
                $"energy {saved.Health}, not 6:0/99.");
        }
        if (saveRamChangeCount != 2)
        {
            throw new InvalidOperationException(
                $"Expected selected-slot and checkpoint SRAM publications, observed {saveRamChangeCount}.");
        }
        var reloadedMenu = new FileSelectMenuState(bus);
        if (!reloadedMenu.SelectedSlotContainsSave)
            throw new InvalidDataException("A restarted file-select menu did not recognize the Ceres checkpoint.");
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.MainGameplay,
            maximumFrames: 180,
            "Ceres elevator did not unlock normal gameplay");
        if (!game.GameplayMovementEnabled)
            throw new InvalidOperationException("Ceres gameplay was reached with movement disabled.");

        // A reconstructed menu reading ENERGY/TIME is not a save-load test. Build a wholly
        // new outer dispatcher over the same cartridge/SRAM image, enter slot A through the
        // same title, file-select, and options calls as the desktop host, and require the
        // existing-save branch to reconstruct the cartridge-authored Ceres station. This
        // deliberately does not call SaveSlot or a runtime initializer from the audit.
        var restartedGame = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        int restartedSaveRamChangeCount = 0;
        restartedGame.SaveRamChanged += () => restartedSaveRamChangeCount++;
        FrontendFrame restartedFrame = FrontendAuditDriver.EnterSelectedSlot(restartedGame);
        if (restartedFrame.GameState != SuperMetroidGameState.MainGameplayFadeIn ||
            restartedGame.GameplayActiveRoomPointer != game.GameplayActiveRoomPointer)
        {
            throw new InvalidDataException(
                $"Fresh frontend loaded state {restartedFrame.GameState}/room " +
                $"${restartedGame.GameplayActiveRoomPointer.GetValueOrDefault():X4}, not " +
                $"the saved Ceres room ${game.GameplayActiveRoomPointer.GetValueOrDefault():X4}.");
        }
        if (restartedSaveRamChangeCount != 1)
        {
            throw new InvalidOperationException(
                $"Existing-save startup published {restartedSaveRamChangeCount} SRAM changes; " +
                "only the selected-slot word should be rewritten.");
        }
        restartedFrame = StepUntil(
            restartedGame,
            restartedFrame,
            candidate => candidate.GameState == SuperMetroidGameState.MainGameplay,
            maximumFrames: 180,
            "fresh frontend did not unlock the loaded Ceres checkpoint");
        if (!restartedGame.GameplayMovementEnabled ||
            restartedGame.GameplayCollectedItems != saved.CollectedItems ||
            restartedGame.GameplayEquippedItems != saved.EquippedItems)
        {
            throw new InvalidDataException(
                "Fresh frontend did not restore Ceres control and saved inventory words.");
        }

        // Ceres 6:0 intentionally uses a special elevator loader. Seed one ordinary
        // Crateria 0:0 payload through the real SRAM encoder as a separate proof that a
        // fresh dispatcher follows the ROM's area/load-station/door tables and restores
        // player, boss, map, and clock state before constructing the room.
        var crateriaSystem = new Bank80SystemState();
        crateriaSystem.SetBossBits(0, BossBits.AreaTorizo);
        crateriaSystem.MarkSaveStationUsed(areaIndex: 0, stationBitIndex: 0);
        crateriaSystem.MarkExploredMapTile(areaIndex: 0, mapX: 27, mapY: 5);
        var crateriaSamus = new SamusState
        {
            Health = 87,
            MaxHealth = 199,
            Missiles = 3,
            MaxMissiles = 5,
            EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        };
        var crateriaTime = new GameTimeState();
        crateriaTime.Load(frames: 12, seconds: 56, minutes: 34, hours: 12);
        new SuperMetroidSaveRam(bus).SaveSlot(
            0,
            SuperMetroidSaveSnapshot.Capture(
                crateriaSamus,
                crateriaSystem,
                area: 0,
                saveStation: 0,
                gameTime: crateriaTime));

        var crateriaReload = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        FrontendFrame crateriaFrame = FrontendAuditDriver.EnterSelectedSlot(crateriaReload);
        LoadStationEntry crateriaStation = LoadStationEntry.Load(bus, areaIndex: 0, stationIndex: 0);
        if (crateriaFrame.GameState != SuperMetroidGameState.MainGameplay ||
            crateriaReload.GameplayActiveRoomPointer != crateriaStation.RoomPointer ||
            crateriaReload.GameplayHealth != 87 ||
            crateriaReload.GameplayMaxHealth != 199 ||
            crateriaReload.GameplayEquippedItems != crateriaSamus.EquippedItems ||
            !crateriaReload.GameplayHasBossBits(0, BossBits.AreaTorizo) ||
            !crateriaReload.GameplayIsMapTileExplored(0, 27, 5) ||
            crateriaReload.GameplayTimeHours != 12 ||
            crateriaReload.GameplayTimeMinutes != 34)
        {
            throw new InvalidDataException(
                $"General saved-game startup disagreed: state={crateriaFrame.GameState}, " +
                $"room=${crateriaReload.GameplayActiveRoomPointer.GetValueOrDefault():X4}/" +
                $"${crateriaStation.RoomPointer:X4}, energy={crateriaReload.GameplayHealth}/" +
                $"{crateriaReload.GameplayMaxHealth}, items=${crateriaReload.GameplayEquippedItems:X4}, " +
                $"boss={crateriaReload.GameplayHasBossBits(0, BossBits.AreaTorizo)}, " +
                $"map={crateriaReload.GameplayIsMapTileExplored(0, 27, 5)}, " +
                $"time={crateriaReload.GameplayTimeHours:D2}:{crateriaReload.GameplayTimeMinutes:D2}.");
        }

        for (int pixelIndex = 0; pixelIndex < frame.Pixels.Length; pixelIndex++)
        {
            if (frame.Pixels[pixelIndex].A != byte.MaxValue)
            {
                throw new InvalidDataException(
                    $"Skip-intro gameplay frame contains alpha at pixel {pixelIndex}.");
            }
        }

        PngWriter.WriteRgba(outputPath, FrontendFrame.Width, FrontendFrame.Height, frame.Pixels);
        SuperMetroidRuntime liveRuntime = game.RuntimeForVerification
            ?? throw new InvalidOperationException("Skip-intro frontend lost its Ceres runtime.");
        string doorBlocks = DescribeDoorBlocks(bus, liveRuntime);
        Console.WriteLine(
            $"Skip-opening-cinematic audit reached {frame.GameState} on dispatcher frame " +
            $"{frame.FrameNumber}, Samus=(${game.GameplaySamusX:X4},${game.GameplaySamusY:X4}), " +
            $"room=$8F:{game.GameplayActiveRoomPointer.GetValueOrDefault():X4}, " +
            $"doors=[{doorBlocks}]; slot A loaded through fresh Ceres and general Crateria " +
            "frontend paths.");
        Console.WriteLine($"Captured skip-intro Ceres frame to {Path.GetFullPath(outputPath)}.");
        return 0;
    }

    /// <summary>
    /// Formats the live type-$9 blocks with the bank-$83 records they resolve to. This is
    /// intentionally observational: publishing side effects is disabled, so diagnostics
    /// cannot enter a room or alter the controller route they are meant to explain.
    /// </summary>
    private static string DescribeDoorBlocks(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            "Ceres door diagnostics require a loaded level.");
        var doors = new List<string>();
        for (int y = 0; y < level.HeightInBlocks; y++)
        {
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                if (block.CollisionType != RoomCollisionType.DoorBlock)
                    continue;
                CartridgeDoorHeader door = level.ResolveDoorCollision(
                    bus,
                    block.Behavior,
                    runtime.Samus?.Pose ?? 0,
                    publishDoorSideEffects: false);
                doors.Add($"({x:X2},{y:X2})->$83:{door.Pointer:X4}/$8F:{door.DestinationRoomPointer:X4}");
            }
        }
        return string.Join(' ', doors);
    }

    private static FrontendFrame StepUntil(
        SuperMetroidGame game,
        FrontendFrame initialFrame,
        Func<FrontendFrame, bool> finished,
        int maximumFrames,
        string failure)
    {
        FrontendFrame frame = initialFrame;
        for (int frameIndex = 0; frameIndex < maximumFrames && !finished(frame); frameIndex++)
            frame = game.Step(0);
        if (!finished(frame))
            throw new InvalidOperationException($"{failure} within {maximumFrames} frames.");
        return frame;
    }
}
