using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Read-only inventory of every enemy definition referenced by a named retail population.
/// The checked-in upstream symbol map supplies population boundaries only; every record and
/// every reported header field is read again from the user's untouched cartridge image.
/// </summary>
internal static class RetailEnemyCoverageAudit
{
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
                "Retail enemy coverage requires upstream-sm/assets/names.txt.",
                symbolPath);
        }

        ushort[] populations = File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0xa1", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kEnemyPopulation_", StringComparison.Ordinal))
            .Select(ParseBankA1Pointer)
            .Distinct()
            .Order()
            .ToArray();

        var occurrences = new Dictionary<ushort, (int Populations, int Records)>();

        // Keep focused population dumps beside the complete definition inventory. These
        // are not hard-coded fixture claims: the lists below are populated by scanning the
        // named retail population boundaries on every run. Adding the family currently
        // being translated makes the audit double as a reproducible room-discovery tool.
        var focusedDefinitions = new Dictionary<ushort, string>
        {
            [0xd23f] = "Rinka",
            [0xd27f] = "Rio",
            [0xd2bf] = "Norfair lava-jumping enemy",
            [0xd2ff] = "Norfair Rio",
            [0xd33f] = "Lower Norfair Rio",
            [0xd37f] = "Maridia Large Snail",
            [0xd3ff] = "GRipper",
            [0xd43f] = "Ripper II",
            [0xd4bf] = "Dragon",
            [0xd4ff] = "Growing shutter",
            [0xd53f] = "Shootable vertical shutter",
            [0xd57f] = "Shootable horizontal shutter",
            [0xd5bf] = "Destroyable vertical shutter",
            [0xd5ff] = "Kamer vertical platform",
            [0xdfbf] = "Boulder",
            [0xe27f] = "Zebetites",
            [0xe5bf] = "Etecoon",
            [0xe5ff] = "Dachora",
            [0xe63f] = "Evir",
            [0xe67f] = "Evir projectile",
            [0xe6bf] = "Morph-ball eye",
            [0xe77f] = "Wrecked Ship ghost",
            [0xe7bf] = "Yapping Maw",
            [0xea7f] = "Blue Brinstar face block",
            [0xeabf] = "Ki-Hunter body",
            [0xeaff] = "Ki-Hunter wings",
            [0xeb3f] = "Red Ki-Hunter body",
            [0xeb7f] = "Red Ki-Hunter wings",
            [0xebbf] = "Gold Ki-Hunter body",
            [0xebff] = "Gold Ki-Hunter wings",
            [0xeeff] = "Bomb Torizo",
            [0xef7f] = "Golden Torizo",
            [0xefff] = "Tourian entrance statue",
            [0xf07f] = "Shaktool",
            [0xf193] = "Brinstar Pipe Bug",
            [0xf1d3] = "strong Brinstar Pipe Bug",
            [0xf213] = "Norfair Pipe Bug",
            [0xf253] = "yellow Brinstar Pipe Bug",
        };
        var focusedPopulations = focusedDefinitions.Keys.ToDictionary(
            definition => definition,
            _ => new List<ushort>());
        foreach (ushort population in populations)
        {
            var definitionsInPopulation = new HashSet<ushort>();
            int cursor = population;
            for (int recordIndex = 0; recordIndex < RoomEnemySystem.MaximumEnemyCount; recordIndex++)
            {
                ushort definition = ReadWord(bus, 0xa10000 | cursor);
                if (definition == 0xffff)
                    break;
                if (definition < 0x8000)
                {
                    throw new InvalidDataException(
                        $"Named population $A1:{population:X4} contains invalid definition " +
                        $"${definition:X4} at record {recordIndex}.");
                }

                (int populationCount, int recordCount) = occurrences.GetValueOrDefault(definition);
                occurrences[definition] = (populationCount, recordCount + 1);
                definitionsInPopulation.Add(definition);
                cursor = unchecked((ushort)(cursor + 16));
            }

            foreach (ushort definition in definitionsInPopulation)
            {
                (int populationCount, int recordCount) = occurrences[definition];
                occurrences[definition] = (populationCount + 1, recordCount);
            }
            foreach (ushort definition in focusedDefinitions.Keys)
            {
                if (definitionsInPopulation.Contains(definition))
                    focusedPopulations[definition].Add(population);
            }
        }

        foreach ((ushort pointer, (int populationCount, int recordCount)) in occurrences.OrderBy(pair => pair.Key))
        {
            RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, pointer);
            Console.WriteLine(
                $"${pointer:X4} pop={populationCount,3} records={recordCount,4} " +
                $"init=${definition.Bank:X2}:{definition.InitializationAiPointer:X4} " +
                $"main=${definition.Bank:X2}:{definition.MainAiPointer:X4} " +
                $"touch=${definition.TouchAiPointer:X4} shot=${definition.ShotAiPointer:X4} " +
                $"hp={definition.Health,5} dmg={definition.Damage,4} name=${definition.NamePointer:X4}");
        }

        Console.WriteLine(
            $"Retail inventory: {occurrences.Count} definitions across " +
            $"{populations.Length} named enemy populations.");
        foreach ((ushort definition, string familyName) in focusedDefinitions)
        {
            List<ushort> familyPopulations = focusedPopulations[definition];
            Console.WriteLine(
                $"{familyName} populations: " +
                string.Join(", ", familyPopulations.Select(pointer => $"$A1:{pointer:X4}")));
            foreach (ushort population in familyPopulations)
            {
                Console.WriteLine($"  $A1:{population:X4}:");
                for (int cursor = population, recordIndex = 0;
                    recordIndex < RoomEnemySystem.MaximumEnemyCount;
                    cursor = unchecked((ushort)(cursor + 16)), recordIndex++)
                {
                    ushort recordDefinition = ReadWord(bus, 0xa10000 | cursor);
                    if (recordDefinition == 0xffff)
                        break;
                    ushort[] words = Enumerable.Range(0, 8)
                        .Select(index => ReadWord(bus, 0xa10000 | (cursor + index * 2)))
                        .ToArray();
                    Console.WriteLine("    " + string.Join(' ', words.Select(word => $"{word:X4}")));
                }
            }

            foreach (string line in File.ReadLines(symbolPath).Where(line =>
                line.StartsWith("0x8f", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kRoomState_", StringComparison.Ordinal)))
            {
                ushort statePointer = ParseFixedBankPointer(line, 0x8f0000);
                ushort populationPointer = ReadWord(bus, 0x8f0000 | (statePointer + 8));
                if (familyPopulations.Contains(populationPointer))
                {
                    Console.WriteLine(
                        $"  {familyName} state $8F:{statePointer:X4} -> " +
                        $"$A1:{populationPointer:X4} {line[(line.IndexOf(' ') + 1)..]}");
                }
            }
        }
        return 0;
    }

    private static ushort ParseBankA1Pointer(string line)
        => ParseFixedBankPointer(line, 0xa10000);

    private static ushort ParseFixedBankPointer(string line, int expectedBank)
    {
        int separator = line.IndexOf(' ');
        if (separator < 0 ||
            !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out int address) ||
            (address & 0xff0000) != expectedBank)
        {
            throw new InvalidDataException($"Malformed fixed-bank symbol-map line: {line}");
        }
        return unchecked((ushort)address);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
