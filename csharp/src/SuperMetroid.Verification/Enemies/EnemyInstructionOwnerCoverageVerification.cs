using SuperMetroid.Core.Audio;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Globalization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
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
        Dictionary<EnemyDefinitionId, List<RoomEnemyPlacement>> populations =
            ReadRetailEnemyPopulationRecords(rom);
        EnemyDefinitionId[] definitions = ReadNamedRetailEnemyDefinitions();
        // Each definition's header is read from the cartridge here, on the suite's own thread.
        RoomEnemyDefinition[] headers = [.. definitions.Select(definitionPointer =>
            SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.Load(rom, definitionPointer))];
        // The shared installed fixtures every record's enemy system and Samus are built from load here too, as timed shared
        // fixture loads, so the workers below only read them.
        _ = InstalledFixtures.EnemyOverreads;
        _ = InstalledFixtures.BossAndIntroOverreads;
        _ = InstalledFixtures.EnemyFrameDisplays;
        _ = InstalledFixtures.FramesBeyondAnimation;

        // Each definition's records run on one worker, the definitions across the machine's cores (#1757): every record
        // builds its own enemy system, services, artwork binding and Samus, and reads only the immutable header, records
        // and installed fixtures, so the definitions are independent. A definition writes only its own outcome, which is
        // gathered in definition order once every definition has run.
        var outcomes = new DefinitionOutcome[definitions.Length];
        ParallelSweep.For(0, definitions.Length, index =>
            outcomes[index] = AuditDefinition(definitions[index], headers[index], populations));

        var translated = new HashSet<EnemyDefinitionId>();
        var owned = new HashSet<EnemyDefinitionId>();
        var withoutSpecies = new List<EnemyDefinitionId>();
        var unresolved = new List<string>();
        for (int index = 0; index < definitions.Length; index++)
        {
            DefinitionOutcome outcome = outcomes[index];
            if (outcome.Translated)
                translated.Add(definitions[index]);
            if (outcome.Owned)
                owned.Add(definitions[index]);
            if (outcome.WithoutSpecies)
                withoutSpecies.Add(definitions[index]);
            unresolved.AddRange(outcome.Unresolved);
        }

        AssertEqual(153, definitions.Length,
            "owner audit enumerates every named retail enemy definition a room places or code spawns, the five Ceres doors and walls included");
        AssertTrue(translated.Count > 100,
            "owner audit recognizes the translated retail initializer surface");
        AssertTrue(owned.Count > 90,
            "owner audit reaches the translated instruction-processing surface");
        HashSet<ushort> unresolvedDefinitions = unresolved
            .Select(entry => ushort.Parse(
                entry.AsSpan(1, 4),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture))
            .ToHashSet();
        AssertEqual(0, unresolvedDefinitions.Count,
            "every translated instruction-processing enemy runs a typed animation; actual=" +
            string.Join(", ", unresolved.Distinct()));

        // A definition the game reads but never places in a slot has no species (#2010): Spore Spawn's stalk and the Torizo
        // orbs only lend the drops of a spore impact and a broken orb their chances.
        AssertEqual("SporeSpawnStalk, BombTorizoOrb, GoldenTorizoOrb", string.Join(", ", withoutSpecies),
            "only the definitions the game never places have no species");

        var unknownEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot unknown = unknownEnemies.SlotTable.Slots[0];
        // A definition no species owns has no entity to become (#2010): binding it fails, naming the definition, instead
        // of creating a placeholder that runs nothing.
        const EnemyDefinitionId unowned = (EnemyDefinitionId)0x9000;
        InvalidOperationException unknownException = AssertThrows<InvalidOperationException>(
            () => unknown.Bind(unowned, unknown.Definition),
            "binding a definition no species owns fails");
        AssertTrue(
            unknownException.Message.Contains($"No enemy species owns definition {unowned}", StringComparison.Ordinal),
            "the failure names the unowned definition");

        Console.WriteLine(
            $"  Ordinary-enemy mechanics owners: {definitions.Length} named definitions, " +
            $"{translated.Count} translated initializers, {owned.Count} compiled " +
            $"instruction-processing owners, and {unresolvedDefinitions.Count} " +
            "explicitly inventoried pending families.");
    }

    /// <summary>What auditing one definition's records found.</summary>
    /// <param name="Translated">Whether a record's initialization ran translated code.</param>
    /// <param name="Owned">Whether a record processes instructions and one that does runs a typed animation.</param>
    /// <param name="Unresolved">Each instruction-processing record with no typed animation, in record order.</param>
    /// <param name="WithoutSpecies">Whether no species owns the definition, so no slot can hold it.</param>
    private sealed record DefinitionOutcome(bool Translated, bool Owned, IReadOnlyList<string> Unresolved, bool WithoutSpecies = false);

    /// <summary>Initializes <paramref name="definitionPointer"/> from each population record placing it, each in an enemy system of its own.</summary>
    private static DefinitionOutcome AuditDefinition(EnemyDefinitionId definitionPointer, RoomEnemyDefinition definition,
        Dictionary<EnemyDefinitionId, List<RoomEnemyPlacement>> populations)
    {
        var unresolved = new List<string>();
        IReadOnlyList<RoomEnemyPlacement> records =
            populations.TryGetValue(definitionPointer, out List<RoomEnemyPlacement>? found)
                ? found
                : [RoomEnemyPopulationRecordCodec.Decode(definitionPointer, 128, 128, 0, 0, 0,
                    EnemySpawnSettingsCodec.FirstParameterPlacing(definitionPointer), 0)];

        bool definitionTranslated = false;
        bool definitionNeedsOwner = false;
        bool definitionHasOwner = false;
        foreach (RoomEnemyPlacement population in records)
        {
            var enemies =EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
            enemies.VerificationServices().NextRandom = () => 0;
            enemies.VerificationServices().ReadRandom = () => 0;

            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            // The placement's settings are recorded before the entity is bound, as a room load records them, so a species
            // that places several actors (the Ceres doors and walls) creates the one the placement names (#1953, #1999).
            slot.SetSpawnSettings(population.SpawnSettings);
            try
            {
                slot.Bind(definitionPointer, slot.Definition);
            }
            catch (InvalidOperationException exception) when (
                exception.Message.StartsWith("No enemy species owns definition", StringComparison.Ordinal))
            {
                // The registry refuses a definition no species owns; the audit records it and the caller asserts which.
                return new DefinitionOutcome(false, false, [], WithoutSpecies: true);
            }
            slot.Bind(slot.DefinitionId, definition);
            slot.Radius = new(definition.Radius.X, definition.Radius.Y);
            slot.Health = definition.Health;
            slot.Position = slot.Position.WithWhole(new(population.Position.X, population.Position.Y));
            slot.Contact = population.Contact;
            slot.Processing = population.Processing;
            slot.Presence = population.Presence;
            slot.CollidesByFrameHitboxes = population.CollidesByFrameHitboxes;
            slot.AnimationClock.ShowsNewFrame = false;

            slot.AnimationClock.StepOnNextUpdate();

            bool translatedRecord = true;
            try
            {
                EnemyLifecycle.RunInitializationAi(slot, EnemySpawnSurroundings.None with { Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()) });
            }
            catch (Exception exception)
                when (IsUntranslatedEnemyInitializer(
                    exception, definitionPointer))
            {
                translatedRecord = false;
            }
            catch (Exception)
            {
                // A matched initializer may require its native sibling slot, room
                // geometry, or encounter state. Matching the dispatcher still proves
                // this is translated code; the population's instruction ownership is
                // independent of completing that encounter-specific initialization.
            }

            if (!translatedRecord)
                continue;

            definitionTranslated = true;
            // A record still marked deleted after initialization (the n00b tube crack's palette-only record) is
            // removed by the scheduler before any AI or instruction frame.
            bool processesInstructions =
                slot.Processing.HasAny(EnemyProcessing.Animates) &&
                !slot.Presence.HasAny(EnemyPresence.Deleted);
            if (!processesInstructions)
                continue;

            // Every instruction-processing enemy runs a typed program (#641); none reads cartridge lists.
            definitionNeedsOwner = true;
            if (slot.Entity is IEnemyTypedAnimation { HasTypedAnimation: true })
                definitionHasOwner = true;
            else
                unresolved.Add($"{definitionPointer} has no typed animation");
        }

        return new DefinitionOutcome(definitionTranslated, definitionNeedsOwner && definitionHasOwner, unresolved);
    }

    /// <summary>Recognizes the dispatcher's specific failure for a definition without translated initialization AI.</summary>
    private static bool IsUntranslatedEnemyInitializer(
        Exception? exception,
        EnemyDefinitionId definitionPointer) =>
        exception is InvalidDataException invalid &&
        invalid.Message.StartsWith(
            $"Enemy {definitionPointer} initialization AI $",
            StringComparison.Ordinal) &&
        invalid.Message.EndsWith(" is not translated.", StringComparison.Ordinal);

    /// <summary>
    /// The bank-$A0 headers no room places and no code spawns: Bang, the spinning turtle eye, the horizontal shutter, the
    /// second Sbug, the green and red wall pirates, the grey, green, red and magenta ninja pirates, and the four whose
    /// artwork only graphics sets load, Stoke, the Reflec, the dead Sidehopper's second record and the Tourian statue ghost
    /// (#1686).
    /// </summary>
    private static readonly HashSet<ushort> UnplacedEnemyHeaders =
        [0xceff, 0xd57f, 0xd8bf, 0xdb3f, 0xdbff, 0xedbf, 0xf03f, 0xf153, 0xf393, 0xf3d3, 0xf4d3, 0xf513, 0xf553, 0xf5d3];

    /// <summary>
    /// Extracts the species of the distinct bank-$A0 enemy headers named in the pinned symbol inventory, and of the header
    /// every Ceres door and wall shares, which the inventory labels as other data rather than as an enemy definition
    /// (#1999). A header no room places and no code spawns (<see cref="UnplacedEnemyHeaders"/>) is cartridge inventory
    /// with no species, and is not one. The inventory also names the header the cartridge leaves in a killed enemy's place
    /// while it waits to respawn (<see cref="EnemyHeaderWords.AwaitingRespawnHeader"/>): the port holds that place as
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
        AssertTrue(namedHeaders.Remove(EnemyHeaderWords.AwaitingRespawnHeader),
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
        return sharing.Length > 0 ? sharing : [EnemyDefinitionIds.FromHeaderPointer(header)];
    }

    /// <summary>Groups native population records from all retail room states by enemy definition.</summary>
    private static Dictionary<EnemyDefinitionId, List<RoomEnemyPlacement>>
        ReadRetailEnemyPopulationRecords(ISnesAddressSpace rom)
    {
        RoomStateSettings[] states = RoomStateDefinitions.All.ToArray();
        AssertEqual(RoomStateDefinitions.RetailStateCount, states.Length,
            "owner fixture enumerates every retail room state");
        var records = new Dictionary<EnemyDefinitionId, List<RoomEnemyPlacement>>();

        foreach (ushort populationPointer in states
                     .Select(state => RoomEnemyPopulationDefinitions.PointerOf(state.EnemyPopulation))
                     .Distinct())
        {
            int cursor = RoomEnemyRomLayout.PopulationBank | populationPointer;
            for (int recordIndex = 0;
                 recordIndex < EnemySlotOrder.Capacity;
                 recordIndex++, cursor += 16)
            {
                ushort definitionPointerWord = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor);
                if (definitionPointerWord == 0xffff)
                    break;
                int Word(int offset) => CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + offset);
                // A header several actors share places the one its first parameter selects (#1989).
                EnemyDefinitionId definitionPointer = EnemySpawnSettingsCodec.PlacedActor(definitionPointerWord, Word(12));

                RoomEnemyPlacement record = RoomEnemyPopulationRecordCodec.Decode(
                    definitionPointer, Word(2), Word(4), Word(6), Word(8), Word(10), Word(12), Word(14));
                if (!records.TryGetValue(
                    definitionPointer,
                    out List<RoomEnemyPlacement>? definitionRecords))
                {
                    definitionRecords = [];
                    records.Add(definitionPointer, definitionRecords);
                }
                definitionRecords.Add(record);
            }
        }

        return records;
    }

    /// <summary>Runs the retail-wide audit of translated enemy instruction ownership.</summary>
    [VerificationCommand("--enemy-instruction-owner-coverage")]
    internal static int EnemyInstructionOwnerCoverageCommand()
    {
        Suite(nameof(VerifyEnemyInstructionOwnerCoverage), () => VerifyEnemyInstructionOwnerCoverage());
        return 0;
    }
}
