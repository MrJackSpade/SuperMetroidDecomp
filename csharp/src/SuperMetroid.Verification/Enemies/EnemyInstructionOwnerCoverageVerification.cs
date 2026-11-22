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
        // A definition no room places is one only code spawns, joining the encounter of the enemy that spawns it: the room's
        // species registry places none of them from a population, so no population record sets one up.
        HashSet<EnemyDefinitionId> placed = [.. populations.SelectMany(population => population.Placements.ToArray()).Select(record => record.DefinitionId)];
        EnemyDefinitionId[] unplaced = [.. definitions.Where(definition => !placed.Contains(definition))];
        AssertEqual(string.Join(", ", CodeSpawnedDefinitions.Order()), string.Join(", ", unplaced.Order()),
            "the named definitions no room places are the ones only code spawns");
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
        // outcomes, which are gathered in population order once every room has loaded.
        Dictionary<RoomEnemyPopulationDefinition, SelectedRoom> rooms = RoomsPlacing();
        var audits = new RecordOutcome[populations.Length][];
        ParallelSweep.For(0, audits.Length, index => audits[index] = AuditPopulation(rooms[populations[index]]));

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
        AssertEqual(definitions.Length - unplaced.Length, audited.Count,
            "owner audit sets up every named retail enemy definition a room places");
        AssertTrue(owned.Length > 90,
            "owner audit reaches the translated instruction-processing surface");
        // Each entry names its definition by identity (#2554), no longer by a header address.
        AssertEqual("", string.Join(", ", definitions.Where(unresolved.Contains).Select(definition => $"{definition} has no typed animation")),
            "every translated instruction-processing enemy runs a typed animation");

        Console.WriteLine(
            $"  Ordinary-enemy mechanics owners: {definitions.Length - unplaced.Length} of {definitions.Length} named definitions set up through " +
            $"{populations.Length} retail populations ({unplaced.Length} only code spawns), {owned.Length} compiled instruction-processing owners, " +
            "each running a typed animation.");
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
        IReadOnlyList<RoomEnemyPlace> places = enemies.LoadRoom(room.State.EnemyPopulation, NewSamus(), terrain);
        RoomEnemyPlacement[] records = room.State.EnemyPopulation.Placements.ToArray();
        var outcomes = new RecordOutcome[records.Length];
        for (int index = 0; index < records.Length; index++)
        {
            RoomEnemyPlace place = places[index];
            AssertEqual(records[index].DefinitionId, place.HeldOccupant().DefinitionId, $"the load places record {index}'s enemy in its own place");
            // A record still marked deleted after initialization (the n00b tube crack's palette-only record) is removed by
            // the scheduler before any AI or instruction frame.
            bool processesInstructions = place.Processing.HasAny(EnemyProcessing.Animates) && !place.Presence.HasAny(EnemyPresence.Deleted);
            outcomes[index] = new RecordOutcome(records[index].DefinitionId, processesInstructions, place.Entity is IEnemyTypedAnimation);
        }
        return outcomes;
    }

    /// <summary>
    /// The named definitions only code spawns, each joining the encounter of the enemy that spawns it: Ridley's breakup
    /// pieces as he dies, the Ceres getaway chamber's walls as Ceres Ridley escapes, the Baby Metroid Mother Brain's last
    /// beam brings in, and the glass tubes that fall in her room.
    /// </summary>
    private static readonly EnemyDefinitionId[] CodeSpawnedDefinitions =
        [EnemyDefinitionId.RidleyBreakupPiece, EnemyDefinitionId.CeresGetawayChamberWall, EnemyDefinitionId.BabyMetroidCutscene, EnemyDefinitionId.MotherBrainTubes];

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
