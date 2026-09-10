using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>All allocation words against original FireSBA, without advancing particle pre-instructions.</summary>
internal static class ComboAllocationAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "6A50975CDAEA6A8465118CA7E27DC024BA7CB6A959DD4BDCEB26071921411CD8")
            throw new InvalidDataException("Use the accepted native combo activation v1 trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var samus = new SamusState { EquippedBeams = (ushort)(0x1000 | int.Parse(row[0])),
                PowerBombs = ushort.Parse(row[1]), SelectedHudItem = row[2] == "1" ? (ushort)3 : (ushort)0,
                AutoCancelHudItemIndex = 3, XPosition = 512, YPosition = 384,
                Pose = row[3] == "1" ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose };
            projectiles.Slots[0].PreInstruction = row[4] switch { "1" => SamusProjectilePreInstruction.IceCombo,
                "2" => SamusProjectilePreInstruction.PlasmaCombo, _ => SamusProjectilePreInstruction.None };
            bool carry = projectiles.TryActivateCombo(bus, samus, shared, out _);
            string actual = $"{samus.PowerBombs:X4},{samus.SelectedHudItem:X4},{samus.AutoCancelHudItemIndex:X4}," +
                $"{projectiles.ProjectileCounter:X4},{shared.CooldownTimer:X4},{projectiles.ComboState:X4},{(carry ? 1 : 0)}," +
                string.Join('/', projectiles.Slots.Take(4).Select(slot =>
                    $"{slot.Type:X4}{slot.Direction:X4}{Pointer(slot.PreInstruction):X4}{slot.XPosition:X4}{slot.YPosition:X4}" +
                    $"{(ushort)slot.XVelocity:X4}{(ushort)slot.YVelocity:X4}{slot.Variable:X4}{slot.TrailTimer:X4}" +
                    $"{slot.Damage:X4}{slot.XRadius:X4}{slot.YRadius:X4}{slot.InstructionPointer:X4}{slot.InstructionTimer:X4}"));
            if (actual != string.Join(',', row[5..]))
            {
                if (failures++ < 8) Console.WriteLine($"COMBO {string.Join(',', row[..5])}: {actual} != {string.Join(',', row[5..])}");
            }
            cases++;
        }
        if (cases != 432) throw new InvalidDataException("Incomplete combo allocation matrix.");
        Console.WriteLine($"Combo allocation: {cases} cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }

    private static ushort Pointer(SamusProjectilePreInstruction instruction) => instruction switch
    {
        SamusProjectilePreInstruction.None => 0,
        SamusProjectilePreInstruction.IceCombo => SamusComboRomData.Ice,
        SamusProjectilePreInstruction.IceComboOutward => SamusComboRomData.IceOutward,
        SamusProjectilePreInstruction.WaveCombo => SamusComboRomData.Wave,
        SamusProjectilePreInstruction.SpazerCombo => SamusComboRomData.Spazer,
        SamusProjectilePreInstruction.PlasmaCombo => SamusComboRomData.Plasma,
        _ => throw new InvalidDataException("Unexpected ordinary projectile in combo allocation.")
    };
}
