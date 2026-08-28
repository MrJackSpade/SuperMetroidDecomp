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
        var rinkaPopulations = new List<ushort>();
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
            if (definitionsInPopulation.Contains(0xd23f))
                rinkaPopulations.Add(population);
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
        Console.WriteLine(
            "Rinka populations: " +
            string.Join(", ", rinkaPopulations.Select(pointer => $"$A1:{pointer:X4}")));
        foreach (ushort population in rinkaPopulations)
        {
            Console.WriteLine($"  $A1:{population:X4}:");
            for (int cursor = population, recordIndex = 0;
                recordIndex < RoomEnemySystem.MaximumEnemyCount;
                cursor = unchecked((ushort)(cursor + 16)), recordIndex++)
            {
                ushort definition = ReadWord(bus, 0xa10000 | cursor);
                if (definition == 0xffff)
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
            if (rinkaPopulations.Contains(populationPointer))
                Console.WriteLine($"  Rinka state $8F:{statePointer:X4} -> $A1:{populationPointer:X4} {line[(line.IndexOf(' ') + 1)..]}");
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
