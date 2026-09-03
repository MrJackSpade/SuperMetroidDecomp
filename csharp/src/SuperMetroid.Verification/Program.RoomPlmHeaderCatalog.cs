using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Proves the named header catalog has no aliases and covers every header accepted by
    /// the sequential loader in all retail room populations for the pinned cartridge.
    /// </summary>
    static void VerifyRoomPlmHeaderCatalog()
    {
        ushort[] cataloguedHeaders = typeof(RoomPlmHeaders)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .ToArray();
        AssertEqual(cataloguedHeaders.Length, cataloguedHeaders.Distinct().Count(),
            "PLM header catalog has no duplicate native pointers");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        if (!File.Exists(romPath) || !File.Exists(symbolPath))
        {
            Console.WriteLine(
                "  PLM headers: uniqueness passes; retail-population coverage skipped (private inputs absent).");
            return;
        }

        HashSet<ushort> catalog = cataloguedHeaders.ToHashSet();
        ushort[] populationPointers = File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0x8f", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kRoomPlms_", StringComparison.Ordinal))
            .Select(ParseRoomPlmPopulationPointer)
            .Distinct()
            .ToArray();
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var retailHeaders = new List<ushort>();
        foreach (ushort populationPointer in populationPointers)
        {
            ushort cursor = populationPointer;
            bool terminated = false;
            for (int record = 0; record < 256; record++)
            {
                ushort header = ReadRoomPlmWord(bus, cursor);
                if (header == 0)
                {
                    terminated = true;
                    break;
                }
                retailHeaders.Add(header);
                cursor = unchecked((ushort)(cursor + 6));
            }
            AssertTrue(terminated,
                $"retail PLM population $8F:{populationPointer:X4} is terminated");
        }

        ushort[] supportedHeaders = retailHeaders
            .Where(RoomPlmSystem.IsSupportedRoomPopulationHeader)
            .Distinct()
            .ToArray();
        foreach (ushort header in supportedHeaders)
        {
            AssertTrue(catalog.Contains(header),
                $"supported retail PLM header $84:{header:X4} is catalogued");
        }
        AssertEqual(941, retailHeaders.Count, "retail PLM population record count");
        AssertEqual(70, retailHeaders.Distinct().Count(),
            "retail PLM population distinct-header count");
        AssertEqual(57, supportedHeaders.Length,
            "catalogued sequential-loader retail header coverage");

        Console.WriteLine(
            "  PLM headers: unique catalog and all 57 supported retail-population headers agree.");
    }

    private static ushort ParseRoomPlmPopulationPointer(string line)
    {
        int separator = line.IndexOf(' ');
        if (separator < 0 || !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture,
                out int address))
        {
            throw new InvalidDataException($"Malformed room-PLM symbol line: {line}");
        }
        return unchecked((ushort)address);
    }

    private static ushort ReadRoomPlmWord(SuperMetroidAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(
            bus.ReadByte((int)new SnesAddress(0x8f, pointer)) |
            (bus.ReadByte((int)new SnesAddress(
                0x8f,
                unchecked((ushort)(pointer + 1)))) << 8)));
}
