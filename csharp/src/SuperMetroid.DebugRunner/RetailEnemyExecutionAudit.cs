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
internal static partial class RetailEnemyExecutionAudit
{
    private const int RoomBank = 0x8f0000;
    private const int EnemyPopulationBank = 0xa10000;
    private const int LifecycleFramesPerView = 512;
    private const int ExtendedLifecycleFramesPerView = 2048;

    // These are screen-relative placements, not guessed enemy-state overrides. Each camera
    // view is still derived from an authored actor; moving only Samus across that legal view
    // exercises the ROM's signed left/right and above/below selectors from a fresh load.
    private static readonly SamusPlacement[] CenteredSamusPlacement =
    [
        new("center", 128, 112),
    ];

    private static readonly SamusPlacement[] DirectionalSamusPlacements =
    [
        new("center", 128, 112),
        new("left", 32, 112),
        new("right", 224, 112),
        new("above", 128, 32),
        new("below", 128, 192),
    ];

    public static int Run(string romPath) => Run(
        romPath,
        framesPerView: 1,
        freshLoadPerView: false,
        CenteredSamusPlacement);

    /// <summary>
    /// Runs every authored camera view from a fresh population for long enough to cross
    /// ordinary idle, movement, animation, and attack timers. Unlike the one-frame smoke
    /// gate, one view cannot mutate the starting state observed by the next view.
    /// </summary>
    public static int RunLifecycle(string romPath) => Run(
        romPath,
        framesPerView: LifecycleFramesPerView,
        freshLoadPerView: true,
        CenteredSamusPlacement);

    /// <summary>
    /// Extends the ordinary lifecycle gate to 2,048 frames per authored camera view. This
    /// deliberately remains a separate developer command: it crosses long idle, cooldown,
    /// and phase timers without making the fast 512-frame regression gate four times slower.
    /// Every view still starts from frame zero so a previous camera cannot age, damage, or
    /// delete companion actors before their own long-form execution begins.
    /// </summary>
    public static int RunExtendedLifecycle(string romPath) => Run(
        romPath,
        framesPerView: ExtendedLifecycleFramesPerView,
        freshLoadPerView: true,
        CenteredSamusPlacement);

    /// <summary>
    /// Runs five legal Samus positions around every authored camera view. Each position gets
    /// its own fresh population and 512 frames, preventing a center-only run from hiding the
    /// opposite facing branch, vertical pursuit, retreat, or proximity attack selector.
    /// </summary>
    public static int RunDirectionalLifecycle(string romPath) => Run(
        romPath,
        framesPerView: LifecycleFramesPerView,
        freshLoadPerView: true,
        DirectionalSamusPlacements);

    private static int Run(
        string romPath,
        int framesPerView,
        bool freshLoadPerView,
        IReadOnlyList<SamusPlacement> samusPlacements)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailRoomState[] states = LoadNamedRetailStates();

        var failures = new List<RetailExecutionFailure>();
        var populations = new HashSet<ushort>();
        var definitions = new HashSet<ushort>();
        int nonEmptyStates = 0;
        int scheduledViews = 0;
        int scheduledPlacements = 0;
        long scheduledFrames = 0;

