using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Full enemy-frame ordering after lethal-Ice and direct-freeze contacts.</summary>
internal static class IceThawAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "D06ECE784E8E26534709254AE384503266516EDACB6A34586D6FCBCE0D5AB8D7")
            throw new InvalidDataException("Use the accepted ice-thaw-417-v2 capture.");
        var bus = new TargetData(SuperMetroidAddressSpace.LoadRetailRom(rom));
        RoomEnemySystem enemies = null!;
        SamusState samus = null!;
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            int frame = int.Parse(row[2]);
            if (frame == 0)
            {
                bus.IceVulnerability = byte.Parse(row[0]);
                var projectiles = new SamusProjectileSystem();
                var shared = new SamusBombProjectileSystem();
                samus = new SamusState { XPosition = 128, YPosition = 128,
                    Pose = SamusPoseIds.FacingRightNormalPose,
                    EquippedBeams = 0x1002, PowerBombs = 2, SelectedHudItem = 3 };
                if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                    throw new InvalidDataException("Fixture must activate Ice Shield.");
                foreach (var particle in projectiles.Slots.Take(4))
                    particle.XPosition = particle.YPosition = 1024;
                projectiles.Slots[0].XPosition = projectiles.Slots[0].YPosition = 128;
                projectiles.RunProjectileInstructionHandler(bus, projectiles.Slots[0]);
                enemies = new RoomEnemySystem();
                enemies.Load(bus, 0xf000, 0, new SnesVram(), new SnesCgram(), () => 1);
                var target = enemies.Slots[0];
                target.EnemyDefinitionPointer = 0xf000;
                target.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3,
                    MainAiPointer = EnemyAiCodePointers.BankA0.NoOp,
                    HurtAiPointer = EnemyAiCodePointers.BankA0.NoOp,
                    ShotAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyShot,
                    VulnerabilityPointer = 0xf000 };
                target.XPosition = target.YPosition = 128;
                target.XRadius = target.YRadius = 16;
                target.SpritemapPointer = 0x8000;
                target.Health = 90;
                enemies.StepFrame(0, 0, false);
                target.FrameCounter = 0;
                enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus);
                samus.XPosition = samus.YPosition = 1024;
            }
            if (frame == int.Parse(row[1])) samus.EquippedBeams = 0x1000;
            // Native Samus and particles are outside this target, just as no projectile
            // collection is supplied to the managed frame after the initial hit.
            enemies.StepFrame(0, 0, false, samus: samus);
            var enemy = enemies.Slots[0];
            string actual = $"{enemy.Health:X4},{enemy.FrozenTimer:X4},{enemy.AiHandlerBits:X4}," +
                $"{enemy.FlashTimer:X4},{enemy.InvincibilityTimer:X4},{enemy.FrameCounter:X4}";
            if (actual != string.Join(',', row[3..]))
            {
                if (failures++ < 12)
                    Console.WriteLine($"ICE THAW {string.Join(',', row[..3])}: {actual} != {string.Join(',', row[3..])}");
            }
            cases++;
        }
        if (cases != 3360) throw new InvalidDataException("Incomplete Ice Shield thaw matrix.");
        Console.WriteLine($"Ice thaw: {cases} frames, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }

    private sealed class TargetData(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public byte IceVulnerability { get; set; }
        public byte ReadByte(int address) => address switch
        {
            0xa1f000 or 0xa1f001 => 0xff,
            0xb4f002 => IceVulnerability,
            >= 0xb4f000 and < 0xb4f020 => 2,
            _ => inner.ReadByte(address)
        };
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
