using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;

/// <summary>
/// Drives ordinary collision into the retail Crateria map station, acknowledges its
/// message without Start, and requires the automatic map handoff and return to gameplay.
/// </summary>
internal static class MapStationPauseGateAudit
{
    private const ushort CrateriaMapEntranceDoor = 0x8bda;

    public static int RunLockedControls(string romPath)
    {
        // Exercise both continuous and fresh Shoot input for each humanoid weapon
        // producer; every case enters the real station through normal collision.
        foreach (ushort selectedWeapon in new ushort[] { 0, 1, 2 })
        foreach (bool holdShoot in new[] { false, true })
            Run(romPath, auditLockedControls: true, selectedWeapon, holdShoot);
        return 0;
    }

    public static int Run(string romPath, bool auditLockedControls = false,
        ushort selectedWeapon = 0, bool holdShoot = false)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        SeedCrateriaSave(bus);
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        FrontendFrame frame = FrontendAuditDriver.EnterSelectedSlot(game);
        frame = FrontendAuditDriver.StepUntil(
            game,
            frame,
            _ => game.GameplayMovementEnabled &&
                 game.RuntimeForVerification?.Samus is { InputLocked: false },
            maximumFrames: 420,
            "saved-game appearance did not restore ordinary Samus input");

        SuperMetroidRuntime runtime = game.RuntimeForVerification
            ?? throw new InvalidOperationException("Map-station audit lost its gameplay runtime.");
        runtime.LoadCartridgeRoomThroughDoorForVerification(
            CartridgeDoorHeader.Load(bus, CrateriaMapEntranceDoor));
        StationPlmSnapshot station = runtime.Plms.Stations.Single(
            candidate => candidate.Kind == StationKind.Map);
        // Start beside the real access block; activation must come from ordinary
        // movement/collision, not a direct notification that bypasses station setup.
        SamusState samus = runtime.Samus!;
        if (auditLockedControls)
        {
            samus.SelectedHudItem = selectedWeapon;
            samus.Missiles = samus.MaxMissiles = 10;
            samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        }
        samus.XPosition = checked((ushort)((station.BlockIndex % runtime.LevelData!.WidthInBlocks + 1) * 16 + 40));
        samus.YPosition = checked((ushort)(station.BlockIndex / runtime.LevelData.WidthInBlocks * 16 + 11));

        bool sawInsertionOrExtendedHold = false;
        bool sawMapDataMessage = false;
        bool mapMessageFinished = false;
        int ownedFrames = 0;
        while (ownedFrames < 500)
        {
            bool messageAtEntry = runtime.MessageBox.IsActive;
            bool lockedAtEntry = samus.InputLocked;
            byte poseAtEntry = samus.Pose;
            ushort input = messageAtEntry
                ? (ownedFrames & 1) == 0 ? (ushort)SnesButton.A : (ushort)0
                : (ushort)SnesButton.Left;
            if (auditLockedControls && lockedAtEntry && !messageAtEntry)
                input = (ushort)(SnesButton.Right |
                    (holdShoot || (ownedFrames & 1) == 0 ? SnesButton.X : 0));
            if (auditLockedControls && lockedAtEntry && messageAtEntry)
                input |= (ushort)SnesButton.Right;
            frame = game.Step(input);
            ownedFrames++;

            if (auditLockedControls && lockedAtEntry && !messageAtEntry)
            {
                if (runtime.Projectiles.LastFiredProjectileSnapshot is not null)
                    throw new InvalidDataException($"Station lock admitted a new shot at frame {ownedFrames}.");
            }
            if (auditLockedControls && lockedAtEntry && samus.Pose != poseAtEntry)
                throw new InvalidDataException($"Station lock changed pose {poseAtEntry:X2} to {samus.Pose:X2} at frame {ownedFrames}.");

            if (messageAtEntry && !runtime.MessageBox.IsActive)
            {
                if (game.GameState != SuperMetroidGameState.PausingDarkening)
                    throw new InvalidDataException($"Map message closed at frame {ownedFrames}, but selected {game.GameState} instead of automatic map pause.");
                mapMessageFinished = true;
                break;
            }

            if (game.GameState != SuperMetroidGameState.MainGameplay)
            {
                throw new InvalidDataException(
                    $"Premature transition to {game.GameState} on map-station-owned frame " +
                    $"{ownedFrames}; message={runtime.MessageBox.Phase}, " +
                    $"inputLocked={runtime.Samus?.InputLocked}.");
            }

            sawInsertionOrExtendedHold |= runtime.Samus is { InputLocked: true } &&
                !runtime.MessageBox.IsActive && !mapMessageFinished;
            sawMapDataMessage |= runtime.MessageBox.IsActive;
        }

