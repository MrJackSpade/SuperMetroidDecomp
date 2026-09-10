using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Original-CPU storage admission, countdown, warning and palette comparison.</summary>
internal static class ShineStorageAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "EF2CE1E9112C7F0DAB9A1615766B83B3C9DEF85CCB6BFCF8B441E81A9501E638")
            throw new InvalidDataException("Use accepted shine-storage-465-v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int records = 0, differences = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..2])))
        {
            string[] seed = group.First();
            var shine = new SamusShinesparkState();
            var cgram = new SnesCgram();
            ushort items = seed[1] switch { "0" => 0, "1" => (ushort)SamusEquipmentFlags.VariaSuit,
                _ => (ushort)SamusEquipmentFlags.GravitySuit };
            shine.TryStoreFromSpeedBooster(ushort.Parse(seed[0], NumberStyles.HexNumber));
            int failures = 0;
            foreach (string[] row in group)
            {
                int frame = int.Parse(row[2]);
                if (frame != 0) shine.UpdatePalette(bus, cgram, items);
                int sound = shine.ConsumeStoredShineWarningSoundRequest() ? 12 : 0;
                string palette = frame != 0 && shine.PaletteType == 1
                    ? string.Concat(cgram.Colors.Slice(SamusPaletteRomData.Common.SamusObjPaletteStart, 16)
                        .ToArray().Select(color => $"{color:X4}")) : "";
                string actual = $"{shine.ShineTimer:X4},{shine.PaletteType:X4},{shine.PaletteFrameOffset:X4},{sound:X4},{palette}";
                if (actual != string.Join(',', row[3..])) failures++;
                records++;
            }
            differences += failures;
            Console.WriteLine($"Storage {group.Key}: {failures} mismatches.");
        }
        if (records != 4392) throw new InvalidDataException("Incomplete storage matrix.");
        Console.WriteLine($"Stored shine: {records} records, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}
