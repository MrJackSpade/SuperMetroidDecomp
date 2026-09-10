using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Zero-charge bomb producer: first-slot exception, ammo and armed-PB ownership.</summary>
internal static class BombAdmissionAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "3D384C192B95B243BE8E02EE4AF5B07373F404439815711854E8F83B2886EC62")
            throw new InvalidDataException("Use the accepted bomb-admission v1 trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            var samus = new SamusState { XPosition = 128, YPosition = 128,
                SelectedHudItem = row[0] == "1" ? (ushort)3 : (ushort)0,
                PowerBombs = ushort.Parse(row[3]),
                EquippedItems = row[5] == "1" ? (ushort)SamusEquipmentFlags.Bombs : (ushort)0 };
            var bombs = new SamusBombProjectileSystem();
            ushort count = ushort.Parse(row[1]);
            bombs.SetSharedBombCounter(count);
            bombs.SetSharedCooldown(ushort.Parse(row[2]));
            for (int i = 0; i < count; i++) bombs.Slots[i].Type = 0x500;
            if (row[6] == "1") bombs.PowerBombExplosion.Arm();
            bombs.TryPlaceBomb(bus, samus, (ushort)SnesButton.X,
                row[4] == "1" ? (ushort)SnesButton.X : (ushort)0, out _);
            string actual = $"{bombs.BombCounter:X4},{bombs.CooldownTimer:X4}," +
                $"{samus.PowerBombs:X4},{samus.SelectedHudItem:X4},{(bombs.PowerBombExplosion.IsArmed ? 1 : 0)}," +
                string.Concat(bombs.Slots.Select(slot => $"{slot.Type:X4}"));
            if (actual != string.Join(',', row[7..]))
                if (failures++ < 12) Console.WriteLine($"BOMB {string.Join(',', row[..7])}: {actual} != {string.Join(',', row[7..])}");
            cases++;
        }
        if (cases != 512) throw new InvalidDataException("Incomplete bomb admission matrix.");
        Console.WriteLine($"Bomb admission: {cases} cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
