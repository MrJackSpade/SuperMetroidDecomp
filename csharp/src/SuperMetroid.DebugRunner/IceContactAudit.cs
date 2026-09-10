using System.Security.Cryptography;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Ice Shield damage, lethal freezing, Ice removal and particle consumption.</summary>
internal static class IceContactAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "5007128C0E926474328C3EABB2F9BE3B8E73D02DE877B383E627E91B4BB05CD3")
            throw new InvalidDataException("Use the accepted ice-contact-417-v1 capture.");
        var bus = new TargetData(SuperMetroidAddressSpace.LoadRetailRom(rom));
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            bus.IceVulnerability = byte.Parse(row[1]);
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var samus = new SamusState { XPosition = 128, YPosition = 128,
                Pose = SamusPoseIds.FacingRightNormalPose,
                EquippedBeams = 0x1002, PowerBombs = 2, SelectedHudItem = 3 };
            if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                throw new InvalidDataException("Fixture must activate Ice Shield.");
            samus.EquippedBeams = row[2] == "1" ? (ushort)0x1002 : (ushort)0x1000;
            samus.LiquidPhysics.RoomIdentity = new RoomIdentity((AreaId)byte.Parse(row[3]), 0);
            foreach (var particle in projectiles.Slots.Take(4))
                particle.XPosition = particle.YPosition = 1024;
            var slot = projectiles.Slots[0];
            slot.XPosition = slot.YPosition = 128;
            projectiles.RunProjectileInstructionHandler(bus, slot);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, 0xf000, 0, new SnesVram(), new SnesCgram(), () => 1);
            var enemy = enemies.Slots[0];
            enemy.EnemyDefinitionPointer = 0xf000;
            enemy.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3,
                MainAiPointer = EnemyAiCodePointers.BankA0.NoOp,
                ShotAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyShot,
                VulnerabilityPointer = 0xf000 };
            enemy.XPosition = enemy.YPosition = 128;
            enemy.XRadius = enemy.YRadius = 16;
            enemy.SpritemapPointer = 0x8000;
            enemy.Health = ushort.Parse(row[0]);
            // Build the production collision list before installing the boundary's
            // frozen clock, so fixture setup cannot decrement or thaw it prematurely.
            enemies.StepFrame(0, 0, false);
            enemy.Definition = enemy.Definition with { Bank = 0xa0 };
            enemy.FrozenTimer = row[4] == "1" ? (ushort)20 : (ushort)0;
            enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus);
            bool alive = enemy.EnemyDefinitionPointer != 0;
            var freezeSounds = enemies.SoundRequests.Where(request =>
                request.SoundEffect.Library == SoundEffectLibrary.Library3 &&
                request.SoundEffect.Value == 10).ToArray();
            if (freezeSounds.Length > 1 || freezeSounds.Any(request => request.MaximumQueued != 3))
                throw new InvalidDataException("Freeze must publish one max-three request at most.");
            string actual = $"{(alive ? 1 : 0)},{(alive ? enemy.Health : 0):X4}," +
                $"{(alive ? enemy.FrozenTimer : 0):X4},{(alive ? enemy.AiHandlerBits : 0):X4}," +
                $"{(alive ? enemy.InvincibilityTimer : 0):X4},{slot.Damage:X4},{slot.Direction:X4}," +
                $"{(freezeSounds.Length == 0 ? 0 : 10):X2}";
            projectiles.StepIceCombo(bus, samus, slot, shared, 0, 0);
            actual += $",{projectiles.ProjectileCounter:X4}";
            if (actual != string.Join(',', row[5..]))
            {
                if (failures++ < 12)
                    Console.WriteLine($"ICE CONTACT {string.Join(',', row[..5])}: {actual} != {string.Join(',', row[5..])}");
            }
            cases++;
        }
        if (cases != 96) throw new InvalidDataException("Incomplete Ice Shield contact matrix.");
        Console.WriteLine($"Ice contact: {cases} cases, {failures} mismatches.");
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
