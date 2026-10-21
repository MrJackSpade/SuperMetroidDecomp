using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Globalization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

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

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Dictionary<EnemyDefinitionId, List<RoomEnemyPopulationRecord>> populations =
            ReadRetailEnemyPopulationRecords(rom);
        EnemyDefinitionId[] definitions = ReadNamedRetailEnemyDefinitions();


        var translated = new HashSet<EnemyDefinitionId>();
        var owned = new HashSet<EnemyDefinitionId>();
        var unresolved = new List<string>();

        foreach (EnemyDefinitionId definitionPointer in definitions)
        {
            RoomEnemyDefinition definition = SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.Load(
                rom, definitionPointer);
            IReadOnlyList<RoomEnemyPopulationRecord> records =
                populations.TryGetValue(definitionPointer, out List<RoomEnemyPopulationRecord>? found)
                    ? found
                    : [new RoomEnemyPopulationRecord(definitionPointer, new(128, 128), 0, default, default, 0, 0)];

            bool definitionTranslated = false;
            bool definitionNeedsOwner = false;
            bool definitionHasOwner = false;
            foreach (RoomEnemyPopulationRecord population in records)
            {
                var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
                enemies.Presentation.BindVram(BgArtworkFixtures.For(new SnesVram()));
                enemies.Presentation.BindCgram(new SnesCgram());
                enemies.VerificationServices().NextRandom = () => 0;
                enemies.VerificationServices().ReadRandom = () => 0;

                RoomEnemySlot slot = enemies.SlotTable.Slots[0];
                try
                {
                    slot.Bind(definitionPointer, slot.Definition);
                }
                catch (InvalidDataException exception) when (IsUnportedSpecies(exception, definitionPointer))
                {
                    // The registry refuses a room-placed header whose species AI no code ports.
                    continue;
                }
                slot.Bind(slot.DefinitionId, definition);
                slot.Radius = new(definition.Radius.X, definition.Radius.Y);
                slot.Health = definition.Health;
                slot.Layer = definition.Layer;
                slot.Position = slot.Position.WithWhole(new(population.Position.X, population.Position.Y));
                slot.CurrentInstruction = checked((ushort)population.InitializationParameter);
                slot.Properties = population.Properties;
                slot.ExtraProperties = population.ExtraProperties;
                slot.SetSpawnParameter1(checked((ushort)population.Parameter1));
                slot.SetSpawnParameter2(checked((ushort)population.Parameter2));
                slot.InstructionTimer = WrappingCountdown.Start(1);

                bool translatedRecord = true;
                try
                {
                    EnemySlotDispatch.RunInitializationAi(slot, EnemySpawnSurroundings.None with { Samus = new SamusState() });
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
                    slot.Properties.HasAny(EnemyProperties.ProcessInstructions) &&
                    !slot.Properties.HasAny(EnemyProperties.Deleted);
                if (!processesInstructions)
                    continue;

                // Every instruction-processing enemy runs a typed program (#641); none reads cartridge lists.
                definitionNeedsOwner = true;
                if ((slot.Entity as IEnemyTypedAnimation)?.FindTypedAnimation() is not null)
                    definitionHasOwner = true;
                else
                    unresolved.Add($"${(int)definitionPointer:X4} has no typed animation");
            }

            if (definitionTranslated)
                translated.Add(definitionPointer);
            if (definitionNeedsOwner && definitionHasOwner)
                owned.Add(definitionPointer);
        }

        AssertEqual(163, definitions.Length,
            "owner audit enumerates every named retail enemy definition");
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

        var unknownEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot unknown = unknownEnemies.SlotTable.Slots[0];
        unknown.Bind((EnemyDefinitionId)0x9000, unknown.Definition);
        unknown.Bind(unknown.DefinitionId, default(RoomEnemyDefinition));
        InvalidDataException unknownException = AssertThrows<InvalidDataException>(
            () => EnemyInstructionRunner.Process([unknown, null, null, Position16<RoomPixels>.Zero, SnesButton.None]),
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
            $"Enemy ${(int)definitionPointer:X4} initialization AI $",
            StringComparison.Ordinal) &&
        invalid.Message.EndsWith(" is not translated.", StringComparison.Ordinal);

    /// <summary>Whether binding <paramref name="definitionPointer"/> failed because its species AI is not ported.</summary>
    private static bool IsUnportedSpecies(InvalidDataException exception, EnemyDefinitionId definitionPointer) =>
        exception.Message.EndsWith($" holds {definitionPointer}, whose species AI is not ported.", StringComparison.Ordinal);

    /// <summary>Extracts distinct bank-$A0 enemy headers named in the pinned symbol inventory.</summary>
    private static EnemyDefinitionId[] ReadNamedRetailEnemyDefinitions()
    {
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        return File.ReadLines(symbolPath)
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(fields => fields.Length == 2 &&
                fields[0].StartsWith("0xa0", StringComparison.Ordinal) &&
                fields[1].StartsWith("kEnemyDef_", StringComparison.Ordinal))
            .Select(fields => EnemyDefinitionIds.FromHeaderPointer(ushort.Parse(
                fields[0].AsSpan(4),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture)))
            .Distinct()
            .Order()
            .ToArray();
    }

    /// <summary>Groups native population records from all retail room states by enemy definition.</summary>
    private static Dictionary<EnemyDefinitionId, List<RoomEnemyPopulationRecord>>
        ReadRetailEnemyPopulationRecords(ISnesAddressSpace rom)
    {
        CartridgeRoomState[] states = RoomStateDefinitions.All.ToArray();
        AssertEqual(RoomStateDefinitions.RetailStateCount, states.Length,
            "owner fixture enumerates every retail room state");
        var records = new Dictionary<EnemyDefinitionId, List<RoomEnemyPopulationRecord>>();

        foreach (ushort populationPointer in states
                     .Select(state => RoomEnemyPopulationDefinitions.PointerOf(state.EnemyPopulation))
                     .Distinct())
        {
            int cursor = RoomEnemyRomLayout.PopulationBank | populationPointer;
            for (int recordIndex = 0;
                 recordIndex < EnemySlotTable.MaximumEnemyCount;
                 recordIndex++, cursor += 16)
            {
                ushort definitionPointerWord = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor);
                if (definitionPointerWord == 0xffff)
                    break;
                EnemyDefinitionId definitionPointer = EnemyDefinitionIds.FromHeaderPointer(definitionPointerWord);

                var record = new RoomEnemyPopulationRecord(definitionPointer, new(CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 4))), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 6)), EnemyPropertyWord.FromNativeWord(CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 8))), EnemyExtraPropertyWord.FromNativeWord(CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 10))), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 12)), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 14)));
                if (!records.TryGetValue(
                    definitionPointer,
                    out List<RoomEnemyPopulationRecord>? definitionRecords))
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
