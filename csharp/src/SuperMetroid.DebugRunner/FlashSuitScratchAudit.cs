using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Native suit HDMA writes to the suspended Flash's aliased ammo counter.</summary>
internal static class FlashSuitScratchAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72" ||
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != "C3CF44F4E8D518AD8360CE194E4FF760A0D72F2C7752D82A33953613D40BA894")
            throw new InvalidDataException("Use the pinned ROM and native suit-entry trace.");
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 644 || rows.Any(row => row.Length != 8)) throw new InvalidDataException("Incomplete suit trace.");
        int mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]}"))
        {
            var first = group.First();
            var samus = new SamusState { Pose = first[0] == "1" ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose,
                XPosition = 256, YPosition = 400, Health = 49, MaxHealth = 99, Missiles = 10, SuperMissiles = 10, PowerBombs = 10 };
            samus.RefreshCollisionRadii(bus);
            if (!samus.CrystalFlash.TryBegin(bus, samus, 0x470, 0x40)) throw new InvalidDataException("Flash rejected.");
            bool gravity = first[1] == "1";
            samus.EquippedItems = samus.CollectedItems = (ushort)(gravity ? SamusEquipmentFlags.GravitySuit : SamusEquipmentFlags.VariaSuit);
            var suit = new SamusSuitPickupState();
            var cgram = new SnesCgram();
            suit.Begin(bus, samus, 0, 0, gravity ? SamusSuitPickupKind.Gravity : SamusSuitPickupKind.Varia);
            int step = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[2]) != step) throw new InvalidDataException("Reordered suit trace.");
                if (step++ != 0) suit.Step(bus, samus, cgram);
                string actual = $"{samus.CrystalFlash.AmmoDecrementTimer:X4},{suit.Substate:X4},{suit.LightBeamPosition:X4},{samus.SharedShineTimer:X4},{samus.CrystalFlash.SpecialPaletteType:X4}";
                string expected = string.Join(',', row[3..]);
                if (actual == expected) continue;
                mismatches++;
                if (!reported) Console.WriteLine($"Suit/Flash {group.Key} step {step - 1}: {actual} != {expected}");
                reported = true;
            }
        }
        Console.WriteLine($"Suit/Flash scratch: {rows.Length} checkpoints, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