        if (!sawInsertionOrExtendedHold || !sawMapDataMessage ||
            !mapMessageFinished ||
            !runtime.System.HasAreaMap(AreaId.Crateria))
        {
            throw new InvalidDataException(
                "Map-station audit did not observe insertion, data message, acquisition, " +
                $"and automatic pause within {ownedFrames} frames.");
        }

        frame = FrontendAuditDriver.StepUntil(game, frame,
            _ => game.GameState == SuperMetroidGameState.PausedB, 120,
            "Automatic station map did not become visible");
        if (game.PauseScreenMode != 0 || frame.Pixels.Distinct().Count() < 8)
            throw new InvalidDataException("Automatic station pause did not render the map screen.");
        const string captureDirectory = "csharp/test-temp/map-station-561";
        Directory.CreateDirectory(captureDirectory);
        PngWriter.WriteRgba(Path.Combine(captureDirectory, "automatic-map.png"),
            FrontendFrame.Width, FrontendFrame.Height, frame.Pixels);
        game.Step(0);
        // Pause chrome uses the cartridge's delayed-held input, not a one-NMI tap.
        for (int i = 0; i < 8; i++) frame = game.Step((ushort)SnesButton.Start);
        frame = FrontendAuditDriver.StepUntil(game, frame,
            _ => game.GameState == SuperMetroidGameState.MainGameplay, 120,
            "Station map dismissal did not resume gameplay");
        for (int i = 0; i < 180; i++)
        {
            bool locked = samus.InputLocked;
            byte pose = samus.Pose;
            game.Step((ushort)(auditLockedControls && locked
                ? SnesButton.Right | ((i & 1) == 0 ? SnesButton.X : 0)
                : SnesButton.Left));
            if (auditLockedControls && locked &&
                (samus.Pose != pose || runtime.Projectiles.LastFiredProjectileSnapshot is not null))
                throw new InvalidDataException("Station retraction admitted a turn or shot after map dismissal.");
        }
        if (game.GameState != SuperMetroidGameState.MainGameplay || runtime.MessageBox.IsActive || samus.InputLocked)
            throw new InvalidDataException("Acquired station repeated its prompt or failed to release Samus.");

        if (auditLockedControls)
        {
            if (samus.Missiles != 10 || samus.SuperMissiles != 10)
                throw new InvalidDataException("Station-owned input consumed ammunition.");
            game.Step(0);
            game.Step((ushort)SnesButton.X);
            if (runtime.Projectiles.LastFiredProjectileSnapshot is null)
                throw new InvalidDataException("Station release did not restore shooting.");
        }

        Console.WriteLine(
            $"Map-station pause gate passed {ownedFrames} station-owned frames across " +
            "insertion, map-data message, automatic map display, dismissal, and acquired-station control.");
        return 0;
    }

    private static void SeedCrateriaSave(SuperMetroidAddressSpace bus)
    {
        var system = new Bank80SystemState();
        system.SetBossBits(AreaId.Crateria, BossBits.AreaTorizo);
        system.MarkSaveStationUsed(AreaId.Crateria, stationBitIndex: 0);
        var samus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
            EquippedItems = SamusEquipmentFlags.MorphBall.ToNativeWord(),
            CollectedItems = SamusEquipmentFlags.MorphBall.ToNativeWord(),
        };
        new SuperMetroidSaveRam(bus).SaveSlot(
            0,
            SuperMetroidSaveSnapshot.Capture(
                samus,
                system,
                area: (ushort)AreaId.Crateria,
                saveStation: 0));
    }
}
