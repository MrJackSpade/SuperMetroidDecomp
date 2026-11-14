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


        var translated = new HashSet<EnemyDefinitionId>();
        var owned = new HashSet<EnemyDefinitionId>();
        var unresolved = new List<string>();

        foreach (EnemyDefinitionId definitionPointer in definitions)
        {
            RoomEnemyDefinition definition = SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.Load(
                rom, definitionPointer);
            IReadOnlyList<RoomEnemyPlacement> records =
                populations.TryGetValue(definitionPointer, out List<RoomEnemyPlacement>? found)
                    ? found
                    : [RoomEnemyPopulationRecordCodec.Decode(definitionPointer, 128, 128, 0, 0, 0, 0, 0)];

            bool definitionTranslated = false;
            bool definitionNeedsOwner = false;
            bool definitionHasOwner = false;
            foreach (RoomEnemyPlacement population in records)
            {
                var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
                enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
                enemies.VerificationServices().NextRandom = () => 0;
                enemies.VerificationServices().ReadRandom = () => 0;

                RoomEnemySlot slot = enemies.SlotTable.Slots[0];
                slot.Bind(definitionPointer, slot.Definition);
                slot.Bind(slot.DefinitionId, definition);
                slot.Radius = new(definition.Radius.X, definition.Radius.Y);
                slot.Health = definition.Health;
                slot.Position = slot.Position.WithWhole(new(population.Position.X, population.Position.Y));
                slot.SetSpawnSettings(population.SpawnSettings);
                slot.Contact = population.Contact;
                slot.Processing = population.Processing;
                slot.Presence = population.Presence;
                slot.CollidesByFrameHitboxes = population.CollidesByFrameHitboxes;
                slot.AnimationClock.ShowsNewFrame = false;

                slot.AnimationClock.StepOnNextUpdate();

                bool translatedRecord = true;
                try
                {
                    EnemyLifecycle.RunInitializationAi(slot, EnemySpawnSurroundings.None with { Samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) });
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

            if (definitionTranslated)
                translated.Add(definitionPointer);
            if (definitionNeedsOwner && definitionHasOwner)
                owned.Add(definitionPointer);
        }

        AssertEqual(149, definitions.Length,
            "owner audit enumerates every named retail enemy definition a room places or code spawns");
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

        var unknownEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot unknown = unknownEnemies.SlotTable.Slots[0];
        unknown.Bind((EnemyDefinitionId)0x9000, unknown.Definition);
        unknown.Bind(unknown.DefinitionId, default(RoomEnemyDefinition));
        InvalidDataException unknownException = AssertThrows<InvalidDataException>(
            () => EnemyInstructionRunner.Process([unknown, null, null, Position16<RoomPixels>.Zero, ControllerButton.None]),
            "an instruction-processing enemy without a typed animation fails instead of reading cartridge lists");
        AssertTrue(
            unknownException.Message.Contains("has no typed animation", StringComparison.Ordinal),
            "the failure identifies the missing typed animation");

        Console.WriteLine(
            $"  Ordinary-enemy mechanics owners: {definitions.Length} named definitions, " +
            $"{translated.Count} translated initializers, {owned.Count} compiled " +
            $"instruction-processing owners, and {unresolvedDefinitions.Count} " +
            "explicitly inventoried pending families.");
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
    /// Extracts the species of the distinct bank-$A0 enemy headers named in the pinned symbol inventory. A header no
    /// room places and no code spawns (<see cref="UnplacedEnemyHeaders"/>) is cartridge inventory with no species, and is
    /// not one.
    /// </summary>
    private static EnemyDefinitionId[] ReadNamedRetailEnemyDefinitions()
    {
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        return File.ReadLines(symbolPath)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(fields => fields.Length == 2 &&
                fields[0].StartsWith("0xa0", StringComparison.Ordinal) &&
                fields[1].StartsWith("kEnemyDef_", StringComparison.Ordinal))
            .Select(fields => ushort.Parse(
                fields[0].AsSpan(4),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture))
            .Where(header => !UnplacedEnemyHeaders.Contains(header))
            .Select(header => EnemyDefinitionIds.FromHeaderPointer(header))
            .Distinct()
            .Order()
            .ToArray();
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
                EnemyDefinitionId definitionPointer = EnemyDefinitionIds.FromHeaderPointer(definitionPointerWord);

                int Word(int offset) => CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + offset);
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
