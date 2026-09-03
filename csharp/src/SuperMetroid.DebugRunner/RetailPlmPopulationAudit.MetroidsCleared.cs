using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-backed coverage for the resident Metroid-room clear observers. This is kept
/// beside the broad population inventory because its primary contract is not merely header
/// recognition: loading <c>$DB44</c> must preserve every earlier <c>$B703</c> scroll owner.
/// </summary>
internal static partial class RetailPlmPopulationAudit
{
    private const ushort ParlorRoomPointer = 0x92fd;
    private const ushort ParlorMetroidsGoneStatePointer = 0x9348;
    private const int ExpectedMetroidsClearedStateCount = 17;
    private const int ExpectedMetroidsClearedPopulationCount = 13;

    private static readonly int[] ReportedParlorTriggerBlocks = [940, 1062, 1172];

    /// <summary>
    /// Loads all seventeen retail states containing <c>$84:DB44</c> through the production
    /// one-pass loader, runs the exact argument-selected quota observer, and sends Samus's
    /// real vertical collision scan through every formerly crashing Parlor trigger.
    /// </summary>
    public static int AuditMetroidsClearedStates(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        ScrollAuditRoomState[] states = LoadScrollAuditStates()
            .Where(state => ReadScrollAuditPopulation(
                    bus,
                    CartridgeRoomState.Load(bus, state.StatePointer).PlmPointer)
                .Any(record =>
                    record.Header == RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired))
            .ToArray();

        if (states.Length != ExpectedMetroidsClearedStateCount)
        {
            throw new InvalidDataException(
                $"Retail $DB44 inventory found {states.Length} states, expected " +
                $"{ExpectedMetroidsClearedStateCount}.");
        }

        int distinctPopulations = states
            .Select(state => CartridgeRoomState.Load(bus, state.StatePointer).PlmPointer)
            .Distinct()
            .Count();
        if (distinctPopulations != ExpectedMetroidsClearedPopulationCount)
        {
            throw new InvalidDataException(
                $"Retail $DB44 inventory found {distinctPopulations} populations, expected " +
                $"{ExpectedMetroidsClearedPopulationCount}.");
        }

        int loadedObservers = 0;
        int testedEventBranches = 0;
        int testedParlorTriggers = 0;
        foreach (ScrollAuditRoomState state in states)
        {
            CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, state.RoomPointer);
            CartridgeRoomState exactState = CartridgeRoomState.Load(bus, state.StatePointer);
            CartridgeRoomHeader room = defaultRoom with { State = exactState };
            CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
            ScrollAuditPopulationRecord[] sourceRecords =
                ReadScrollAuditPopulation(bus, exactState.PlmPointer);
            ScrollAuditPopulationRecord[] sourceObservers = sourceRecords
                .Where(record =>
                    record.Header == RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired)
                .ToArray();

            ushort[] unsupportedHeaders = sourceRecords
                .Where(record => !RoomPlmSystem.IsSupportedRoomPopulationHeader(record.Header))
                .Select(record => record.Header)
                .Distinct()
                .Order()
                .ToArray();
            if (unsupportedHeaders.Length != 0)
            {
                throw new InvalidDataException(
                    $"Retail $DB44 room/state $8F:{state.RoomPointer:X4}/" +
                    $"{state.StatePointer:X4} is still blocked by PLM header(s) " +
                    $"{string.Join(", ", unsupportedHeaders.Select(header => $"$84:{header:X4}"))}.");
            }

            var plms = new RoomPlmSystem();
            var system = new Bank80SystemState();
            var samus = new SamusState();
            BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();
            plms.LoadRoomPopulation(
                bus,
                assets.LevelData,
                streamer,
                new SnesVram(),
                exactState.PlmPointer,
                system,
                room.AreaIndex,
                getSamus: () => samus,
                isAreaTorizoDefeated: () => false,
                isTourianStatueFinished: () => false,
                hasAreaBossBit: _ => false,
                hasEvent: system.HasEvent,
                setEvent: system.SetEvent);

            RoomPlmSlotSnapshot[] loaded = plms.PopulationSlots
                .Where(slot =>
                    slot.HeaderPointer ==
                        RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired)
                .ToArray();
            if (loaded.Length != sourceObservers.Length)
            {
                throw new InvalidDataException(
                    $"Retail $DB44 room/state $8F:{state.RoomPointer:X4}/" +
                    $"{state.StatePointer:X4} loaded {loaded.Length} observers, expected " +
                    $"{sourceObservers.Length}.");
            }

            foreach (RoomPlmSlotSnapshot observer in loaded)
            {
                ScrollAuditPopulationRecord source = sourceObservers.Single(record =>
                    assets.LevelData.GetBlockIndex(record.BlockX, record.BlockY) ==
                        observer.BlockIndex);
                if (observer.RoomArgument != source.RoomArgument ||
                    observer.InstructionPointer !=
                        RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired)
                {
                    throw new InvalidDataException(
                        $"Retail $DB44 observer in state $8F:{state.StatePointer:X4} lost its " +
                        "room argument or permanent sleep-list pointer.");
                }

                loadedObservers++;
            }

            plms.Step(
                bus,
                assets.LevelData,
                streamer,
                layer1XPosition: 0,
                layer1YPosition: 0,
                bg1XOffset: 0,
                assets.Scrolls,
                enemyDeaths: 2,
                enemyDeathQuota: 3);
            AssertMetroidsClearedEventState(system, sourceObservers, shouldBeSet: false, state);

            plms.Step(
                bus,
                assets.LevelData,
                streamer,
                layer1XPosition: 0,
                layer1YPosition: 0,
                bg1XOffset: 0,
                assets.Scrolls,
                enemyDeaths: 3,
                enemyDeathQuota: 3);
            testedEventBranches += AssertMetroidsClearedEventState(
                system,
                sourceObservers,
                shouldBeSet: true,
                state);

            if (state.RoomPointer == ParlorRoomPointer &&
                state.StatePointer == ParlorMetroidsGoneStatePointer)
            {
                testedParlorTriggers = AuditReportedParlorTriggerCollisions(
                    bus,
                    assets.LevelData,
                    plms);
            }
        }

