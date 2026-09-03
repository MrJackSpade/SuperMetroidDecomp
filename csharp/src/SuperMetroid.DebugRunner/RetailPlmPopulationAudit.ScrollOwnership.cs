using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Production-loader audit for the native contract between type-$3/BTS-$46 collision
/// blocks and their resident bank-$84 scroll PLMs.
/// </summary>
internal static partial class RetailPlmPopulationAudit
{
    /// <summary>
    /// Loads every symbolized retail room state, compares cartridge collision data to its
    /// authored scroll population, then verifies the translated loader creates exactly one
    /// live owner for every resulting trigger. Unsupported population records are reported
    /// as explicit audit blockers rather than allowing the states behind them to disappear.
    /// </summary>
    public static int AuditScrollOwnership(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        ScrollAuditRoomState[] states = LoadScrollAuditStates();
        var failures = new List<ScrollOwnershipFailure>();
        var blockers = new List<ScrollAuditLoadBlocker>();
        var loaderFailures = new List<ScrollAuditLoaderFailure>();
        var lifecycleFailures = new List<ScrollAuditLoaderFailure>();
        int loadedStates = 0;
        int sourceOwnerCount = 0;
        int verifiedOwnerCount = 0;

        foreach (ScrollAuditRoomState state in states)
        {
            CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, state.RoomPointer);
            CartridgeRoomState exactState = CartridgeRoomState.Load(bus, state.StatePointer);
            CartridgeRoomHeader room = defaultRoom with { State = exactState };
            CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
            ScrollAuditPopulationRecord[] records = ReadScrollAuditPopulation(bus, exactState.PlmPointer);
            int[] sourceOwners = records
                .Where(record => record.Header == RoomPlmHeaders.ScrollTrigger)
                .Select(record => assets.LevelData.GetBlockIndex(record.BlockX, record.BlockY))
                .ToArray();
            sourceOwnerCount += sourceOwners.Length;

            ReportDuplicateSourceOwners(state, sourceOwners, failures);
            ReportRawTriggerMismatches(state, assets.LevelData, sourceOwners, failures);

            ushort[] unsupportedHeaders = records
                .Where(record => !RoomPlmSystem.IsSupportedRoomPopulationHeader(record.Header))
                .Select(record => record.Header)
                .Distinct()
                .Order()
                .ToArray();
            if (unsupportedHeaders.Length != 0)
            {
                blockers.Add(new ScrollAuditLoadBlocker(
                    state,
                    exactState.PlmPointer,
                    unsupportedHeaders));
                continue;
            }

            var plms = new RoomPlmSystem();
            var system = new Bank80SystemState();
            var samus = new SamusState();
            BackgroundTilemapStreamer streamer = assets.LevelData.CreateBackgroundStreamer();
            var vram = new SnesVram();
            try
            {
                plms.LoadRoomPopulation(
                    bus,
                    assets.LevelData,
                    streamer,
                    vram,
                    exactState.PlmPointer,
                    system,
                    room.AreaIndex,
                    getSamus: () => samus,
                    isAreaTorizoDefeated: () => false,
                    isTourianStatueFinished: () => false,
                    hasAreaBossBit: _ => false,
                    hasEvent: _ => false,
                    setEvent: _ => { });
            }
            catch (Exception exception)
            {
                loaderFailures.Add(new ScrollAuditLoaderFailure(
                    state,
                    exactState.PlmPointer,
                    exception));
                continue;
            }
            loadedStates++;

            ScrollPlmSnapshot[] owners = plms.ScrollPlms.ToArray();
            ReportLoadedOwnerMismatches(state, assets.LevelData, sourceOwners, owners, failures);
            verifiedOwnerCount += owners.Length;

            try
            {
                foreach (ScrollPlmSnapshot owner in owners)
                {
                    if (!plms.TryNotifyScrollTouch(owner.BlockIndex))
                    {
                        failures.Add(new ScrollOwnershipFailure(
                            state,
                            owner.BlockIndex,
                            "loaded owner rejected the native scroll-touch callback"));
                    }
                }
                plms.Step(
                    bus,
                    assets.LevelData,
                    streamer,
                    layer1XPosition: 0,
                    layer1YPosition: 0,
                    bg1XOffset: 0,
                    assets.Scrolls);
                ReportLoadedOwnerMismatches(
                    state,
                    assets.LevelData,
                    sourceOwners,
                    plms.ScrollPlms.ToArray(),
                    failures);
                foreach (ScrollPlmSnapshot owner in plms.ScrollPlms.Where(owner => owner.Triggered))
                {
                    failures.Add(new ScrollOwnershipFailure(
                        state,
                        owner.BlockIndex,
                        "scroll owner remained awake after its cartridge data completed"));
                }
            }
            catch (Exception exception)
            {
                lifecycleFailures.Add(new ScrollAuditLoaderFailure(
                    state,
                    exactState.PlmPointer,
                    exception));
            }
        }

