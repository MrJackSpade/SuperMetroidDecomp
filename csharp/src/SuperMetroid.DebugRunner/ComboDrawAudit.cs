using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Complete projectile/trail OAM bytes, not a particle-position proxy.</summary>
internal static class ComboDrawAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "C92A9E80D98A24BD0D665A9DC36E541059843B2E1B523BE84B79F9796E4F95B3")
            throw new InvalidDataException("Use the accepted combo-draw v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int frames = 0, failures = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).GroupBy(line =>
            string.Join(',', line.Split(',')[..2])))
        {
            string[] seed = group.Key.Split(',');
            int beam = int.Parse(seed[0]);
            ushort cameraX = seed[1] == "1" ? (ushort)80 : (ushort)0;
            ushort cameraY = seed[1] == "1" ? (ushort)64 : (ushort)0;
            var samus = new SamusState { XPosition = 128, YPosition = 128,
                Pose = SamusPoseIds.FacingRightNormalPose, EquippedBeams = (ushort)(0x1000 | beam),
                SelectedHudItem = 3, PowerBombs = 2 };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var oam = new OamBuffer();
            if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                throw new InvalidDataException("Fixture combo activation failed.");
            foreach (string line in group)
            {
                string[] row = line.Split(',');
                int frame = int.Parse(row[2]);
                samus.XPosition = (ushort)(128 + frame % 9);
                samus.YPosition = (ushort)(128 + frame / 7 % 9);
                if (frame == 20) samus.EquippedBeams = (ushort)(0x1000 | (beam == 8 ? 1 : beam * 2));
                for (int i = 3; i >= 0; i--)
                {
                    var slot = projectiles.Slots[i];
                    if (!slot.IsActive) continue;
                    switch (slot.PreInstruction)
                    {
                        case SamusProjectilePreInstruction.WaveCombo:
                            projectiles.StepWaveCombo(bus, samus, slot, shared); break;
                        case SamusProjectilePreInstruction.IceCombo:
                        case SamusProjectilePreInstruction.IceComboOutward:
                            projectiles.StepIceCombo(bus, samus, slot, shared, cameraX, cameraY); break;
                        case SamusProjectilePreInstruction.SpazerCombo:
                        case SamusProjectilePreInstruction.SpazerComboFalling:
                            projectiles.StepSpazerCombo(bus, samus, slot, shared, cameraY); break;
                        case SamusProjectilePreInstruction.PlasmaCombo:
                            projectiles.StepPlasmaCombo(bus, samus, slot, shared, cameraX, cameraY); break;
                        default: throw new InvalidDataException("Unexpected combo handler.");
                    }
                    if (slot.IsActive) projectiles.RunProjectileInstructionHandler(bus, slot);
                }
                oam.BeginFrame();
                projectiles.DrawLiveProjectiles(bus, oam, cameraX, cameraY, (ushort)frame);
                projectiles.HandleTrailsAndDraw(bus, oam, cameraX, cameraY, false);
                string actual = $"{projectiles.ProjectileCounter:X4}," +
                    string.Concat(projectiles.Slots.Take(4).Select(slot => $"{slot.Type:X4}")) +
                    $",{oam.NextByteOffset:X4},{Convert.ToHexString(oam.LowTable[..oam.NextByteOffset])},{Convert.ToHexString(oam.HighTable)}";
                if (actual != string.Join(',', row[3..]))
                    if (failures++ < 8) Console.WriteLine($"DRAW {group.Key}/{frame}: {actual} != {string.Join(',', row[3..])}");
                frames++;
            }
        }
        if (frames != 5120) throw new InvalidDataException("Incomplete draw matrix.");
        Console.WriteLine($"Combo draw: {frames} frames, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
