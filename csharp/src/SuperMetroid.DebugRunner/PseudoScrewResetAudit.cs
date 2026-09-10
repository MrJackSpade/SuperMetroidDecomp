using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Non-overlapping ordinary enemy dispatch: isolate timer reset from actual touch damage.</summary>
internal static class PseudoScrewResetAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "B7FB5034CA07E962EA0F3D1184D32A58D3E0EB65972160DCDD4050BFCB59A035")
            throw new InvalidDataException("Use the accepted ordinary reset native v1 capture.");
        var cartridge = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var bus = new EmptyPopulation(cartridge);
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            ushort contact = ushort.Parse(row[0]), initial = ushort.Parse(row[3]);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, 0xf000, 0, new SnesVram(), new SnesCgram(), () => 1);
            var enemy = enemies.Slots[0];
            enemy.EnemyDefinitionPointer = 0xf000;
            enemy.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3,
                MainAiPointer = 0x804c, TouchAiPointer = row[2] == "1" ? (ushort)0x804c : (ushort)0xa477 };
            enemy.XPosition = 192; enemy.YPosition = 128;
            enemy.XRadius = enemy.YRadius = 8;
            enemy.SpritemapPointer = row[1] == "1" ? (ushort)0x8000 : (ushort)0;
            // Build EnemyMain's interactive list without a Samus contact phase, then
            // seed exactly the cartridge collision-handler boundary timer and mode.
            enemies.StepFrame(0, 0, false);
            var samus = new SamusState { XPosition = 128, YPosition = 128,
                InvincibilityTimer = initial };
            samus.Kinematics.XRadius = 5; samus.Kinematics.YRadius = 12;
            samus.HorizontalSpeed.ContactDamageIndex = contact;
            bool hit = enemies.ResolveOrdinarySamusContact(samus, 0);
            ushort expected = ushort.Parse(row[4], NumberStyles.HexNumber);
            if (hit || samus.InvincibilityTimer != expected)
            {
                failures++;
                Console.WriteLine($"RESET {line}: actual={samus.InvincibilityTimer}, hit={hit}");
            }
            cases++;
        }
        if (cases != 24) throw new InvalidDataException("Incomplete ordinary reset matrix.");
        var empty = new RoomEnemySystem();
        empty.Load(bus, 0xf000, 0, new SnesVram(), new SnesCgram(), () => 1);
        var untouched = new SamusState { InvincibilityTimer = 9 };
        untouched.HorizontalSpeed.ContactDamageIndex = 4;
        if (empty.ResolveOrdinarySamusContact(untouched, 0) || untouched.InvincibilityTimer != 9)
            throw new InvalidDataException("An empty enemy list must not execute an enemy collision handler.");
        Console.WriteLine($"Pseudo Screw ordinary reset: {cases} cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }

    private sealed class EmptyPopulation(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is 0xa1f000 or 0xa1f001 ? (byte)0xff : inner.ReadByte(address);
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
