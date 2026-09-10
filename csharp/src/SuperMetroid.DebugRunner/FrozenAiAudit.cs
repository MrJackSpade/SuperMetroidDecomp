using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Common frozen callback boundary, including Ice removal and timer expiration.</summary>
internal static class FrozenAiAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "CF8B8D22622DBD7463A79CAC01F5EFEE3ABD6E1B9B42D0C9EF6EB514A494376F")
            throw new InvalidDataException("Use the accepted frozen-ai-417-v1 capture.");
        var bus = new EmptyPopulation(SuperMetroidAddressSpace.LoadRetailRom(rom));
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            var samus = new SamusState { XPosition = 128, YPosition = 128,
                EquippedBeams = row[0] == "1" ? (ushort)SamusBeamFlags.Ice : (ushort)0 };
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, 0xf000, 0, new SnesVram(), new SnesCgram(), () => 1);
            var enemy = enemies.Slots[0];
            enemy.EnemyDefinitionPointer = 0xf000;
            enemy.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3,
                MainAiPointer = EnemyAiCodePointers.BankA0.NoOp };
            enemy.XPosition = enemy.YPosition = 128;
            enemy.XRadius = enemy.YRadius = 16;
            enemy.SpritemapPointer = 0x8000;
            enemy.Health = 100;
            enemy.FrozenTimer = ushort.Parse(row[1]);
            enemy.AiHandlerBits = ushort.Parse(row[2]);
            enemy.FlashTimer = 18;
            // The real frame selects common frozen AI. Its zeroed flash clock means
            // subsequent frame housekeeping cannot alter these three oracle fields.
            enemies.StepFrame(0, 0, false, samus: samus);
            string actual = $"{enemy.FrozenTimer:X4},{enemy.AiHandlerBits:X4},{enemy.FlashTimer:X4}";
            if (actual != string.Join(',', row[3..]))
            {
                failures++;
                Console.WriteLine($"FROZEN {string.Join(',', row[..3])}: {actual} != {string.Join(',', row[3..])}");
            }
            cases++;
        }
        if (cases != 16) throw new InvalidDataException("Incomplete frozen AI matrix.");
        Console.WriteLine($"Frozen AI: {cases} cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }

    private sealed class EmptyPopulation(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is 0xa1f000 or 0xa1f001 ? (byte)0xff : inner.ReadByte(address);
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
