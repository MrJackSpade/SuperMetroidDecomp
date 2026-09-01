using System.Globalization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Exhaustive private-ROM inventory of every named bank-$8F room PLM population. It proves
/// the sequential loader's support classifier accounts for every retail record and reports
/// untranslated headers with exact population coordinates rather than silently skipping.
/// </summary>
internal static class RetailPlmPopulationAudit
{
    private const int ExpectedRecordCount = 941;
    private const int ExpectedDistinctHeaderCount = 70;
    private const int ExpectedSupportedRecordCount = 882;
    private const int ExpectedSupportedHeaderCount = 54;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string symbolPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "upstream-sm",
            "assets",
            "names.txt");
        if (!File.Exists(symbolPath))
            throw new FileNotFoundException("Retail PLM audit requires names.txt.", symbolPath);

        ushort[] populations = File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0x8f", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kRoomPlms_", StringComparison.Ordinal))
            .Select(ParseBank8fPointer)
            .Distinct()
            .Order()
            .ToArray();
        var records = new List<RetailPlmRecord>();
        foreach (ushort population in populations)
        {
            ushort cursor = population;
            bool terminated = false;
            for (int recordIndex = 0; recordIndex < 256; recordIndex++)
            {
                ushort header = ReadWord(bus, 0x8f0000 | cursor);
                if (header == 0)
                {
                    terminated = true;
                    break;
                }
                records.Add(new RetailPlmRecord(
                    population,
                    recordIndex,
                    cursor,
                    header,
                    bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 2))),
                    bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 3))),
                    ReadWord(bus, 0x8f0000 | unchecked((ushort)(cursor + 4))),
                    RoomPlmSystem.IsSupportedRoomPopulationHeader(header)));
                cursor = unchecked((ushort)(cursor + 6));
            }
            if (!terminated)
            {
                throw new InvalidDataException(
                    $"Retail PLM population $8F:{population:X4} has no zero terminator.");
            }
        }

        int distinctHeaders = records.Select(record => record.Header).Distinct().Count();
        if (records.Count != ExpectedRecordCount ||
            distinctHeaders != ExpectedDistinctHeaderCount)
        {
            throw new InvalidDataException(
                $"Retail PLM inventory found {records.Count} records/{distinctHeaders} headers; " +
                $"expected {ExpectedRecordCount}/{ExpectedDistinctHeaderCount}.");
        }

        RetailPlmRecord[] unsupported = records.Where(record => !record.Supported).ToArray();
        int supportedRecords = records.Count - unsupported.Length;
        int supportedHeaders = records
            .Where(record => record.Supported)
            .Select(record => record.Header)
            .Distinct()
            .Count();
        if (supportedRecords != ExpectedSupportedRecordCount ||
            supportedHeaders != ExpectedSupportedHeaderCount)
        {
            throw new InvalidDataException(
                $"Sequential PLM dispatcher covers {supportedRecords} records/" +
                $"{supportedHeaders} headers; expected {ExpectedSupportedRecordCount}/" +
                $"{ExpectedSupportedHeaderCount} for the pinned retail revision.");
        }
        foreach (IGrouping<ushort, RetailPlmRecord> group in unsupported
                     .GroupBy(record => record.Header)
                     .OrderBy(group => group.Key))
        {
            RetailPlmRecord first = group.First();
            Console.WriteLine(
                $"untranslated $84:{group.Key:X4} x{group.Count(),2}; first " +
                $"$8F:{first.Population:X4} record {first.RecordIndex} " +
                $"({first.BlockX},{first.BlockY}) arg ${first.Argument:X4}");
        }
        Console.WriteLine(
            $"Retail PLM inventory: {populations.Length} populations, {records.Count} records, " +
            $"{distinctHeaders} headers; {supportedRecords} records/{supportedHeaders} headers supported, " +
            $"{unsupported.Length} fail loudly with population context.");
        return 0;
    }

    private static ushort ParseBank8fPointer(string line)
    {
        int separator = line.IndexOf(' ');
        if (separator < 0 || !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture,
                out int address))
        {
            throw new InvalidDataException($"Malformed bank-$8F symbol line: {line}");
        }
        return unchecked((ushort)address);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct RetailPlmRecord(
        ushort Population,
        int RecordIndex,
        ushort RecordPointer,
        ushort Header,
        byte BlockX,
        byte BlockY,
        ushort Argument,
        bool Supported);
}
