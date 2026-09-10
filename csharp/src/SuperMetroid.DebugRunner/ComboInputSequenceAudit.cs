using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Combined alpha dispatcher with scripted poses, not movement-physics emulation.</summary>
internal static class ComboInputSequenceAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "CC8511EA8A9BAD9E0CBD36616D8D3CBE82A98AEE8C533A906F45B28FA70F20FA")
            throw new InvalidDataException("Use the accepted combo-input v3 trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var level = new RoomLevelData(16, 32, new ushort[512], new byte[512], new ushort[512], new byte[8]);
        int frames = 0, failures = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).GroupBy(line => string.Join(',', line.Split(',')[..3])))
        {
            string[] seed = group.Key.Split(',');
            int script = int.Parse(seed[2]);
            var samus = new SamusState { XPosition = 128, YPosition = 128,
                EquippedBeams = (ushort)(0x1000 | int.Parse(seed[0])), PowerBombs = ushort.Parse(seed[1]),
                SelectedHudItem = 3, EquippedItems = (ushort)(SamusEquipmentFlags.Bombs | SamusEquipmentFlags.MorphBall) };
            samus.Kinematics.XRadius = 5; samus.Kinematics.YRadius = 16;
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.FlareCounter))!
                .SetValue(projectiles, (ushort)119);
            foreach (string line in group)
            {
                string[] row = line.Split(',');
                int frame = int.Parse(row[3]);
                samus.Pose = script == 1 && frame < 20 ? SamusPoseIds.SpinJumpRightPose :
                    script == 2 && frame < 20 ? SamusPoseIds.TurningRightToLeftPose : SamusPoseIds.FacingRightNormalPose;
                ushort held = script == 3 && frame == 0 ? (ushort)0 : (ushort)SnesButton.X;
                ushort pressed = (script == 3 ? frame == 1 : frame == 0) ? (ushort)SnesButton.X : (ushort)0;
                if (script == 4 && frame >= 2)
                {
                    samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
                    samus.SelectedHudItem = 0;
                    held = pressed = (frame & 1) == 0 ? (ushort)SnesButton.X : (ushort)0;
                }
                shared.StepFrame(bus, level, samus, held, pressed);
                projectiles.StepFrame(bus, level, samus, held, pressed, 0, 0, shared);
                samus.ProjectileFlareCounter = projectiles.FlareCounter;
                string actual = $"{projectiles.FlareCounter:X4},{projectiles.PreviousBeamChargeCounter:X4}," +
                    $"{samus.PowerBombs:X4},{samus.SelectedHudItem:X4},{projectiles.ProjectileCounter:X4},{shared.CooldownTimer:X4}," +
                    string.Concat(projectiles.Slots.Select(slot => $"{slot.Type:X4}")) +
                    $",{shared.BombCounter:X4}," + string.Concat(shared.Slots.Select(slot => $"{slot.Type:X4}"));
                if (actual != string.Join(',', row[4..]))
                    if (failures++ < 12) Console.WriteLine($"INPUT {group.Key}/{frame}: {actual} != {string.Join(',', row[4..])}");
                frames++;
            }
        }
        if (frames != 23680) throw new InvalidDataException("Incomplete combined input matrix.");
        Console.WriteLine($"Combo input sequence: {frames} frames, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
