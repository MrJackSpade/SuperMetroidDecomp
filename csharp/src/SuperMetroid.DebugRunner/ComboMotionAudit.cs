using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class ComboMotionAudit
{
    public static int Run(string rom, string trace, SamusBeamFlags family = SamusBeamFlags.Ice,
        bool wavePatterns = false)
    {
        if (wavePatterns && family != SamusBeamFlags.Wave)
            throw new ArgumentException("Pattern matrix requires Wave Shield.", nameof(family));
        bool wave = family == SamusBeamFlags.Wave, plasma = family == SamusBeamFlags.Plasma;
        bool spazer = family == SamusBeamFlags.Spazer;
        bool animated = family != SamusBeamFlags.Ice;
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            (wavePatterns ? "89890BAE1F685B2D26471915F1A5D99F21B41E3B446244E53366F7C1D482B805" :
                spazer ? "73763E826E1578999042745F4312EE9F4A909778A00E8361FE4E276EAC14403E" :
                plasma ? "2F053D2B633D5B4A534B868D9FA7DE78AFCF9EC1D4ECC4CFC6809D2B41701EC6" :
                wave ? "7D20E4D77F95D6633377648EDA456DD4D71A4286FA966572B05B23DB0B617442" :
                "AFDBD584CC67F305238D796A91CBAD6BE6E088D0C40842C35323CA1EB79CDAB2"))
            throw new InvalidDataException("Use accepted Ice combo motion v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int frames = 0, failures = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..2])))
        {
            var seed = group.First();
            var samus = new SamusState { XPosition = 128, YPosition = 128, PowerBombs = 2,
                EquippedBeams = (ushort)(SamusBeamFlags.Charge | family), SelectedHudItem = 3,
                Pose = !wavePatterns && seed[0] == "1" ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                throw new InvalidDataException("Ice allocation failed.");
            foreach (var row in group)
            {
                int frame = int.Parse(row[2]);
                samus.XPosition = (ushort)(128 + (animated && seed[0] == "1" ? -1 : 1) * (frame % 9));
                samus.YPosition = animated ? (ushort)(128 + (spazer && seed[0] == "1" ? 64 : 0) + frame / 7 % 9) : (ushort)128;
                if (wavePatterns)
                {
                    // Authored target trajectories, supplied identically to both engines;
                    // the cartridge, not this script, computes every particle coordinate.
                    int phase = frame % 120, vertical = frame % 80;
                    bool stationary = seed[0] == "0";
                    samus.XPosition = (ushort)(128 + (stationary ? 0 : phase < 60 ? phase : 120 - phase));
                    samus.YPosition = (ushort)(128 + (stationary ? 0 : (vertical < 40 ? vertical : 80 - vertical) / 4));
                    samus.Pose = seed[0] == "2" && phase >= 60
                        ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
                }
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
                        ushort sound = spazer ? projectiles.StepSpazerCombo(bus, samus, projectiles.Slots[i], shared, 0) :
                            plasma ? (ushort)0 : wave ? projectiles.StepWaveCombo(bus, samus, projectiles.Slots[i], shared) :
                            projectiles.StepIceCombo(bus, samus, projectiles.Slots[i], shared, 0, 0);
                        if (plasma) projectiles.StepPlasmaCombo(bus, samus, projectiles.Slots[i], shared, 0, 0);
                        if (sound != 0) sounds += sound.ToString("X2");
                        if (animated && projectiles.Slots[i].IsActive)
                            projectiles.RunProjectileInstructionHandler(bus, projectiles.Slots[i]);
                    }
                string actual = $"{projectiles.ProjectileCounter:X4},{shared.CooldownTimer:X4},{projectiles.FlareCounter:X4},{sounds}," +
                    string.Join('/', projectiles.Slots.Take(4).Select(slot => !slot.IsActive ? "0" :
                        $"{slot.XPosition:X4}{slot.YPosition:X4}{NativePointer(slot.PreInstruction):X4}" +
                        $"{slot.Variable:X4}{(ushort)slot.XVelocity:X4}{(ushort)slot.YVelocity:X4}{slot.TrailTimer:X4}" +
                        (animated ? $"{slot.XSubposition:X4}{slot.YSubposition:X4}" : "") +
                        (spazer ? $"{slot.AuxiliaryPhase:X4}{slot.Type:X4}{slot.InstructionPointer:X4}{slot.Damage:X4}" : "")));
                if (actual != string.Join(',', row[3..]))
                    if (failures++ < 8) Console.WriteLine($"COMBO {group.Key}/{frame}: {actual} != {string.Join(',', row[3..])}");
                if (plasma && seed[1] == "-1")
                {
                    if (frame is 37 or 74 && projectiles.Slots.Take(4).Any(slot => !slot.IsActive ||
                        slot.XVelocity != (frame == 37 ? 192 : 44) || slot.YVelocity != (frame == 37 ? 1 : 2)))
                        throw new InvalidDataException("Plasma expansion/contraction boundary differs from native.");
                    if (frame == 199 && projectiles.ProjectileCounter != 0)
                        throw new InvalidDataException("Plasma outward rings failed to leave the viewport.");
                }
                if (wavePatterns)
                {
                    int expectedCount = frame >= 599 ? 0 : seed[1] == "10" && frame >= 10 ? 3 : 4;
                    if (projectiles.ProjectileCounter != expectedCount ||
                        projectiles.Slots.Take(4).Any(slot => slot.IsActive && slot.Damage != 300))
                        throw new InvalidDataException("Wave particle damage or exact hit/expiry frame differs.");
                }
                frames++;
            }
        }
        if (frames != (wavePatterns ? 5760 : plasma || spazer ? 1200 : 3840)) throw new InvalidDataException("Incomplete combo matrix.");
        Console.WriteLine($"{family} combo motion: {frames} frames, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }

    private static ushort NativePointer(SamusProjectilePreInstruction instruction) => instruction switch
    {
        SamusProjectilePreInstruction.IceCombo => SamusComboRomData.Ice,
        SamusProjectilePreInstruction.IceComboOutward => SamusComboRomData.IceOutward,
        SamusProjectilePreInstruction.WaveCombo => SamusComboRomData.Wave,
        SamusProjectilePreInstruction.PlasmaCombo => SamusComboRomData.Plasma,
        SamusProjectilePreInstruction.SpazerCombo => SamusComboRomData.Spazer,
        SamusProjectilePreInstruction.SpazerComboFalling => SamusComboRomData.SpazerFalling,
        _ => throw new InvalidDataException("Unexpected live combo handler.")
    };
}
