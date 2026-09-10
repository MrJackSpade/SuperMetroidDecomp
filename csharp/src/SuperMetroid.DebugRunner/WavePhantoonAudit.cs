using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Per-particle Wave Shield contacts against retail Phantoon hitboxes.</summary>
internal static class WavePhantoonAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "49CBBF9D478C6F3351B62B825BB5D0B8E2093DD6822EC8D8953EAE2BB2E61675")
            throw new InvalidDataException("Use the accepted wave-phantoon-418-v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, 0xcd13);
        var assets = CartridgeRoomAssets.Load(bus, room);
        RoomEnemySystem enemies = null!;
        SamusProjectileSystem projectiles = null!;
        SamusBombProjectileSystem shared = null!;
        SamusState samus = null!;
        int cases = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            string[] row = line.Split(',');
            if (row[3] == "0")
            {
                samus = new SamusState { XPosition = 128, YPosition = 128,
                    Pose = SamusPoseIds.FacingRightNormalPose,
                    EquippedBeams = 0x1001, PowerBombs = 2, SelectedHudItem = 3 };
                projectiles = new SamusProjectileSystem();
                shared = new SamusBombProjectileSystem();
                if (!projectiles.TryActivateCombo(bus, samus, shared, out _))
                    throw new InvalidDataException("Fixture must activate Wave Shield.");
                for (int i = 0; i < 4; i++)
                {
                    var particle = projectiles.Slots[i];
                    particle.XPosition = particle.YPosition =
                        (int.Parse(row[0]) & (1 << i)) != 0 ? (ushort)128 : (ushort)1024;
                    projectiles.RunProjectileInstructionHandler(bus, particle);
                }
                enemies = new RoomEnemySystem();
                enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                    new SnesVram(), new SnesCgram(), () => 1, level: assets.LevelData,
                    samus: samus, isAreaBossDefeated: () => false);
                var body = enemies.Phantoon!.Body;
                body.XPosition = body.YPosition = 128;
                body.Health = 2500;
                body.SpritemapPointer = ushort.Parse(row[1]);
                body.Properties = 0;
                body.ExtraProperties = 4;
                body.VariableF = ushort.Parse(row[2]);
                body.VariableE = 60;
                enemies.Phantoon.Tentacles!.VariableA = 0;
                enemies.Phantoon.Tentacles.VariableB = 0;
                enemies.Phantoon.Tentacles.Parameter2 = 0;
            }
            enemies.ResolvePhantoonProjectileHits(bus, projectiles, shared);
            foreach (var particle in projectiles.Slots.Take(4).Reverse())
                if (particle.IsActive && (particle.Direction & 0xf0) != 0)
                    projectiles.StepWaveCombo(bus, samus, particle, shared);
            var state = enemies.Phantoon!;
            string actual = $"{state.Body.Health:X4},{state.Body.VariableF:X4},{state.Body.VariableE:X4}," +
                $"{state.Body.Properties:X4},{state.Tentacles!.VariableB:X4},{state.Tentacles.Parameter2:X4}," +
                $"{projectiles.ProjectileCounter:X4}," +
                string.Concat(projectiles.Slots.Take(4).Select(particle => $"{particle.Type:X4}"));
            if (actual != string.Join(',', row[4..]))
            {
                if (failures++ < 12)
                    Console.WriteLine($"WAVE PHANTOON {string.Join(',', row[..4])}: {actual} != {string.Join(',', row[4..])}");
            }
            cases++;
        }
        if (cases != 384) throw new InvalidDataException("Incomplete Wave/Phantoon contact matrix.");
        Console.WriteLine($"Wave Phantoon: {cases} contact passes, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
