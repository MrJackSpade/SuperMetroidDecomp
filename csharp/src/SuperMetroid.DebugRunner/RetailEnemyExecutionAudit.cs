using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Loads and executes every named retail room-state enemy population directly from the ROM.
/// </summary>
/// <remarks>
/// <para>
/// The older coverage audit proves that definitions and population records can be parsed. That
/// is intentionally weaker than proving that the translated engine can consume them. This audit
/// crosses that gap: every state gets its real room dimensions, compressed level data, graphics
/// set, enemy tileset, population, and physical slot ordering before the normal enemy scheduler
/// is allowed to run.
/// </para>
/// <para>
/// This is a first-frame execution gate, not a claim that every long-running state machine has
/// completed. Its job is to make missing initialization AI, main AI, and initial instruction
/// opcodes impossible to hide behind an inventory-only report. Focused family audits remain the
/// authority for complete movement, attacks, damage, death, and long animation loops.
/// </para>
/// </remarks>
internal static class RetailEnemyExecutionAudit
{
    private const int RoomBank = 0x8f0000;
    private const int EnemyPopulationBank = 0xa10000;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string symbolPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "upstream-sm",
            "assets",
            "names.txt");
        if (!File.Exists(symbolPath))
        {
            throw new FileNotFoundException(
                "Retail enemy execution requires upstream-sm/assets/names.txt.",
                symbolPath);
        }

        RetailRoomState[] states = File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0x8f", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kRoomState_", StringComparison.Ordinal))
            .Select(ParseRoomState)
            .DistinctBy(state => state.StatePointer)
            .OrderBy(state => state.StatePointer)
            .ToArray();

        var failures = new List<RetailExecutionFailure>();
        var populations = new HashSet<ushort>();
        var definitions = new HashSet<ushort>();
        int nonEmptyStates = 0;
        int scheduledViews = 0;

        foreach (RetailRoomState state in states)
        {
            try
            {
                CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, state.RoomPointer);
                CartridgeRoomState exactState = CartridgeRoomState.Load(bus, state.StatePointer);

                // CartridgeRoomHeader.Load selects a state from game flags. The symbol map names
                // every alternative state independently, so replace only that selected record.
                // Fixed room facts (area, dimensions, door list) still come from the real header.
                CartridgeRoomHeader room = defaultRoom with { State = exactState };
                CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

                ushort populationPointer = exactState.EnemyPopulationPointer;
                populations.Add(populationPointer);
                ushort[] stateDefinitions = ReadPopulationDefinitions(bus, populationPointer);
                if (stateDefinitions.Length == 0)
                    continue;

                nonEmptyStates++;
                definitions.UnionWith(stateDefinitions);

                LoadedRetailState loaded = LoadState(bus, room, assets);

                // A population can span a room several screens wide. One camera origin cannot
                // admit every actor through $A0's processing-window test, so derive a distinct
                // clamped view for every live slot. Reusing the loaded state preserves native
                // multi-part ownership and avoids inventing isolated enemy records.
                (ushort X, ushort Y)[] views = loaded.Enemies.Slots
                    .Where(slot => slot.EnemyDefinitionPointer is not 0 and not 0xffff)
                    .Select(slot => CameraFor(room, slot))
                    .Distinct()
                    .ToArray();

                foreach ((ushort cameraX, ushort cameraY) in views)
                {
                    // Keep Samus in the active view. Many initial state machines branch on her
                    // signed relative position; a null or wrapped far-away actor would exercise
                    // a host-only situation that retail gameplay cannot produce.
                    loaded.Samus.XPosition = unchecked((ushort)(cameraX + 128));
                    loaded.Samus.YPosition = unchecked((ushort)(cameraY + 112));

                    loaded.Enemies.StepFrame(
                        cameraX,
                        cameraY,
                        timeIsFrozen: false,
                        loaded.Samus,
                        level: assets.LevelData,
                        samusProjectiles: loaded.SamusProjectiles,
                        nmiFrameCounter8: unchecked((byte)scheduledViews),
                        mode7Transform: loaded.Mode7Transform,
                        sharedProjectiles: loaded.SharedProjectiles);
                    scheduledViews++;
                }
            }
            catch (Exception exception)
            {
                failures.Add(new RetailExecutionFailure(state, exception));
            }
        }

        if (failures.Count != 0)
        {
            Console.Error.WriteLine(
                $"Retail enemy execution found {failures.Count} failing room states:");
            foreach (IGrouping<string, RetailExecutionFailure> group in failures
                .GroupBy(failure => $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                Console.Error.WriteLine($"  {group.Key}");
                foreach (RetailExecutionFailure failure in group.Take(12))
                {
                    Console.Error.WriteLine(
                        $"    room/state $8F:{failure.State.RoomPointer:X4}/" +
                        $"${failure.State.StatePointer:X4} {failure.State.Symbol}");
                }
                if (group.Count() > 12)
                    Console.Error.WriteLine($"    ... and {group.Count() - 12} more states");
            }
            return 1;
        }

        Console.WriteLine(
            $"Retail enemy execution audit passed: {states.Length} named room states, " +
            $"{nonEmptyStates} non-empty states, {populations.Count} population pointers, " +
            $"{definitions.Count} definitions, and {scheduledViews} authored enemy views " +
            "loaded and completed their first scheduled frame.");
        return 0;
    }

    private static LoadedRetailState LoadState(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 128,
            YPosition = 112,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var random = new Bank80SystemState();
        var roomScrollBytes = new byte[Math.Max(1, room.WidthInScreens * room.HeightInScreens)];
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            isAreaBossDefeated: () => false,
            hasEvent: _ => false,
            setEvent: _ => { },
            clearEvent: _ => { },
            isAreaMiniBossDefeated: () => false,
            setAreaMiniBossDefeated: () => { },
            isAreaTorizoDefeated: () => false,
            setAreaTorizoDefeated: () => { },
            isRoomPlmPresent: _ => false,
            setSamusControlsEnabled: _ => { },
            setRoomScrollByte: (index, value) =>
            {
                if ((uint)index < (uint)roomScrollBytes.Length)
                    roomScrollBytes[index] = value;
            },
            setAreaBossDefeated: () => { },
            incrementMotherBrainGlassRoomArgument: () => { },
            readRoomScrollByte: index =>
                (uint)index < (uint)roomScrollBytes.Length ? roomScrollBytes[index] : (byte)0,
            setMotherBrainLayerBlendingDefaultConfig: _ => { },
            setMotherBrainBg2Scroll: (_, _) => { });

        return new LoadedRetailState(
            enemies,
            samus,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem(),
            new SamusMode7Transform());
    }

    private static ushort[] ReadPopulationDefinitions(
        ISnesAddressSpace bus,
        ushort populationPointer)
    {
        var definitions = new List<ushort>();
        ushort cursor = populationPointer;
        for (int index = 0; index < RoomEnemySystem.MaximumEnemyCount; index++)
        {
            ushort definition = ReadWord(bus, EnemyPopulationBank | cursor);
            if (definition == 0xffff)
                return definitions.ToArray();
            definitions.Add(definition);
            cursor = unchecked((ushort)(cursor + 16));
        }

        // A retail population is allowed to occupy all 32 managed enemy slots. In that
        // case its terminator is the word immediately following the final 16-byte record,
        // rather than one of the words inspected by the bounded slot loop above.
        if (ReadWord(bus, EnemyPopulationBank | cursor) == 0xffff)
            return definitions.ToArray();

        throw new InvalidDataException(
            $"Population $A1:{populationPointer:X4} has no terminator in 32 records.");
    }

    private static (ushort X, ushort Y) CameraFor(
        CartridgeRoomHeader room,
        RoomEnemySlot slot)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 224);
        return (
            unchecked((ushort)Math.Clamp(slot.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(slot.YPosition - 112, 0, maximumY)));
    }

    private static RetailRoomState ParseRoomState(string line)
    {
        int separator = line.IndexOf(' ');
        const string symbolPrefix = "kRoomState_";
        if (separator < 0 ||
            !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out int stateAddress) ||
            (stateAddress & 0xff0000) != RoomBank)
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

        return new RetailRoomState(roomPointer, unchecked((ushort)stateAddress), symbol);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct RetailRoomState(
        ushort RoomPointer,
        ushort StatePointer,
        string Symbol);

    private sealed record RetailExecutionFailure(
        RetailRoomState State,
        Exception Exception);

    private sealed record LoadedRetailState(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SamusProjectileSystem SamusProjectiles,
        SamusBombProjectileSystem SharedProjectiles,
        SamusMode7Transform Mode7Transform);
}
