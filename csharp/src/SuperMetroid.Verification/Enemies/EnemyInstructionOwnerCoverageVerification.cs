using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

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

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Dictionary<EnemyDefinitionId, List<RoomEnemyPopulationRecord>> populations =
            ReadRetailEnemyPopulationRecords(rom);
        EnemyDefinitionId[] definitions = ReadNamedRetailEnemyDefinitions();

        const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.NonPublic;

        var translated = new HashSet<EnemyDefinitionId>();
        var owned = new HashSet<EnemyDefinitionId>();
        var unresolved = new List<string>();

        foreach (EnemyDefinitionId definitionPointer in definitions)
        {
            RoomEnemyDefinition definition = SuperMetroid.AssetExtraction.RoomEnemyDefinitionImporter.Load(
                rom, definitionPointer);
            IReadOnlyList<RoomEnemyPopulationRecord> records =
                populations.TryGetValue(definitionPointer, out List<RoomEnemyPopulationRecord>? found)
                    ? found
                    : [new RoomEnemyPopulationRecord(definitionPointer, new(128, 128), 0, 0, 0, 0, 0)];

            bool definitionTranslated = false;
            bool definitionNeedsOwner = false;
            bool definitionHasOwner = false;
            foreach (RoomEnemyPopulationRecord population in records)
            {
                var enemies = new RoomEnemySystem();
                enemies.BindFixtureBus(rom);
                enemies.Presentation._vram = new SnesVram();
                enemies.Presentation._cgram = new SnesCgram();
                enemies.VerificationServices().NextRandom = ((Func<ushort>)(() => 0));
                enemies.VerificationServices().ReadRandom = ((Func<ushort>)(() => 0));

                RoomEnemySlot slot = enemies.SlotTable.Slots[0];
                slot.EnemyDefinitionPointer = definitionPointer;
                slot.Definition = definition;
                slot.Radius = new(definition.Radius.X, definition.Radius.Y);
                slot.Health = definition.Health;
                slot.Layer = definition.Layer;
                slot.Position = slot.Position.WithWhole(new(population.Position.X, population.Position.Y));
                slot.CurrentInstruction = population.InitializationParameter;
                slot.Properties = population.Properties;
                slot.ExtraProperties = population.ExtraProperties;
                slot.Parameter1 = population.Parameter1;
                slot.Parameter2 = population.Parameter2;
                slot.InstructionTimer = NativeCountdown.Start(1);
                slot.AiBank = definition.Bank;

                bool translatedRecord = true;
                try
                {
                    enemies.RunInitializationAi(slot, null, new SamusState(), (ushort)0, (ushort)0);
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
                bool processesInstructions =
                    (slot.Properties & (ushort)EnemyProperties.ProcessInstructions) != 0;
                if (!processesInstructions || slot.CurrentInstruction == 0)
                    continue;

                definitionNeedsOwner = true;
                if (HasCompiledEnemyInstructionOwner(
                    enemies, rom, slot, slot.CurrentInstruction))
                {
                    definitionHasOwner = true;
                }
                else
                {
                    unresolved.Add(
                        $"${(int)definitionPointer:X4} -> ${definition.Bank:X2}:" +
                        $"{slot.CurrentInstruction:X4}");
                }
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
            "every translated ordinary-enemy instruction owner is compiled; actual=" +
            string.Join(", ", unresolved.Distinct()));

        var unknown = new RoomEnemySlot(0)
        {
            EnemyDefinitionPointer = (EnemyDefinitionId)0x9000,
            Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 },
            CurrentInstruction = 0x9000,
        };
        InvalidDataException unknownException = AssertThrows<InvalidDataException>(
            () => EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(unknown, unknown.CurrentInstruction),
            "unknown ordinary-enemy definition fails instead of reading cartridge mechanics");
        AssertTrue(
            unknownException.Message.Contains("has no compiled owner", StringComparison.Ordinal),
            "unknown ordinary-enemy failure identifies the missing compiled owner");

        Console.WriteLine(
            $"  Ordinary-enemy mechanics owners: {definitions.Length} named definitions, " +
            $"{translated.Count} translated initializers, {owned.Count} compiled " +
            $"instruction-processing owners, and {unresolvedDefinitions.Count} " +
            "explicitly inventoried pending families.");
    }

    /// <summary>Distinguishes a compiled family dispatch from the terminal cartridge fallback read.</summary>
    private static bool HasCompiledEnemyInstructionOwner(
        RoomEnemySystem enemies,
        ISnesAddressSpace source,
        RoomEnemySlot slot,
        ushort address)
    {
        var probe = NewEnemyInstructionOwnerReadProbe(source);
        enemies.BindFixtureBus(probe);
        try
        {
            _ = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(slot, address);
            return true;
        }
        catch (EnemyInstructionOwnerFallbackReadException)
        {
            return false;
        }
        catch (Exception)
        {
            // A family owner can reject an address that is presentation-only or lies
            // outside its authored program. The important boundary is that dispatch
            // reached that owner rather than the terminal unowned-definition branch.
            return true;
        }
        finally
        {
            enemies.BindFixtureBus(source);
        }
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
                     .Select(state => state.EnemyPopulationPointer)
                     .Distinct())
        {
            int cursor = RoomEnemyRomLayout.PopulationBank | populationPointer;
            for (int recordIndex = 0;
                 recordIndex < RoomEnemySystem.MaximumEnemyCount;
                 recordIndex++, cursor += 16)
            {
                ushort definitionPointerWord = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor);
                if (definitionPointerWord == 0xffff)
                    break;
                EnemyDefinitionId definitionPointer = EnemyDefinitionIds.FromHeaderPointer(definitionPointerWord);

                var record = new RoomEnemyPopulationRecord(definitionPointer, new(CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 4))), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 6)), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 8)), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 10)), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 12)), CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 14)));
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

    /// <summary>Turns any cartridge mechanics read into a recognizable unowned-definition failure.</summary>
    private static CartridgeReadGuard NewEnemyInstructionOwnerReadProbe(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(_ => true,
            address => new EnemyInstructionOwnerFallbackReadException(address));

    /// <summary>Signals that an ordinary enemy's instruction dispatch reached the ROM fallback.</summary>
    internal sealed class EnemyInstructionOwnerFallbackReadException(int address) :
        Exception($"Ordinary-enemy mechanics fell back to ROM at ${address:X6}.");

    /// <summary>Runs the retail-wide audit of translated enemy instruction ownership.</summary>
    [VerificationCommand("--enemy-instruction-owner-coverage")]
    internal static int EnemyInstructionOwnerCoverageCommand()
    {
        Suite(nameof(VerifyEnemyInstructionOwnerCoverage), () => VerifyEnemyInstructionOwnerCoverage());
        return 0;
    }
}
