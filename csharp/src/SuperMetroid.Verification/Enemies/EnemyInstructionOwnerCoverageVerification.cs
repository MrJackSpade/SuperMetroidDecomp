using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Globalization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Audits instruction ownership for every named retail enemy definition and population variant.</summary>
internal static class EnemyInstructionOwnerCoverageVerification
{
    /// <summary>
    /// Audits every named retail enemy definition through every population record that
    /// uses it. This is deliberately broader than the family-specific execution tests:
    /// it catches translated actors whose initial instruction stream was never exercised
    /// by a scenario, which is how the three Space Pirate families escaped the first
    /// attempted removal of the generic cartridge fallback.
    /// </summary>
    internal static void VerifyEnemyInstructionOwnerCoverage()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Ordinary-enemy mechanics owners: retail inventory skipped " +
                "(private ROM absent).");
            return;
        }

        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        // A population that places no enemy sets none up.
        RoomEnemyPopulationDefinition[] populations = [.. ReadRetailPopulations(rom).Where(population => population.Placements.Length > 0)];
        EnemyDefinitionId[] definitions = ReadNamedRetailEnemyDefinitions();
        // The shared installed fixtures every room's enemy system and Samus are built from load here, as timed shared
        // fixture loads, so the workers below only read them.
        _ = InstalledFixtures.EnemyOverreads;
        _ = InstalledFixtures.BossAndIntroOverreads;
        _ = InstalledFixtures.EnemyFrameDisplays;
        _ = InstalledFixtures.FramesBeyondAnimation;
        _ = InstalledFixtures.RoomAssets;

        // Each population sets up in a retail room that places it, as that room's load sets it up (#2554): over the room's
        // terrain, whose blocks an enemy's setup reads (an Alcoon lands on its floor), after the room's objects, which an
        // enemy's setup reaches (Mother Brain's glass, Bomb Torizo's hand), each part with the body placed before it. Each
        // room loads in systems of its own, the rooms across the machine's cores (#1757), each writing only its own records'
        // outcomes, which are gathered in population order once every room has loaded. A definition no room places is one
        // only code spawns, joining the encounter of the enemy that spawns it, which the room's species registry refuses to
        // place from a population: each is set up as its spawner's encounter spawns it, in a room of its own that places the
        // spawner (#2554), whose outcomes follow the populations'.
        Dictionary<RoomEnemyPopulationDefinition, SelectedRoom> rooms = RoomsPlacing();
        var audits = new RecordOutcome[populations.Length + SpawningEncounters.Length][];
        ParallelSweep.For(0, audits.Length, index => audits[index] = index < populations.Length
            ? AuditPopulation(rooms[populations[index]])
            : AuditSpawningEncounter(SpawningEncounters[index - populations.Length], populations, rooms));
        int spawnedOutcomes = audits.Skip(populations.Length).Sum(audit => audit.Length);

        HashSet<EnemyDefinitionId> named = [.. definitions];
        var audited = new HashSet<EnemyDefinitionId>();
        var needingOwner = new HashSet<EnemyDefinitionId>();
        var unresolved = new HashSet<EnemyDefinitionId>();
        foreach (RecordOutcome record in audits.SelectMany(audit => audit).Where(record => named.Contains(record.Definition)))
        {
            audited.Add(record.Definition);
            if (!record.ProcessesInstructions)
                continue;
            // Every instruction-processing enemy runs a typed program (#641); none reads cartridge lists.
            needingOwner.Add(record.Definition);
            if (!record.RunsTypedAnimation)
                unresolved.Add(record.Definition);
        }
        EnemyDefinitionId[] owned = [.. needingOwner.Where(definition => !unresolved.Contains(definition))];

        AssertEqual(150, definitions.Length,
            "owner audit enumerates every named retail enemy definition a room places or code spawns, the five Ceres doors and walls included");
        AssertEqual("", string.Join(", ", definitions.Where(definition => !audited.Contains(definition))),
            "owner audit sets up every named retail enemy definition, as a room places it or its spawner's encounter spawns it");
        AssertTrue(owned.Length > 90,
            "owner audit reaches the translated instruction-processing surface");
        // Each entry names its definition by identity (#2554), no longer by a header address.
        AssertEqual("", string.Join(", ", definitions.Where(unresolved.Contains).Select(definition => $"{definition} has no typed animation")),
            "every translated instruction-processing enemy runs a typed animation");

        Console.WriteLine(
            $"  Ordinary-enemy mechanics owners: {audited.Count} of {definitions.Length} named definitions set up through " +
            $"{populations.Length} retail populations and {SpawningEncounters.Length} spawning encounters ({spawnedOutcomes} spawned enemies), " +
            $"{owned.Length} compiled instruction-processing owners, each running a typed animation.");
    }

    /// <summary>What setting one population record's enemy up found.</summary>
    /// <param name="Definition">The record's enemy.</param>
    /// <param name="ProcessesInstructions">Whether the enemy animates and is not deleted after its setup.</param>
    /// <param name="RunsTypedAnimation">Whether the enemy's entity runs a typed animation.</param>
    private sealed record RecordOutcome(EnemyDefinitionId Definition, bool ProcessesInstructions, bool RunsTypedAnimation);

    /// <summary>A Samus as a new game makes her, beside whom a population's enemies are set up.</summary>
    private static SamusState NewSamus() =>
        new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(),
            global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());

    /// <summary>
    /// Loads <paramref name="room"/>'s terrain, its objects and then its enemy population, as the room's load does, and reads
    /// each population record's enemy after its setup, in the place the load put it.
    /// </summary>
    private static RecordOutcome[] AuditPopulation(SelectedRoom room)
    {
        LoadedRoom loaded = LoadRoom(room);
        RoomEnemyPlacement[] records = room.State.EnemyPopulation.Placements.ToArray();
        var outcomes = new RecordOutcome[records.Length];
        for (int index = 0; index < records.Length; index++)
        {
            RoomEnemyPlace place = loaded.Places[index];
            AssertEqual(records[index].DefinitionId, place.HeldOccupant().DefinitionId, $"the load places record {index}'s enemy in its own place");
            outcomes[index] = OutcomeOf(place);
        }
        return outcomes;
    }

    /// <summary>
    /// What setting <paramref name="place"/>'s enemy up found. A place still marked deleted after its enemy's setup (the n00b
    /// tube crack's palette-only record) is emptied by the scheduler before any AI or instruction frame.
    /// </summary>
    private static RecordOutcome OutcomeOf(RoomEnemyPlace place)
    {
        bool processesInstructions = place.Processing.HasAny(EnemyProcessing.Animates) && !place.Presence.HasAny(EnemyPresence.Deleted);
        return new RecordOutcome(place.HeldOccupant().DefinitionId, processesInstructions, place.Entity is IEnemyTypedAnimation);
    }

    /// <summary>
    /// An encounter that spawns named definitions no room places, run on the enemy that spawns them.
    /// </summary>
    /// <param name="Spawner">The enemy whose encounter spawns them, which a retail room places.</param>
    /// <param name="Spawned">The definitions the encounter spawns.</param>
    /// <param name="Run">Runs the encounter on the spawner's place in its loaded room until it has spawned them.</param>
    private sealed record SpawningEncounter(EnemyDefinitionId Spawner, EnemyDefinitionId[] Spawned, Action<LoadedRoom, RoomEnemyPlace> Run);

    /// <summary>
    /// The encounters that spawn the named definitions no room places, each as the game runs it: Ridley's death breaks him
    /// into his breakup's pieces on the last update of its explosions, Ceres Ridley's retreat places his getaway chamber's
    /// walls as its delay runs out before he hands the room to the getaway, and Mother Brain's fake death drops her first
    /// falling tube before her late fight's final beam brings in the Baby.
    /// </summary>
    private static readonly SpawningEncounter[] SpawningEncounters =
    [
        new(EnemyDefinitionId.Ridley, [EnemyDefinitionId.RidleyBreakupPiece], RunRidleyDeath),
        new(EnemyDefinitionId.RidleyCeres, [EnemyDefinitionId.CeresGetawayChamberWall], RunCeresRidleyGetaway),
        new(EnemyDefinitionId.MotherBrainBody, [EnemyDefinitionId.MotherBrainTubes, EnemyDefinitionId.BabyMetroidCutscene], RunMotherBrainCollapseAndLateFight),
    ];

    /// <summary>
    /// Loads the first retail room that places <paramref name="encounter"/>'s spawner, as the room's load does, runs the
    /// encounter on the spawner, and reads each enemy it spawned after its setup, in the place the spawn put it.
    /// </summary>
    /// <exception cref="InvalidOperationException">No retail room places the spawner.</exception>
    private static RecordOutcome[] AuditSpawningEncounter(SpawningEncounter encounter, RoomEnemyPopulationDefinition[] populations,
        Dictionary<RoomEnemyPopulationDefinition, SelectedRoom> rooms)
    {
        int Spawner(RoomEnemyPopulationDefinition population) =>
            Array.FindIndex(population.Placements.ToArray(), record => record.DefinitionId == encounter.Spawner);
        RoomEnemyPopulationDefinition placing = populations.FirstOrDefault(population => Spawner(population) >= 0)
            ?? throw new InvalidOperationException($"No retail room places {encounter.Spawner}, whose encounter spawns {string.Join(", ", encounter.Spawned)}.");
        LoadedRoom loaded = LoadRoom(rooms[placing]);
        encounter.Run(loaded, loaded.Places[Spawner(placing)]);
        RoomEnemyPlace[] spawned = [.. loaded.Enemies.Places.InUpdateOrder.Where(place =>
            place.Occupant is { } occupant && encounter.Spawned.Contains(occupant.DefinitionId))];
        foreach (EnemyDefinitionId definition in encounter.Spawned)
        {
            AssertTrue(spawned.Any(place => place.HeldOccupant().DefinitionId == definition),
                $"{encounter.Spawner}'s encounter spawns {definition}");
        }
        return [.. spawned.Select(OutcomeOf)];
    }

    /// <summary>Runs Lower Norfair Ridley's main AI on the last update of his death's explosions, which breaks him up.</summary>
    private static void RunRidleyDeath(LoadedRoom room, RoomEnemyPlace body) =>
        NorfairRidleyDeathFixtures.RunPhase(body, body.EntityOf<NorfairRidleyEntity>().Encounter, NorfairRidleyPhase.DeathExplosions, 1, room.Samus);

    /// <summary>
    /// Runs Ceres Ridley's main AI on the last update of his retreat's delay, which places the getaway chamber's walls, and
    /// on the next, which hands the room to the getaway as it begins, and then the getaway room main's first step.
    /// </summary>
    private static void RunCeresRidleyGetaway(LoadedRoom room, RoomEnemyPlace body)
    {
        CeresRidleyState ridley = body.EntityOf<CeresRidleyEntity>().Encounter;
        ridley.Phase = CeresRidleyPhase.RetreatDelay;
        ridley.PhaseTimer.LastFor(1);
        body.RunMainAi(room.Samus);
        body.RunMainAi(room.Samus);
        AssertEqual(CeresEscapeStage.Getaway, room.Enemies.CeresEscape.Stage, "Ceres Ridley hands the room to the getaway after his retreat");
        room.Enemies.CeresEscape.RunGetawayRoomMain(room.Samus, RoomBackgroundsFixtures.CeresFloor(), 0);
    }

    /// <summary>
    /// Runs Mother Brain's body's main AI as her fake death's tube collapse begins, which drops its first tube once enough
    /// enemy projectiles are free, and then on the last update of her late fight's final-beam wait, which brings in the Baby.
    /// </summary>
    private static void RunMotherBrainCollapseAndLateFight(LoadedRoom room, RoomEnemyPlace body)
    {
        MotherBrainEnemyState motherBrain = body.EntityOf<MotherBrainBodyEntity>().Encounter;
        motherBrain.DescentPhase = MotherBrainDescentPhase.CollapseTubes;
        motherBrain.TubeCollapse = MotherBrainTubeCollapseProgress.Start;
        motherBrain.Body.RunMainAi(room.Samus);
        _ = MotherBrainBabyArrival.BringInBaby(motherBrain, room.Samus);
    }

    /// <summary>A retail room loaded as its load sets it up: its enemies, the places its population's records hold, and Samus.</summary>
    /// <param name="Enemies">The room's enemies.</param>
    /// <param name="Places">Each population record's place, in the records' order.</param>
    /// <param name="Samus">Samus, beside whom the room's enemies are set up.</param>
    private sealed record LoadedRoom(RoomEnemySystem Enemies, IReadOnlyList<RoomEnemyPlace> Places, SamusState Samus);

    /// <summary>Loads <paramref name="room"/>'s terrain, its objects and then its enemy population, as the room's load does.</summary>
    private static LoadedRoom LoadRoom(SelectedRoom room)
    {
        RoomLevelData terrain = LoadedRoomAssets.Load(RoomScrollGrid.PowerOnCells(), room, InstalledFixtures.RoomAssets).LevelData;
        // The room's objects set up before its enemies, in a game that has seen no story event and beaten no boss.
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(terrain, new SharedGameState(new SharedRandom()), room.Area),
            room.State.ObjectPopulation, RoomObjectServiceFixtures.Uninstalled with
            {
                Samus = new FixtureRoomSamus(NewSamus),
                Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false, IsBossDefeated = () => false, IsMiniBossDefeated = () => false },
                Events = new FixtureEventFlags { HasEvent = _ => false, SetEvent = _ => { } },
            });
        RoomObjectsReachedByEnemies reached = RoomObjectFixtures.ReachedByEnemiesOf(roomObjects);
        var enemies = EnemySystemFixtures.PlacingAnyEnemy(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().RoomGlass = reached.MotherBrainGlass;
        enemies.VerificationServices().RoomHand = reached.BombTorizoHand;
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        enemies.VerificationServices().NextRandom = () => 0;
        enemies.VerificationServices().ReadRandom = () => 0;
        // A species that asks whether its area's boss is dead sees it alive.
        enemies.VerificationServices().IsAreaBossDefeated = () => false;
        // A species whose setup writes its room's screens' scrolls (Crocomire's) writes them into a room the check keeps no
        // scroll grid for.
        enemies.VerificationServices().SetRoomScrollState = (_, _) => { };
        SamusState samus = NewSamus();
        IReadOnlyList<RoomEnemyPlace> places = enemies.LoadRoom(room.State.EnemyPopulation, samus, terrain);
        return new LoadedRoom(enemies, places, samus);
    }

    /// <summary>
    /// The bank-$A0 headers no room places and no code spawns: Bang, the spinning turtle eye, the horizontal shutter, the
    /// second Sbug, the green and red wall pirates, the grey, green, red and magenta ninja pirates, and the four whose
    /// artwork only graphics sets load, Stoke, the Reflec, the dead Sidehopper's second record and the Tourian statue ghost
    /// (#1686), Spore Spawn's stalk, whose drop table the port holds as the spores' own chances (#2244), and the two Torizo
    /// orbs, which the Torizos throw as projectiles and whose drop tables the orbs name as their own chances (#2332, #2364).
    /// </summary>
    private static readonly HashSet<ushort> UnplacedEnemyHeaders =
        [0xceff, 0xd57f, 0xd8bf, 0xdb3f, 0xdbff, 0xdf7f, 0xedbf, 0xef3f, 0xefbf, 0xf03f, 0xf153, 0xf393, 0xf3d3, 0xf4d3, 0xf513, 0xf553, 0xf5d3];

    /// <summary>
    /// Extracts the species of the distinct bank-$A0 enemy headers named in the pinned symbol inventory, and of the header
    /// every Ceres door and wall shares, which the inventory labels as other data rather than as an enemy definition
    /// (#1999). A header no room places and no code spawns (<see cref="UnplacedEnemyHeaders"/>) is cartridge inventory
    /// with no species, and is not one. The inventory also names the header the cartridge leaves in a killed enemy's place
    /// while it waits to respawn (<see cref="EnemyHeaderWordsConstants.AwaitingRespawnHeader"/>): the port holds that place as
    /// awaiting respawn, with no enemy and no definition, so the audit recognises it as that placeholder and requires the
    /// inventory to name it. Every other header names a species or the audit fails.
    /// </summary>
    private static EnemyDefinitionId[] ReadNamedRetailEnemyDefinitions()
    {
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        HashSet<ushort> namedHeaders = File.ReadLines(symbolPath)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(fields => fields.Length == 2 &&
                fields[0].StartsWith("0xa0", StringComparison.Ordinal) &&
                fields[1].StartsWith("kEnemyDef_", StringComparison.Ordinal))
            .Select(fields => ushort.Parse(
                fields[0].AsSpan(4),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture))
            .ToHashSet();
        AssertTrue(namedHeaders.Remove(EnemyHeaderWordsConstants.AwaitingRespawnHeader),
            "the symbol inventory names the awaiting-respawn placeholder, which the audit holds as no enemy definition");
        IEnumerable<ushort> sharedHeaders = EnemyHeaderWords.SharedHeaders.Keys.Select(EnemyDefinitionIds.HeaderPointer);
        return namedHeaders
            .Union(sharedHeaders)
            .Where(header => !UnplacedEnemyHeaders.Contains(header))
            .SelectMany(DefinitionsPlacedBy)
            .Distinct()
            .Order()
            .ToArray();
    }

    /// <summary>
    /// The definitions a placement of <paramref name="header"/> places: every actor sharing it (#1989), whose placement's
    /// first parameter selects among them, or the one species it names.
    /// </summary>
    /// <exception cref="InvalidDataException">The header is not shared and names no species.</exception>
    private static EnemyDefinitionId[] DefinitionsPlacedBy(ushort header)
    {
        EnemyDefinitionId[] sharing =
            [.. EnemyHeaderWords.SharedHeaders.Keys.Where(actor => EnemyDefinitionIds.HeaderPointer(actor) == header)];
        return sharing.Length > 0 ? sharing : [EnemyDefinitionIdsConstants.FromHeaderPointer(header)];
    }

    /// <summary>Each retail population, with the first room state that places it, in the rooms' and their states' order.</summary>
    private static Dictionary<RoomEnemyPopulationDefinition, SelectedRoom> RoomsPlacing()
    {
        var rooms = new Dictionary<RoomEnemyPopulationDefinition, SelectedRoom>(ReferenceEqualityComparer.Instance);
        foreach ((RoomDefinition room, RoomStateChoice choice) in RoomStateSelectionDefinitions.Choices)
        {
            foreach (RoomStateSettings state in choice.Branches.Select(branch => branch.State).Prepend(choice.Default))
                rooms.TryAdd(state.EnemyPopulation, new SelectedRoom(room, state));
        }
        return rooms;
    }

    /// <summary>
    /// The distinct enemy populations of every retail room state, each checked against the cartridge's native records: its
    /// entries place the enemies the native records do, in their order.
    /// </summary>
    private static RoomEnemyPopulationDefinition[] ReadRetailPopulations(ISnesAddressSpace rom)
    {
        RoomStateSettings[] states = RoomStateDefinitions.All.ToArray();
        AssertEqual(RoomStateDefinitions.RetailStateCount, states.Length,
            "owner fixture enumerates every retail room state");
        RoomEnemyPopulationDefinition[] populations = [.. states.Select(state => state.EnemyPopulation).Distinct()];
        foreach (RoomEnemyPopulationDefinition population in populations)
        {
            ushort populationPointer = RoomEnemyPopulationDefinitions.PointerOf(population);
            var native = new List<EnemyDefinitionId>();
            int cursor = RoomEnemyRomLayout.PopulationBank | populationPointer;
            for (int recordIndex = 0;
                 recordIndex < EnemyPlaceOrder.Capacity;
                 recordIndex++, cursor += 16)
            {
                ushort definitionPointerWord = rom.ReadCartridgeWord(cursor);
                if (definitionPointerWord == 0xffff)
                    break;
                // A header several actors share places the one its first parameter selects (#1989).
                native.Add(EnemySpawnSettingsCodec.PlacedActor(definitionPointerWord, rom.ReadCartridgeWord(cursor + 12)));
            }
            AssertEqual(string.Join(", ", native), string.Join(", ", population.Placements.ToArray().Select(record => record.DefinitionId)),
                $"population {population.Name} places the native records' enemies in their order");
        }
        return populations;
    }

    /// <summary>Runs the retail-wide audit of translated enemy instruction ownership.</summary>
    [VerificationCommand("--enemy-instruction-owner-coverage")]
    internal static int EnemyInstructionOwnerCoverageCommand()
    {
        Suite(nameof(VerifyEnemyInstructionOwnerCoverage), () => VerifyEnemyInstructionOwnerCoverage());
        return 0;
    }
}
