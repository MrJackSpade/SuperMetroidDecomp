using System.Reflection;
using System.Text.RegularExpressions;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShotCallbackDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var callbacks = new HashSet<int>();
        int headers = 0, lists = 0, boxes = 0;
        foreach (string line in File.ReadLines("upstream-disassembly/src/bank_A0.asm"))
        {
            Match match = Regex.Match(line, @"^EnemyHeaders_\w+:\s*;([0-9A-F]{6});");
            if (!match.Success) continue;
            int address = Convert.ToInt32(match.Groups[1].Value, 16);
            callbacks.Add((rom.ReadByte(address + 12) << 16) | Word(address + 50));
            headers++;
        }
        foreach (string file in Directory.EnumerateFiles("upstream-disassembly/src", "bank_*.asm"))
        {
            bool pending = false;
            foreach (string line in File.ReadLines(file))
            {
                // Count-prefixed standard hitboxes only. Kraid's separately named
                // HitboxDefinitionTable and eight KraidMouth rectangles are raw
                // geometry, not count-prefixed callback-bearing lists.
                if (Regex.IsMatch(line, @"^(?:UNUSED_)?Hitbox_(?!KraidMouth_)\w+:")) pending = true;
                Match match = Regex.Match(line, @";([0-9A-F]{6});");
                if (!pending || !match.Success) continue;
                pending = false;
                int address = Convert.ToInt32(match.Groups[1].Value, 16);
                int count = Word(address);
                AssertTrue(count < 100, "native hitbox inventory count sanity");
                lists++;
                for (int box = 0; box < count; box++)
                {
                    callbacks.Add((address & 0xff0000) | Word(address + 2 + box * 12 + 10));
                    boxes++;
                }
            }
            AssertTrue(!pending, "native hitbox label has an addressed definition");
        }
        AssertEqual(163, headers, "pinned enemy header inventory");
        AssertEqual(221, lists, "pinned named hitbox list inventory including unused lists");
        AssertEqual(309, boxes, "pinned hitbox entries");
        AssertEqual(80, callbacks.Count, "distinct header and hitbox shot callbacks");
        var expectedNoOps = callbacks.Where(address => (address & 65535) != 0 && rom.ReadByte(address) == 0x6b).ToHashSet();
        AssertEqual(12, expectedNoOps.Count, "native literal shot no-op count");
        var classify = typeof(RoomEnemySystem).GetMethod("IsLiteralNoOpEnemyAi", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<byte, ushort, bool>>();
        foreach (int address in callbacks)
            AssertEqual(expectedNoOps.Contains(address), classify((byte)(address >> 16), (ushort)address), "real shot classifier native parity");
        for (int bank = 0; bank <= 255; bank++)
        for (int pointer = 0; pointer <= 65535; pointer++)
            AssertEqual(expectedNoOps.Contains((bank << 16) | pointer), classify((byte)bank, (ushort)pointer), "bank-qualified shot identity");
        var shortcut = typeof(RoomEnemySystem).GetMethod("IsCanonicalMultiboxNoOpAi", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<ushort, bool>>();
        AssertTrue(!shortcut(0x94b5) && shortcut(0x804b) && shortcut(0x804c), "Kraid private RTL is not a canonical multibox shortcut");
        Console.WriteLine("Shot callback inventory: 163 headers, 221 lists, 309 hitboxes and 80 callbacks match; all bank/pointer pairs preserve twelve literal RTL identities independently of multibox header shortcuts.");
    }
}
