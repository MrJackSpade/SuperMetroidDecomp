using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Original collision/shot CPU boundary versus the real managed enemy shot pass.</summary>
internal static class ComboContactAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "FC99218EA93F166D7FA3274A49777CDB8782D335DDB6AE48E308A70F30492AD1")
            throw new InvalidDataException("Use the accepted combo-contact v2 capture.");
        var bus = new EmptyPopulation(SuperMetroidAddressSpace.LoadRetailRom(rom));
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var samus = new SamusState { XPosition = 128, YPosition = 128,
                Pose = SamusPoseIds.FacingRightNormalPose,
                EquippedBeams = (ushort)(0x1000 | int.Parse(row[0])), PowerBombs = 2,
                SelectedHudItem = 3 };
            if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                throw new InvalidDataException("Fixture must activate its combo.");
            foreach (var particle in projectiles.Slots.Take(4))
                particle.XPosition = particle.YPosition = 1024;
            var slot = projectiles.Slots[int.Parse(row[3])];
            slot.XPosition = row[2] == "1" ? (ushort)128 : (ushort)192;
            slot.YPosition = 128;
            projectiles.RunProjectileInstructionHandler(bus, slot);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, 0xf000, 0, new SnesVram(), new SnesCgram(), () => 1);
            var enemy = enemies.Slots[0];
            enemy.EnemyDefinitionPointer = 0xf000;
            enemy.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3,
                MainAiPointer = EnemyAiCodePointers.BankA0.NoOp,
                ShotAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyShot };
            enemy.XPosition = enemy.YPosition = 128;
            enemy.XRadius = enemy.YRadius = 16;
            enemy.SpritemapPointer = 0x8000;
            enemy.Health = 10000;
            enemy.Properties = row[1] == "1" ? (ushort)EnemyProperties.BlocksPlasmaBeam : (ushort)0;
            enemies.StepFrame(0, 0, false);
            enemy.Definition = enemy.Definition with { Bank = 0xa0 };
            enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus);
            ushort pre = slot.PreInstruction switch
            {
                SamusProjectilePreInstruction.WaveCombo => SamusComboRomData.Wave,
                SamusProjectilePreInstruction.IceCombo => SamusComboRomData.Ice,
                SamusProjectilePreInstruction.SpazerCombo => SamusComboRomData.Spazer,
                SamusProjectilePreInstruction.PlasmaCombo => SamusComboRomData.Plasma,
                _ => 0
            };
            string actual = $"{enemy.Health:X4},{enemy.FlashTimer:X4},{enemy.InvincibilityTimer:X4}," +
                $"{slot.Type:X4},{slot.Direction:X4},{pre:X4},{slot.XPosition:X4},{slot.YPosition:X4},{slot.Damage:X4}";
            switch (slot.PreInstruction)
            {
                case SamusProjectilePreInstruction.WaveCombo:
                    projectiles.StepWaveCombo(bus, samus, slot, shared); break;
                case SamusProjectilePreInstruction.IceCombo:
                    projectiles.StepIceCombo(bus, samus, slot, shared, 0, 0); break;
                case SamusProjectilePreInstruction.SpazerCombo:
                    projectiles.StepSpazerCombo(bus, samus, slot, shared, 0); break;
                case SamusProjectilePreInstruction.PlasmaCombo:
                    projectiles.StepPlasmaCombo(bus, samus, slot, shared, 0, 0); break;
            }
            actual += $",{projectiles.ProjectileCounter:X4}," +
                string.Concat(projectiles.Slots.Take(4).Select(particle => $"{particle.Type:X4}"));
            if (actual != string.Join(',', row[4..]))
            {
                if (failures++ < 12) Console.WriteLine($"CONTACT {string.Join(',', row[..4])}: {actual} != {string.Join(',', row[4..])}");
            }
            cases++;
        }
        if (cases != 64) throw new InvalidDataException("Incomplete combo contact matrix.");
        Console.WriteLine($"Combo contact: {cases} cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }

    private sealed class EmptyPopulation(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is 0xa1f000 or 0xa1f001 ? (byte)0xff : inner.ReadByte(address);
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