        foreach (RetailRoomState state in states)
        {
            ushort? activeCameraX = null;
            ushort? activeCameraY = null;
            ushort? activeSamusX = null;
            ushort? activeSamusY = null;
            string? activePlacement = null;
            int activeFrame = -1;
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

                LoadedRetailState initialLoad = LoadState(bus, room, assets);

                // A population can span a room several screens wide. One camera origin cannot
                // admit every actor through $A0's processing-window test, so derive a distinct
                // clamped view for every live slot. Reusing the loaded state preserves native
                // multi-part ownership and avoids inventing isolated enemy records.
                (ushort X, ushort Y)[] views = initialLoad.Enemies.Slots
                    .Where(slot => slot.EnemyDefinitionPointer is not 0 and not 0xffff)
                    .Select(slot => CameraFor(room, slot))
                    .Distinct()
                    .ToArray();

                for (int viewIndex = 0; viewIndex < views.Length; viewIndex++)
                {
                    (ushort cameraX, ushort cameraY) = views[viewIndex];
                    activeCameraX = cameraX;
                    activeCameraY = cameraY;

                    for (int placementIndex = 0;
                        placementIndex < samusPlacements.Count;
                        placementIndex++)
                    {
                        SamusPlacement placement = samusPlacements[placementIndex];
                        activePlacement = placement.Name;

                        // The fast one-frame gate intentionally preserves its historical
                        // single population load. Every long-form view/placement starts at
                        // cartridge-authored frame zero, retaining companion slot ordering
                        // without allowing an earlier scenario to age or delete actors.
                        bool useInitialLoad = !freshLoadPerView &&
                            viewIndex == 0 && placementIndex == 0;
                        LoadedRetailState loaded = useInitialLoad
                            ? initialLoad
                            : LoadState(bus, room, assets);

                        // All five coordinates stay inside the active 256x224 gameplay view.
                        // A null or wrapped far-away actor would exercise a host-only state.
                        loaded.Samus.XPosition = unchecked((ushort)(cameraX + placement.X));
                        loaded.Samus.YPosition = unchecked((ushort)(cameraY + placement.Y));
                        activeSamusX = loaded.Samus.XPosition;
                        activeSamusY = loaded.Samus.YPosition;

                        for (activeFrame = 0; activeFrame < framesPerView; activeFrame++)
                        {
                            loaded.Enemies.StepFrame(
                                cameraX,
                                cameraY,
                                timeIsFrozen: false,
                                loaded.Samus,
                                level: assets.LevelData,
                                samusProjectiles: loaded.SamusProjectiles,
                                nmiFrameCounter8: unchecked((byte)scheduledFrames),
                                mode7Transform: loaded.Mode7Transform,
                                sharedProjectiles: loaded.SharedProjectiles);
                            scheduledFrames++;
                        }
                        scheduledPlacements++;
                    }
                    scheduledViews++;
                }
            }
            catch (Exception exception)
            {
                failures.Add(new RetailExecutionFailure(
                    state,
                    activeCameraX,
                    activeCameraY,
                    activeSamusX,
                    activeSamusY,
                    activePlacement,
                    activeFrame,
                    exception));
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
                        $"${failure.State.StatePointer:X4} {failure.State.Symbol}" +
                        FormatFailureLocation(failure));
                }
                if (group.Count() > 12)
                    Console.Error.WriteLine($"    ... and {group.Count() - 12} more states");
            }
            return 1;
        }

        string completion = framesPerView == 1
            ? "completed their first scheduled frame"
            : $"completed {framesPerView} fresh-load frames each ({scheduledFrames} total frames)";
        Console.WriteLine(
            $"Retail enemy execution audit passed: {states.Length} named room states, " +
            $"{nonEmptyStates} non-empty states, {populations.Count} population pointers, " +
            $"{definitions.Count} definitions, and {scheduledViews} authored enemy views " +
            (scheduledPlacements == scheduledViews
                ? string.Empty
                : $"across {scheduledPlacements} Samus placements ") +
            $"loaded and {completion}.");
        return 0;
    }

    private static string FormatFailureLocation(RetailExecutionFailure failure)
    {
        if (failure.CameraX is null || failure.CameraY is null)
            return string.Empty;
        string samus = failure.SamusX is null || failure.SamusY is null
            ? string.Empty
            : $" Samus=({failure.SamusX:X4},{failure.SamusY:X4})/{failure.Placement}";
        return $" view=({failure.CameraX:X4},{failure.CameraY:X4}){samus} " +
            $"frame={failure.Frame}";
    }

    private static LoadedRetailState LoadState(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        bool areaBossDefeated = IsBossSelectedRoomState(
            bus,
            room.Pointer,
            room.State.Pointer);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 128,
            YPosition = 112,
        };
        // Enemy freeze duration reads the same live area word populated by the runtime
        // room loader. Exhaustive fresh-state probes must not silently behave as Crateria.
        samus.LiquidPhysics.RoomIdentity = room.Identity;
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
            isAreaBossDefeated: () => areaBossDefeated,
            hasEvent: _ => false,
            setEvent: _ => { },
            clearEvent: _ => { },
            isAreaMiniBossDefeated: () => false,
            setAreaMiniBossDefeated: () => { },
            isAreaTorizoDefeated: () => false,
            setAreaTorizoDefeated: () => { },
            isRoomPlmPresent: _ => false,
            setSamusControlsEnabled: _ => { },
            setRoomScrollState: (index, state) =>
            {
                if ((uint)index < (uint)roomScrollBytes.Length)
                    roomScrollBytes[index] = (byte)state;
            },
            setAreaBossDefeated: () => { },
            incrementMotherBrainGlassRoomArgument: () => { },
            readRoomScrollState: index =>
                (uint)index < (uint)roomScrollBytes.Length
                    ? RoomScrollStates.FromCartridge(
                        roomScrollBytes[index], $"retail enemy audit scroll cell {index}")
                    : RoomScrollState.RedBoundary,
            setMotherBrainLayerBlendingDefaultConfig: _ => { },
            setMotherBrainBg2Scroll: (_, _) => { });

        return new LoadedRetailState(
            enemies,
            samus,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem(),
            new SamusMode7Transform());
    }

    /// <summary>
    /// Determines whether an explicitly named room state is selected by one of the retail
    /// area's eight boss bits. The exhaustive audit replaces the loader's default state with
    /// every symbolized alternative, so the callbacks supplied to enemy initialization must
    /// describe that same cartridge branch (powered Work Robots are the first observable
    /// consumer). Trying the real selector with each one-hot boss mask avoids room/enemy IDs.
    /// </summary>
    private static bool IsBossSelectedRoomState(
        ISnesAddressSpace bus,
        ushort roomPointer,
        ushort statePointer)
    {
        ushort defaultState = CartridgeRoomHeader.Load(bus, roomPointer).State.Pointer;
        if (defaultState == statePointer)
            return false;

        for (int bit = 1; bit <= 0x80; bit <<= 1)
        {
            var selection = new RoomStateSelectionContext(
                Events: ReadOnlyMemory<byte>.Empty,
                BossBits: unchecked((BossBits)bit),
                HasMorphBallAndMissiles: false,
                HasPowerBombs: false);
            if (CartridgeRoomHeader.Load(bus, roomPointer, selection).State.Pointer ==
                statePointer)
            {
                return true;
            }
        }

        return false;
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

    private static RetailRoomState[] LoadNamedRetailStates()
    {
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

        RetailRoomState[] named = File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0x8f", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kRoomState_", StringComparison.Ordinal))
            .Select(ParseRoomState)
            .DistinctBy(state => state.StatePointer)
            .OrderBy(state => state.StatePointer)
            .ToArray();
        RetailRoomState[] debug = named.Where(state => state.RoomPointer == RetailAuditRoomDefinitions.DebugRoom).ToArray();
        if (debug.Length != 1 || debug[0].StatePointer != RetailAuditRoomDefinitions.DebugState)
            throw new InvalidDataException("Pinned unused debug-room inventory changed.");
        Console.WriteLine("Retail inventory explicitly excludes unused debug room $8F:E82C/$E839 (area seven).");
        return named.Where(state => state.RoomPointer != RetailAuditRoomDefinitions.DebugRoom).ToArray();
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct RetailRoomState(
        ushort RoomPointer,
        ushort StatePointer,
        string Symbol);

    private sealed record RetailExecutionFailure(
        RetailRoomState State,
        ushort? CameraX,
        ushort? CameraY,
        ushort? SamusX,
        ushort? SamusY,
        string? Placement,
        int Frame,
        Exception Exception);

    private readonly record struct SamusPlacement(string Name, ushort X, ushort Y);

    private sealed record LoadedRetailState(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SamusProjectileSystem SamusProjectiles,
        SamusBombProjectileSystem SharedProjectiles,
        SamusMode7Transform Mode7Transform);
}
