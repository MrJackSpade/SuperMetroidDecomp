using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Exact retail lifecycle proof for Speed Booster escape PLM $84:B8AC.</summary>
internal static partial class RetailPlmPopulationAudit
{
    private const ushort SpeedBoosterEscapeRoom = 0xacf0;
    private const ushort SpeedBoosterEscapeState = 0xacfd;
    private const ushort SpeedBoosterEscapePopulation = 0x8c6e;
    private const ushort SpeedBoosterEscapeEntryDoor = 0x95be;

    /// <summary>
    /// Production-loads room state $ACFD, then drives setup and all three callbacks using
    /// the pinned cartridge's own header, list, FX record, and stage table.
    /// </summary>
    public static int AuditSpeedBoosterEscape(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        if (ReadWord(bus, 0x84b8ae) != RoomPlmInstructionLists.SpeedBoosterEscape ||
            ReadWord(bus, 0x84b88a) != RoomPlmInstructionCodes.InstallPreInstruction ||
            ReadWord(bus, 0x84b88c) !=
                SpeedBoosterEscapePlmRomData.WaitForSpeedBoosterPreInstruction ||
            ReadWord(bus, 0x84b890) != RoomPlmInstructionCodes.InstallPreInstruction ||
            ReadWord(bus, 0x84b892) !=
                SpeedBoosterEscapePlmRomData.WaitForSamusLeftPreInstruction ||
            ReadWord(bus, 0x84b896) != RoomPlmInstructionCodes.InstallPreInstruction ||
            ReadWord(bus, 0x84b898) !=
                SpeedBoosterEscapePlmRomData.AdvanceLavaPreInstruction)
        {
            throw new InvalidDataException(
                "Retail B8AC header/list no longer matches the translated three-stage coroutine.");
        }

        LoadedSpeedBoosterEscape active = LoadSpeedBoosterEscape(
            bus,
            collectedSpeedBooster: true,
            eventAlreadySet: false);
        if (active.Room.State.PlmPointer != SpeedBoosterEscapePopulation)
        {
            throw new InvalidDataException(
                $"Retail state $8F:{SpeedBoosterEscapeState:X4} selected PLM population " +
                $"$8F:{active.Room.State.PlmPointer:X4}, expected " +
                $"$8F:{SpeedBoosterEscapePopulation:X4}.");
        }
        RoomPlmSlotSnapshot speedBoosterSlot = active.Plms.PopulationSlots.Single(
            slot => slot.HeaderPointer == RoomPlmHeaders.SpeedBoosterEscape);
        if (speedBoosterSlot.InstructionPointer != RoomPlmInstructionLists.SpeedBoosterEscape)
        {
            throw new InvalidDataException(
                $"Retail B8AC loaded list $84:{speedBoosterSlot.InstructionPointer:X4}, " +
                $"expected $84:{RoomPlmInstructionLists.SpeedBoosterEscape:X4}.");
        }

        Step(active);
        Step(active);
        if (active.Fx.PackedYVelocity !=
            SpeedBoosterEscapePlmRomData.InitialLavaquakeVelocity)
        {
            throw new InvalidDataException(
                $"B7EF wrote FX velocity ${active.Fx.PackedYVelocity:X4}, expected " +
                $"${SpeedBoosterEscapePlmRomData.InitialLavaquakeVelocity:X4}.");
        }

        active.Samus.XPosition = SpeedBoosterEscapePlmRomData.StartFxMotionSamusX;
        Step(active);
        if (active.Fx.Timer != 1)
            throw new InvalidDataException($"B82A wrote FX timer {active.Fx.Timer}, expected 1.");

        for (ushort offset = 0;
             offset < SpeedBoosterEscapePlmRomData.TerminatorOffset;
             offset += SpeedBoosterEscapePlmRomData.StageByteCount)
        {
            ushort row = unchecked((ushort)(SpeedBoosterEscapePlmRomData.StageTable + offset));
            ushort targetX = ReadWord(bus, 0x840000 | row);
            ushort maximumY = ReadWord(bus, 0x840000 | unchecked((ushort)(row + 2)));
            ushort velocity = ReadWord(bus, 0x840000 | unchecked((ushort)(row + 4)));
            active.Samus.XPosition = targetX;
            ushort priorBaseY = active.Fx.BaseYPosition;
            Step(active);
            ushort expectedBaseY = maximumY < priorBaseY ? maximumY : priorBaseY;
            if (active.Fx.BaseYPosition != expectedBaseY ||
                active.Fx.PackedYVelocity != velocity)
            {
                throw new InvalidDataException(
                    $"B846 stage ${offset:X2} produced base/velocity " +
                    $"${active.Fx.BaseYPosition:X4}/${active.Fx.PackedYVelocity:X4}, " +
                    $"expected ${expectedBaseY:X4}/${velocity:X4} from cartridge table.");
            }
        }
        Step(active);
        if (!active.System.HasEvent(EventNumber.OutranSpeedBoosterLavaquake))
            throw new InvalidDataException("B846 terminator did not mark retail event $15.");

        LoadedSpeedBoosterEscape unequipped = LoadSpeedBoosterEscape(
            bus,
            collectedSpeedBooster: false,
            eventAlreadySet: false);
        unequipped.EarthquakeTimer = 7;
        Step(unequipped);
        Step(unequipped);
        if (unequipped.Plms.PopulationSlots.Any(
                slot => slot.HeaderPointer == RoomPlmHeaders.SpeedBoosterEscape) ||
            unequipped.Fx.TargetYPosition != ushort.MaxValue ||
            unequipped.Fx.PackedYVelocity != 0 ||
            unequipped.Fx.Timer != 0 ||
            unequipped.EarthquakeTimer != 0)
        {
            throw new InvalidDataException(
                "B7EF no-item branch did not disable FX/earthquake and delete the retail actor.");
        }

        LoadedSpeedBoosterEscape completed = LoadSpeedBoosterEscape(
            bus,
            collectedSpeedBooster: true,
            eventAlreadySet: true);
        if (completed.Plms.PopulationSlots.Any(
                slot => slot.HeaderPointer == RoomPlmHeaders.SpeedBoosterEscape))
        {
            throw new InvalidDataException(
                "B89C retained the retail B8AC actor after event $15 was already set.");
        }

        Console.WriteLine(
            "Speed Booster escape PLM audit passed: retail $ACF0/$ACFD production-loaded; " +
            "setup, item/no-item, FX threshold, all three ROM stages, earthquake clear, " +
            "and event-$15 branches agree.");
        return 0;
    }

