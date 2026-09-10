using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Two-target Plasma penetration, invincibility admission and repeat hits.</summary>
internal static class PlasmaRepeatAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "5E449DA6070CD93AC6246391C5125A6D96B27EBFEBAD77BEE323CBE85D4D8193")
            throw new InvalidDataException("Use the accepted plasma-repeat-420-v1 capture.");
        var bus = new EmptyPopulation(SuperMetroidAddressSpace.LoadRetailRom(rom));
        RoomEnemySystem enemies = null!;
        SamusProjectileSystem projectiles = null!;
        SamusBombProjectileSystem shared = null!;
        SamusState samus = null!;
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            int pass = int.Parse(row[2]), selected = int.Parse(row[1]);
            if (pass == 0)
            {
                samus = new SamusState { XPosition = 128, YPosition = 128,
                    Pose = SamusPoseIds.FacingRightNormalPose,
                    EquippedBeams = 0x1008, PowerBombs = 2, SelectedHudItem = 3 };
                projectiles = new SamusProjectileSystem();
                shared = new SamusBombProjectileSystem();
                if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                    throw new InvalidDataException("Fixture must activate Plasma Shield.");
                foreach (var particle in projectiles.Slots.Take(4))
                    particle.XPosition = particle.YPosition = 1024;
                var ring = projectiles.Slots[selected];
                ring.XPosition = ring.YPosition = 128;
                projectiles.RunProjectileInstructionHandler(bus, ring);
                enemies = new RoomEnemySystem();
                enemies.Load(bus, 0xf000, 0, new SnesVram(), new SnesCgram(), () => 1);
                for (int i = 0; i < 2; i++)
                {
                    var target = enemies.Slots[i];
                    target.EnemyDefinitionPointer = 0xf000;
                    target.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3,
                        MainAiPointer = EnemyAiCodePointers.BankA0.NoOp,
                        ShotAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyShot };
                    target.XPosition = target.YPosition = 128;
                    target.XRadius = target.YRadius = 16;
                    target.SpritemapPointer = 0x8000;
                    target.Health = 10000;
                    target.Properties = i == 0 && row[0] == "1"
                        ? (ushort)EnemyProperties.BlocksPlasmaBeam : (ushort)0;
                }
                enemies.StepFrame(0, 0, false);
                foreach (var target in enemies.Slots.Take(2))
                    target.Definition = target.Definition with { Bank = 0xa0 };
            }
            if (pass >= 2)
                foreach (var target in enemies.Slots.Take(2))
                    target.InvincibilityTimer = pass == 2 ? (ushort)1 : (ushort)0;
            enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus);
            var particleToRetire = projectiles.Slots[selected];
            if (particleToRetire.IsActive && (particleToRetire.Direction & 0xf0) != 0)
                projectiles.StepPlasmaCombo(bus, samus, particleToRetire, shared, 0, 0);
            string actual = string.Join(',', enemies.Slots.Take(2).Select(target =>
                $"{target.Health:X4},{target.InvincibilityTimer:X4}")) +
                $",{projectiles.ProjectileCounter:X4}," +
                string.Concat(projectiles.Slots.Take(4).Select(particle => $"{particle.Type:X4}"));
            if (actual != string.Join(',', row[3..]))
            {
                failures++;
                Console.WriteLine($"PLASMA REPEAT {string.Join(',', row[..3])}: {actual} != {string.Join(',', row[3..])}");
            }
            cases++;
        }
        if (cases != 32) throw new InvalidDataException("Incomplete Plasma repeat matrix.");
        Console.WriteLine($"Plasma repeat: {cases} passes, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }

    private sealed class EmptyPopulation(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is 0xa1f000 or 0xa1f001 ? (byte)0xff : inner.ReadByte(address);
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
