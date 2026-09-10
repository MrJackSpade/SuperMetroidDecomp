using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class ComboMotionAudit
{
    public static int Run(string rom, string trace, bool wave = false)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            (wave ? "7D20E4D77F95D6633377648EDA456DD4D71A4286FA966572B05B23DB0B617442" :
                "AFDBD584CC67F305238D796A91CBAD6BE6E088D0C40842C35323CA1EB79CDAB2"))
            throw new InvalidDataException("Use accepted Ice combo motion v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int frames = 0, failures = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..2])))
        {
            var seed = group.First();
            var samus = new SamusState { XPosition = 128, YPosition = 128, PowerBombs = 2,
                EquippedBeams = wave ? (ushort)0x1001 : (ushort)0x1002, SelectedHudItem = 3,
                Pose = seed[0] == "1" ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                throw new InvalidDataException("Ice allocation failed.");
            foreach (var row in group)
            {
                int frame = int.Parse(row[2]);
                samus.XPosition = (ushort)(128 + (wave && seed[0] == "1" ? -1 : 1) * (frame % 9));
                samus.YPosition = wave ? (ushort)(128 + frame / 7 % 9) : (ushort)128;
                // Explicit handler-entry sentinel, not simulated charge input: each
                // active pre-instruction must clear it, including on outward frames.
                typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.FlareCounter))!
                    .SetValue(projectiles, (ushort)77);
                shared.SetSharedCooldown(0);
                if (frame == int.Parse(seed[1])) projectiles.Slots[0].Direction |= 0x10;
                string sounds = "";
                for (int i = 3; i >= 0; i--)
                    if (projectiles.Slots[i].IsActive)
                    {
                        ushort sound = wave ? projectiles.StepWaveCombo(bus, samus, projectiles.Slots[i], shared) :
                            projectiles.StepIceCombo(bus, samus, projectiles.Slots[i], shared, 0, 0);
                        if (sound != 0) sounds += sound.ToString("X2");
                        if (wave && projectiles.Slots[i].IsActive)
                            projectiles.RunProjectileInstructionHandler(bus, projectiles.Slots[i]);
                    }
                string actual = $"{projectiles.ProjectileCounter:X4},{shared.CooldownTimer:X4},{projectiles.FlareCounter:X4},{sounds}," +
                    string.Join('/', projectiles.Slots.Take(4).Select(slot => !slot.IsActive ? "0" :
                        $"{slot.XPosition:X4}{slot.YPosition:X4}{(wave ? SamusComboRomData.Wave : slot.PreInstruction == SamusProjectilePreInstruction.IceCombo ? SamusComboRomData.Ice : SamusComboRomData.IceOutward):X4}" +
                        $"{slot.Variable:X4}{(ushort)slot.XVelocity:X4}{(ushort)slot.YVelocity:X4}{slot.TrailTimer:X4}" +
                        (wave ? $"{slot.XSubposition:X4}{slot.YSubposition:X4}" : "")));
                if (actual != string.Join(',', row[3..]))
                    if (failures++ < 8) Console.WriteLine($"COMBO {group.Key}/{frame}: {actual} != {string.Join(',', row[3..])}");
                frames++;
            }
        }
        if (frames != 3840) throw new InvalidDataException("Incomplete Ice matrix.");
        Console.WriteLine($"{(wave ? "Wave" : "Ice")} combo motion: {frames} frames, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
