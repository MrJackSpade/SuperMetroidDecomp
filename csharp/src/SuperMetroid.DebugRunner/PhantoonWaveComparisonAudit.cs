using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Compare every scroll word with the bounded original-CPU probe, not a second C# formula.</summary>
internal static class PhantoonWaveComparisonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(trace))) !=
            "540325129811D501D37987C45B3F3216099CBF43A6AF5A0009C0C0F97C37504A")
            throw new InvalidDataException("Use the accepted original-CPU Phantoon wave capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int words = 0, cases = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(',').Select(int.Parse).ToArray())
            .GroupBy(row => (Mode: row[0], Amplitude: row[1], Frame: row[2], Phase: row[3])))
        {
            int half = (group.Key.Mode & 1) != 0 ? 64 : 32;
            var actual = new ushort[half * 2];
            PhantoonWaveTable.Build(bus, (ushort)group.Key.Mode, (ushort)group.Key.Phase,
                (ushort)group.Key.Amplitude, 0xffa8, actual);
            var rows = group.ToArray();
            if (rows.Length != actual.Length || rows.Select(r => r[4]).Distinct().Count() != actual.Length)
                throw new InvalidDataException("Native wave cycle is incomplete or contains duplicate indexes.");
            foreach (var row in rows)
            {
                if (actual[row[4]] != row[5])
                    throw new InvalidDataException($"Wave {group.Key}, row {row[4]}: managed {actual[row[4]]}, original CPU {row[5]}.");
                words++;
            }
            cases++;
        }
        if (cases != 480 || words != 46080) throw new InvalidDataException("Native wave corpus is incomplete.");
        Console.WriteLine($"Phantoon wave: {cases} original-CPU cycles, {words} exact scroll words.");
        return 0;
    }
}