        if (testedEventBranches != 4)
        {
            throw new InvalidDataException(
                $"Retail $DB44 audit reached {testedEventBranches} event-setting argument " +
                "branches, expected all four.");
        }
        if (testedParlorTriggers != ReportedParlorTriggerBlocks.Length)
        {
            throw new InvalidDataException(
                "Retail $DB44 audit did not reach the exact Parlor state containing all " +
                "three reported trigger blocks.");
        }

        Console.WriteLine(
            $"Metroids-cleared PLM audit passed: {states.Length} retail states / " +
            $"{distinctPopulations} populations production-loaded; {loadedObservers} " +
            $"observers retained; four event branches and {testedParlorTriggers} exact " +
            "Parlor collision triggers passed.");
        return 0;
    }

    private static int AssertMetroidsClearedEventState(
        Bank80SystemState system,
        IReadOnlyList<ScrollAuditPopulationRecord> observers,
        bool shouldBeSet,
        ScrollAuditRoomState state)
    {
        int activeBranches = 0;
        foreach (ScrollAuditPopulationRecord observer in observers)
        {
            EventNumber? eventNumber = observer.RoomArgument switch
            {
                0x12 => EventNumber.FirstMetroidHallCleared,
                0x14 => EventNumber.FirstMetroidShaftCleared,
                0x16 => EventNumber.SecondMetroidHallCleared,
                0x18 => EventNumber.SecondMetroidShaftCleared,
                _ => null,
            };
            if (eventNumber is null)
                continue;

            activeBranches++;
            if (system.HasEvent(eventNumber.Value) != shouldBeSet)
            {
                throw new InvalidDataException(
                    $"Retail $DB44 state $8F:{state.StatePointer:X4}, argument " +
                    $"${observer.RoomArgument:X4} {(shouldBeSet ? "did not set" : "set early")} " +
                    $"event ${((byte)eventNumber.Value):X2}.");
            }
        }

        return activeBranches;
    }

    private static int AuditReportedParlorTriggerCollisions(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomPlmSystem plms)
    {
        int tested = 0;
        foreach (int blockIndex in ReportedParlorTriggerBlocks)
        {
            if (!plms.ScrollPlms.Any(scroll => scroll.BlockIndex == blockIndex))
            {
                throw new InvalidDataException(
                    $"Parlor trigger block {blockIndex} has no loaded $B703 owner.");
            }

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            if (block.CollisionType != RoomCollisionType.SpecialAir ||
                block.Bts != RoomBlockBehaviorValues.ScrollTrigger)
            {
                throw new InvalidDataException(
                    $"Parlor block {blockIndex} is not the authored type-$3/BTS-$46 trigger.");
            }

            int blockX = blockIndex % level.WidthInBlocks;
            int blockY = blockIndex / level.WidthInBlocks;
            var collision = new SamusKinematicsState
            {
                XPosition = checked((ushort)(blockX * 16 + 8)),
                YPosition = checked((ushort)(blockY * 16 - 8)),
                XRadius = 5,
                YRadius = 8,
            };
            BlockMoveResult result = SamusBlockCollision.MoveVertical(
                bus,
                level,
                collision,
                displacement: 1 << 16,
                scanLeftToRight: true,
                includeSolidEnemies: false,
                plms: plms,
                publishDoorSideEffects: false);
            if (result.Collided ||
                !plms.ScrollPlms.Single(scroll => scroll.BlockIndex == blockIndex).Triggered)
            {
                throw new InvalidDataException(
                    $"Parlor trigger block {blockIndex} did not complete its production " +
                    "vertical collision wake-up.");
            }

            tested++;
        }

        return tested;
    }
}
