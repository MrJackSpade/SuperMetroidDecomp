using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Audits every named retail enemy definition through every population record that
    /// uses it. This is deliberately broader than the family-specific execution tests:
    /// it catches translated actors whose initial instruction stream was never exercised
    /// by a scenario, which is how the three Space Pirate families escaped the first
    /// attempted removal of the generic cartridge fallback.
    /// </summary>
    private static void VerifyEnemyInstructionOwnerCoverage()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Ordinary-enemy mechanics owners: retail inventory skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        Dictionary<ushort, List<RoomEnemyPopulationRecord>> populations =
            ReadRetailEnemyPopulationRecords(rom);
        ushort[] definitions = ReadNamedRetailEnemyDefinitions();

        const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.NonPublic;
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "RunInitializationAi", flags)!;
        MethodInfo readMechanics = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyInstructionMechanicsWord", flags)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        FieldInfo vramField = typeof(RoomEnemySystem).GetField("_vram", flags)!;
        FieldInfo cgramField = typeof(RoomEnemySystem).GetField("_cgram", flags)!;
        FieldInfo nextRandomField = typeof(RoomEnemySystem).GetField("_nextRandom", flags)!;
        FieldInfo readRandomField = typeof(RoomEnemySystem).GetField(
            "_readRandomNumber", flags)!;

        var translated = new HashSet<ushort>();
        var owned = new HashSet<ushort>();
        var unresolved = new List<string>();

        foreach (ushort definitionPointer in definitions)
        {
            RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(
                rom, definitionPointer);
            IReadOnlyList<RoomEnemyPopulationRecord> records =
                populations.TryGetValue(definitionPointer, out List<RoomEnemyPopulationRecord>? found)
                    ? found
                    : [new RoomEnemyPopulationRecord(
                        definitionPointer, 128, 128, 0, 0, 0, 0, 0)];

            bool definitionTranslated = false;
            bool definitionNeedsOwner = false;
            bool definitionHasOwner = false;
            foreach (RoomEnemyPopulationRecord population in records)
            {
                var enemies = new RoomEnemySystem();
                busField.SetValue(enemies, rom);
                vramField.SetValue(enemies, new SnesVram());
                cgramField.SetValue(enemies, new SnesCgram());
                nextRandomField.SetValue(enemies, (Func<ushort>)(() => 0));
                readRandomField.SetValue(enemies, (Func<ushort>)(() => 0));

                RoomEnemySlot slot = enemies.Slots[0];
                slot.EnemyDefinitionPointer = definitionPointer;
                slot.Definition = definition;
                slot.XRadius = definition.XRadius;
                slot.YRadius = definition.YRadius;
                slot.Health = definition.Health;
                slot.Layer = definition.Layer;
                slot.XPosition = population.XPosition;
                slot.YPosition = population.YPosition;
                slot.CurrentInstruction = population.InitializationParameter;
                slot.Properties = population.Properties;
                slot.ExtraProperties = population.ExtraProperties;
                slot.Parameter1 = population.Parameter1;
                slot.Parameter2 = population.Parameter2;
                slot.InstructionTimer = 1;
                slot.AiBank = definition.Bank;

                bool translatedRecord = true;
                try
                {
                    initialize.Invoke(
                        enemies,
                        [slot, null, new SamusState(), (ushort)0, (ushort)0, (ushort)0]);
                }
                catch (TargetInvocationException exception)
                    when (IsUntranslatedEnemyInitializer(
                        exception.InnerException, definitionPointer))
                {
                    translatedRecord = false;
                }
                catch (TargetInvocationException)
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
                    busField, readMechanics, enemies, rom, slot, slot.CurrentInstruction))
                {
                    definitionHasOwner = true;
                }
                else
                {
                    unresolved.Add(
                        $"${definitionPointer:X4} -> ${definition.Bank:X2}:" +
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
        AssertTrue(unresolvedDefinitions.SetEquals(
            [0xd07f, 0xd0bf, 0xde3f, 0xe13f, 0xec7f]),
            "pending translated mechanics owners are exactly both Gunship actors, " +
            "Draygon, Ridley, and Mother Brain; actual=" +
            string.Join(", ", unresolved.Distinct()));

        var unknown = new RoomEnemySlot(0)
        {
            EnemyDefinitionPointer = 0x9000,
            Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 },
            CurrentInstruction = 0x9000,
        };
        var unknownSystem = new RoomEnemySystem();
        AssertTrue(!HasCompiledEnemyInstructionOwner(
                busField,
                readMechanics,
                unknownSystem,
                rom,
                unknown,
                unknown.CurrentInstruction),
            "unknown ordinary-enemy definition has no implicit cartridge owner");

        Console.WriteLine(
            $"  Ordinary-enemy mechanics owners: {definitions.Length} named definitions, " +
            $"{translated.Count} translated initializers, {owned.Count} compiled " +
            $"instruction-processing owners, and {unresolvedDefinitions.Count} " +
            "explicitly inventoried pending families.");
    }

    private static bool HasCompiledEnemyInstructionOwner(
        FieldInfo busField,
        MethodInfo readMechanics,
        RoomEnemySystem enemies,
        ISnesAddressSpace source,
        RoomEnemySlot slot,
        ushort address)
    {
        var probe = new EnemyInstructionOwnerReadProbe(source);
        busField.SetValue(enemies, probe);
        try
        {
            _ = readMechanics.Invoke(enemies, [slot, address]);
            return true;
        }
        catch (TargetInvocationException exception)
            when (exception.InnerException is EnemyInstructionOwnerFallbackReadException)
        {
            return false;
        }
        catch (TargetInvocationException)
        {
            // A family owner can reject an address that is presentation-only or lies
            // outside its authored program. The important boundary is that dispatch
            // reached that owner rather than the terminal unowned-definition branch.
            return true;
        }
        finally
        {
            busField.SetValue(enemies, source);
        }
    }

    private static bool IsUntranslatedEnemyInitializer(
        Exception? exception,
        ushort definitionPointer) =>
        exception is InvalidDataException invalid &&
        invalid.Message.StartsWith(
            $"Enemy ${definitionPointer:X4} initialization AI $",
            StringComparison.Ordinal) &&
        invalid.Message.EndsWith(" is not translated.", StringComparison.Ordinal);

    private static ushort[] ReadNamedRetailEnemyDefinitions()
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
            .Distinct()
            .Order()
            .ToArray();
    }

    private static Dictionary<ushort, List<RoomEnemyPopulationRecord>>
        ReadRetailEnemyPopulationRecords(ISnesAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        CartridgeRoomState[] states = (CartridgeRoomState[])typeof(RoomStateDefinitions)
            .GetField("definitions", flags)!
            .GetValue(null)!;
        var records = new Dictionary<ushort, List<RoomEnemyPopulationRecord>>();

        foreach (ushort populationPointer in states
                     .Select(state => state.EnemyPopulationPointer)
                     .Distinct())
        {
            int cursor = RoomEnemyRomLayout.PopulationBank | populationPointer;
            for (int recordIndex = 0;
                 recordIndex < RoomEnemySystem.MaximumEnemyCount;
                 recordIndex++, cursor += 16)
            {
                ushort definitionPointer = ReadEnemyOwnerAuditWord(rom, cursor);
                if (definitionPointer == 0xffff)
                    break;

                var record = new RoomEnemyPopulationRecord(
                    definitionPointer,
                    ReadEnemyOwnerAuditWord(rom, cursor + 2),
                    ReadEnemyOwnerAuditWord(rom, cursor + 4),
                    ReadEnemyOwnerAuditWord(rom, cursor + 6),
                    ReadEnemyOwnerAuditWord(rom, cursor + 8),
                    ReadEnemyOwnerAuditWord(rom, cursor + 10),
                    ReadEnemyOwnerAuditWord(rom, cursor + 12),
                    ReadEnemyOwnerAuditWord(rom, cursor + 14));
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

    private static ushort ReadEnemyOwnerAuditWord(
        ISnesAddressSpace bus,
        int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    private sealed class EnemyInstructionOwnerReadProbe(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            throw new EnemyInstructionOwnerFallbackReadException(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class EnemyInstructionOwnerFallbackReadException(int address) :
        Exception($"Ordinary-enemy mechanics fell back to ROM at ${address:X6}.");
}