        PrintScrollAuditResults(
            states.Length,
            loadedStates,
            sourceOwnerCount,
            verifiedOwnerCount,
            failures,
            blockers,
            loaderFailures,
            lifecycleFailures);
        return failures.Count == 0 && blockers.Count == 0 && loaderFailures.Count == 0 &&
            lifecycleFailures.Count == 0 ? 0 : 1;
    }

    private static void ReportDuplicateSourceOwners(
        ScrollAuditRoomState state,
        IReadOnlyList<int> sourceOwners,
        ICollection<ScrollOwnershipFailure> failures)
    {
        foreach (IGrouping<int, int> duplicate in sourceOwners.GroupBy(index => index).Where(group => group.Count() > 1))
        {
            failures.Add(new ScrollOwnershipFailure(
                state,
                duplicate.Key,
                $"cartridge population contains {duplicate.Count()} resident $B703 owners"));
        }
    }

    private static void ReportRawTriggerMismatches(
        ScrollAuditRoomState state,
        RoomLevelData level,
        IReadOnlyCollection<int> sourceOwners,
        ICollection<ScrollOwnershipFailure> failures)
    {
        HashSet<int> ownerSet = sourceOwners.ToHashSet();
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(index);
            bool isScrollTrigger =
                block.CollisionType == RoomCollisionType.SpecialAir &&
                block.Bts == RoomBlockBehaviorValues.ScrollTrigger;
            if (isScrollTrigger && !ownerSet.Contains(index))
            {
                failures.Add(new ScrollOwnershipFailure(
                    state,
                    index,
                    "raw level contains type-$3/BTS-$46 but the population has no $B703 owner"));
            }
        }
    }

    private static void ReportLoadedOwnerMismatches(
        ScrollAuditRoomState state,
        RoomLevelData level,
        IReadOnlyCollection<int> sourceOwners,
        IReadOnlyCollection<ScrollPlmSnapshot> loadedOwners,
        ICollection<ScrollOwnershipFailure> failures)
    {
        HashSet<int> expected = sourceOwners.ToHashSet();
        int[] actual = loadedOwners.Select(owner => owner.BlockIndex).ToArray();
        HashSet<int> actualSet = actual.ToHashSet();

        foreach (int blockIndex in expected.Except(actualSet))
        {
            failures.Add(new ScrollOwnershipFailure(
                state,
                blockIndex,
                "production loader did not retain the cartridge $B703 owner"));
        }
        foreach (IGrouping<int, int> duplicate in actual.GroupBy(index => index).Where(group => group.Count() > 1))
        {
            failures.Add(new ScrollOwnershipFailure(
                state,
                duplicate.Key,
                $"production loader retained {duplicate.Count()} owners for one trigger"));
        }

        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(index);
            bool isScrollTrigger =
                block.CollisionType == RoomCollisionType.SpecialAir &&
                block.Bts == RoomBlockBehaviorValues.ScrollTrigger;
            if (isScrollTrigger == actualSet.Contains(index))
                continue;

            failures.Add(new ScrollOwnershipFailure(
                state,
                index,
                isScrollTrigger
                    ? "production level has a scroll-trigger block without a live owner"
                    : "production loader retained a scroll owner whose collision block is not type-$3/BTS-$46"));
        }
    }

    private static void PrintScrollAuditResults(
        int stateCount,
        int loadedStateCount,
        int sourceOwnerCount,
        int verifiedOwnerCount,
        IReadOnlyCollection<ScrollOwnershipFailure> failures,
        IReadOnlyCollection<ScrollAuditLoadBlocker> blockers,
        IReadOnlyCollection<ScrollAuditLoaderFailure> loaderFailures,
        IReadOnlyCollection<ScrollAuditLoaderFailure> lifecycleFailures)
    {
        foreach (IGrouping<string, ScrollOwnershipFailure> group in failures
            .GroupBy(failure => failure.Reason)
            .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            Console.Error.WriteLine($"{group.Key}: {group.Count()} occurrence(s)");
            foreach (ScrollOwnershipFailure failure in group)
            {
                Console.Error.WriteLine(
                    $"  room/state $8F:{failure.State.RoomPointer:X4}/${failure.State.StatePointer:X4} " +
                    $"{failure.State.Symbol}, block {failure.BlockIndex}");
            }
        }

        foreach (IGrouping<string, ScrollAuditLoadBlocker> group in blockers
            .GroupBy(blocker => string.Join(',', blocker.UnsupportedHeaders.Select(header => $"${header:X4}")))
            .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            Console.Error.WriteLine(
                $"production PLM load blocked by header set [{group.Key}]: {group.Count()} state(s)");
            foreach (ScrollAuditLoadBlocker blocker in group)
            {
                Console.Error.WriteLine(
                    $"  room/state $8F:{blocker.State.RoomPointer:X4}/${blocker.State.StatePointer:X4} " +
                    $"{blocker.State.Symbol}, population $8F:{blocker.PopulationPointer:X4}");
            }
        }

        foreach (IGrouping<string, ScrollAuditLoaderFailure> group in loaderFailures
            .GroupBy(failure => $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
            .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            Console.Error.WriteLine(
                $"production PLM loader failed: {group.Key}: {group.Count()} state(s)");
            foreach (ScrollAuditLoaderFailure failure in group)
            {
                Console.Error.WriteLine(
                    $"  room/state $8F:{failure.State.RoomPointer:X4}/${failure.State.StatePointer:X4} " +
                    $"{failure.State.Symbol}, population $8F:{failure.PopulationPointer:X4}");
            }
        }

        foreach (IGrouping<string, ScrollAuditLoaderFailure> group in lifecycleFailures
            .GroupBy(failure => $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
            .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            Console.Error.WriteLine(
                $"first scroll-trigger lifecycle failed: {group.Key}: {group.Count()} state(s)");
            foreach (ScrollAuditLoaderFailure failure in group)
            {
                Console.Error.WriteLine(
                    $"  room/state $8F:{failure.State.RoomPointer:X4}/${failure.State.StatePointer:X4} " +
                    $"{failure.State.Symbol}, population $8F:{failure.PopulationPointer:X4}");
            }
        }

        Console.WriteLine(
            $"Retail scroll ownership audit: {stateCount} states; {loadedStateCount} production-loaded; " +
            $"{sourceOwnerCount} cartridge owners; {verifiedOwnerCount} live owners verified; " +
            $"{failures.Count} ownership failures; {blockers.Count} states blocked by untranslated PLMs; " +
            $"{loaderFailures.Count} other production-loader failures; " +
            $"{lifecycleFailures.Count} first-trigger lifecycle failures.");
    }

    private static ScrollAuditPopulationRecord[] ReadScrollAuditPopulation(
        SuperMetroidAddressSpace bus,
        ushort populationPointer)
    {
        var records = new List<ScrollAuditPopulationRecord>();
        ushort cursor = populationPointer;
        for (int recordIndex = 0; recordIndex < 256; recordIndex++)
        {
            ushort header = ReadWord(bus, 0x8f0000 | cursor);
            if (header == 0)
                return records.ToArray();
            records.Add(new ScrollAuditPopulationRecord(
                header,
                bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 2))),
                bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 3))),
                ReadWord(bus, 0x8f0000 | unchecked((ushort)(cursor + 4)))));
            cursor = unchecked((ushort)(cursor + 6));
        }

        throw new InvalidDataException(
            $"Retail PLM population $8F:{populationPointer:X4} has no zero terminator.");
    }

    private static ScrollAuditRoomState[] LoadScrollAuditStates()
    {
        string symbolPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "upstream-sm",
            "assets",
            "names.txt");
        if (!File.Exists(symbolPath))
            throw new FileNotFoundException("Retail scroll audit requires names.txt.", symbolPath);

        return File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0x8f", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kRoomState_", StringComparison.Ordinal))
            .Select(ParseScrollAuditState)
            .DistinctBy(state => state.StatePointer)
            .OrderBy(state => state.StatePointer)
            .ToArray();
    }

    private static ScrollAuditRoomState ParseScrollAuditState(string line)
    {
        int separator = line.IndexOf(' ');
        const string symbolPrefix = "kRoomState_";
        if (separator < 0 ||
            !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out int stateAddress))
        {
            throw new InvalidDataException($"Malformed room-state symbol: {line}");
        }

        string symbol = line[(separator + 1)..].Trim();
        if (!symbol.StartsWith(symbolPrefix, StringComparison.Ordinal) ||
            symbol.Length < symbolPrefix.Length + 4 ||
            !ushort.TryParse(
                symbol.AsSpan(symbolPrefix.Length, 4),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out ushort roomPointer))
        {
            throw new InvalidDataException(
                $"Room-state symbol does not encode its room header: {line}");
        }

        return new ScrollAuditRoomState(roomPointer, unchecked((ushort)stateAddress), symbol);
    }

    private readonly record struct ScrollAuditPopulationRecord(
        ushort Header,
        byte BlockX,
        byte BlockY,
        ushort RoomArgument);

    private readonly record struct ScrollAuditRoomState(
        ushort RoomPointer,
        ushort StatePointer,
        string Symbol);

    private readonly record struct ScrollOwnershipFailure(
        ScrollAuditRoomState State,
        int BlockIndex,
        string Reason);

    private readonly record struct ScrollAuditLoadBlocker(
        ScrollAuditRoomState State,
        ushort PopulationPointer,
        IReadOnlyList<ushort> UnsupportedHeaders);

    private readonly record struct ScrollAuditLoaderFailure(
        ScrollAuditRoomState State,
        ushort PopulationPointer,
        Exception Exception);
}