    private static LoadedSpeedBoosterEscape LoadSpeedBoosterEscape(
        SuperMetroidAddressSpace bus,
        bool collectedSpeedBooster,
        bool eventAlreadySet)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, SpeedBoosterEscapeRoom) with
        {
            State = CartridgeRoomState.Load(bus, SpeedBoosterEscapeState),
        };
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var system = new Bank80SystemState();
        if (eventAlreadySet)
            system.SetEvent(EventNumber.OutranSpeedBoosterLavaquake);
        var samus = new SamusState
        {
            CollectedItems = collectedSpeedBooster
                ? (ushort)SamusEquipmentFlags.SpeedBooster
                : (ushort)0,
            XPosition = 0x0ae1,
        };
        var fx = new RoomLayer3FxState();
        fx.Load(
            bus,
            new SnesVram(),
            new SnesCgram(),
            room.State.FxPointer,
            doorPointer: SpeedBoosterEscapeEntryDoor,
            randomNumber: 0);
        var result = new LoadedSpeedBoosterEscape(bus, room, assets, system, samus, fx);
        result.Plms.LoadRoomPopulation(
            bus,
            assets.LevelData,
            result.Streamer,
            new SnesVram(),
            room.State.PlmPointer,
            system,
            room.AreaIndex,
            () => samus,
            () => false,
            isTourianStatueFinished: () => false,
            hasAreaBossBit: _ => false,
            hasEvent: system.HasEvent,
            setEvent: system.SetEvent,
            roomFx: fx,
            setEarthquakeTimer: value => result.EarthquakeTimer = value);
        return result;
    }

    private static void Step(LoadedSpeedBoosterEscape loaded) => loaded.Plms.Step(
        loaded.Bus,
        loaded.Assets.LevelData,
        loaded.Streamer,
        0,
        0,
        0,
        loaded.Assets.Scrolls);

    private sealed class LoadedSpeedBoosterEscape(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        Bank80SystemState system,
        SamusState samus,
        RoomLayer3FxState fx)
    {
        public SuperMetroidAddressSpace Bus { get; } = bus;
        public CartridgeRoomHeader Room { get; } = room;
        public CartridgeRoomAssets Assets { get; } = assets;
        public Bank80SystemState System { get; } = system;
        public SamusState Samus { get; } = samus;
        public RoomLayer3FxState Fx { get; } = fx;
        public RoomPlmSystem Plms { get; } = new();
        public BackgroundTilemapStreamer Streamer { get; } = assets.LevelData.CreateBackgroundStreamer();
        public ushort EarthquakeTimer { get; set; }
    }
}
