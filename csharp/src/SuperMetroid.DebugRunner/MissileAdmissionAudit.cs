using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Full missile producer boundary, including rejected-attempt side effects.</summary>
internal static class MissileAdmissionAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "E237C320018B963565FFC7802C220F028EB96C36DEA8B482E8D79131E39BEAC4")
            throw new InvalidDataException("Use the accepted missile-admission v1 trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose,
                XPosition = 128, YPosition = 128, SelectedHudItem = ushort.Parse(row[0]),
                Missiles = ushort.Parse(row[3]), SuperMissiles = ushort.Parse(row[3]) };
            samus.Kinematics.XRadius = 5; samus.Kinematics.YRadius = 16;
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            ushort count = ushort.Parse(row[1]);
            typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.ProjectileCounter))!
                .SetValue(projectiles, count);
            for (int i = 0; i < count; i++)
            {
                projectiles.Slots[i].Damage = 300;
                projectiles.Slots[i].Type = 0x9011;
            }
            shared.SetSharedCooldown(ushort.Parse(row[2]));
            projectiles.TryFireMissile(bus, samus, row[4] == "1" ? (ushort)SnesButton.X : (ushort)0, 0, shared);
            string actual = $"{projectiles.ProjectileCounter:X4},{shared.CooldownTimer:X4}," +
                $"{samus.Missiles:X4},{samus.SuperMissiles:X4}," +
                string.Concat(projectiles.Slots.Select(slot => $"{slot.Type:X4}"));
            if (actual != string.Join(',', row[5..]))
                if (failures++ < 12) Console.WriteLine($"ADMISSION {string.Join(',', row[..5])}: {actual} != {string.Join(',', row[5..])}");
            cases++;
        }
        if (cases != 96) throw new InvalidDataException("Incomplete admission matrix.");
        Console.WriteLine($"Missile admission: {cases} cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
