using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Drives the retail Crateria map station through its complete bank-$84 operation while
/// pulsing Start on every possible rising edge. This exercises the outer state-eight pause
/// admission that the isolated PLM tests cannot observe.
/// </summary>
internal static class MapStationPauseGateAudit
{
    private const ushort CrateriaMapEntranceDoor = 0x8bda;

    public static int Run(string romPath)
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
        int accessBlock = checked(station.BlockIndex + 1);
        if (!runtime.Plms.TryNotifyStationTouch(
                accessBlock,
                (byte)StationAccessBehavior.MapRight))
        {
            throw new InvalidDataException(
                "Retail Crateria map-station access did not resolve its resident parent.");
        }

        bool sawInsertionOrExtendedHold = false;
        bool sawMapDataMessage = false;
        bool sawRetractionAfterMessage = false;
        bool mapMessageFinished = false;
        int ownedFrames = 0;
        while (ownedFrames < 500)
        {
            bool messageAtEntry = runtime.MessageBox.IsActive;
            ushort input = (ownedFrames & 1) == 0 ? (ushort)SnesButton.Start : (ushort)0;
            frame = game.Step(input);
            ownedFrames++;

            if (game.GameState != SuperMetroidGameState.MainGameplay)
            {
                throw new InvalidDataException(
                    $"Start entered {game.GameState} on map-station-owned frame " +
                    $"{ownedFrames}; message={runtime.MessageBox.Phase}, " +
                    $"inputLocked={runtime.Samus?.InputLocked}.");
            }

            sawInsertionOrExtendedHold |= runtime.Samus is { InputLocked: true } &&
                !runtime.MessageBox.IsActive && !mapMessageFinished;
            sawMapDataMessage |= runtime.MessageBox.IsActive;
            mapMessageFinished |= messageAtEntry && !runtime.MessageBox.IsActive;
            sawRetractionAfterMessage |= mapMessageFinished &&
                runtime.Samus is { InputLocked: true };

            if (mapMessageFinished && runtime.Samus is { InputLocked: false })
                break;
        }

        if (runtime.Samus is not { InputLocked: false } ||
            !sawInsertionOrExtendedHold || !sawMapDataMessage ||
            !mapMessageFinished || !sawRetractionAfterMessage ||
            !runtime.System.HasAreaMap(AreaId.Crateria))
        {
            throw new InvalidDataException(
                "Map-station audit did not observe insertion, data message, retraction, " +
                $"and release within {ownedFrames} frames.");
        }

        // A release frame clears the station-owned handler after gameplay has already
        // consumed that frame. Require a new Start edge on the following ordinary frame.
        game.Step(0);
        game.Step((ushort)SnesButton.Start);
        if (game.GameState != SuperMetroidGameState.PausingDarkening)
        {
            throw new InvalidDataException(
                $"First clean Start after map-station release selected {game.GameState}.");
        }

        Console.WriteLine(
            $"Map-station pause gate passed {ownedFrames} station-owned frames across " +
            "insertion, map-data message, retraction, release, and next-frame pause.");
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
