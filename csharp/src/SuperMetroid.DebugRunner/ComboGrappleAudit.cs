using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static class ComboGrappleAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "8F43C1DE13732895F6044ACCF7A14AA3B1274252E37679CDC38B962A55339CDA")
            throw new InvalidDataException("Use the accepted combo-grapple v1 trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose,
                XPosition = 128, YPosition = 128, SelectedHudItem = 3, PowerBombs = 2,
                EquippedBeams = (ushort)(0x1000 | int.Parse(row[0])) };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                throw new InvalidDataException("Fixture combo activation failed.");
            samus.SelectedHudItem = 4;
            shared.SetSharedCooldown(ushort.Parse(row[1]));
            if (SamusGrappleHudInput.IsSelectedAndAdmitted(bus, samus) && (row[2] == "1" || row[3] == "1"))
                SamusGrappleMovement.BeginFiring(bus, samus);
            string actual = $"{(samus.Grapple.Phase == GrapplePhase.Firing ? 1 : 0)}," +
                $"{projectiles.ProjectileCounter:X4},{samus.PowerBombs:X4},{shared.CooldownTimer:X4}," +
                $"{samus.Grapple.AnchorX:X4},{samus.Grapple.AnchorY:X4}," +
                string.Concat(projectiles.Slots.Take(4).Select(slot => $"{slot.Type:X4}"));
            if (actual != string.Join(',', row[4..]))
                if (failures++ < 8) Console.WriteLine($"GRAPPLE {string.Join(',', row[..4])}: {actual} != {string.Join(',', row[4..])}");
            cases++;
        }
        if (cases != 32) throw new InvalidDataException("Incomplete combo grapple matrix.");
        foreach (SamusBeamFlags beam in new[] { SamusBeamFlags.Wave, SamusBeamFlags.Ice,
            SamusBeamFlags.Spazer, SamusBeamFlags.Plasma })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            runtime.Plms.Reset();
            var samus = runtime.Samus!;
            samus.EquippedBeams = (ushort)(SamusBeamFlags.Charge | beam);
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.GrappleBeam;
            samus.PowerBombs = samus.MaxPowerBombs = 2;
            samus.SelectedHudItem = 3;
            for (int frame = 0; frame < 121; frame++) runtime.StepFrame((ushort)SnesButton.X);
            runtime.StepFrame((ushort)SnesButton.Select);
            ushort[] types = runtime.Projectiles.Slots.Take(4).Select(slot => slot.Type).ToArray();
            ushort originX = samus.XPosition, originY = samus.YPosition;
            runtime.StepFrame((ushort)SnesButton.X);
            if (samus.Grapple.Phase != GrapplePhase.Firing || runtime.LastGrappleMovement?.Fired != true ||
                runtime.DebugGrappleItemSelected || runtime.Projectiles.ProjectileCounter != 4 ||
                samus.PowerBombs != 1 || samus.Grapple.AnchorX != originX + 2 ||
                samus.Grapple.AnchorY != originY - 4 ||
                !types.SequenceEqual(runtime.Projectiles.Slots.Take(4).Select(slot => slot.Type)))
                throw new InvalidDataException($"{beam}: normal Select/Shoot did not preserve native grapple/combo ownership.");
        }
        Console.WriteLine($"Combo grapple: {cases} native cases, {failures} mismatches; four real-input handoffs passed.");
        return failures == 0 ? 0 : 1;
    }
}
