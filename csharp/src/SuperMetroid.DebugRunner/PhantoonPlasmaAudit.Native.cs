using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using System.Security.Cryptography;

internal static partial class PhantoonPlasmaAudit
{
    /// <summary>Compare original-CPU contacts, with/without a preliminary Power Beam hit.</summary>
    public static int RunNative(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "5DD3B0EBB99165DC54F986069329C488A7324FB6A10D00B27E992A9C0E560A88")
            throw new InvalidDataException("Use the accepted xplasma-phantoon-native-v5 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, 0xcd13);
        var assets = CartridgeRoomAssets.Load(bus, room);
        RoomEnemySystem enemies = null!;
        SamusProjectileSystem shots = null!;
        var shared = new SamusBombProjectileSystem();
        var samus = new SamusState();
        int count = 0, failures = 0;
        foreach (string line in File.ReadLines(trace).Skip(1))
        {
            ushort[] row = line.Split(',').Select(ushort.Parse).ToArray();
            if (row.Length != 17) throw new InvalidDataException("Invalid Phantoon trace row.");
            if (row[3] == 0)
            {
                enemies = new RoomEnemySystem();
                shots = new SamusProjectileSystem();
                enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                    new SnesVram(), new SnesCgram(), () => 1, level: assets.LevelData,
                    samus: new SamusState(), isAreaBossDefeated: () => false);
                var body = enemies.Phantoon!.Body;
                body.XPosition = body.YPosition = 128;
                body.Health = 2500;
                body.SpritemapPointer = 0xdee7;
                body.Properties = 0;
                body.ExtraProperties = 4;
                body.VariableF = row[0];
                body.VariableE = 60;
                enemies.Phantoon.Tentacles!.VariableA = 0;
                enemies.Phantoon.Tentacles.VariableB = 0;
                enemies.Phantoon.Tentacles.Parameter2 = 0;
            }
            if (row[3] >= 2)
            {
                enemies.StepFrame(0, 0, timeIsFrozen: row[3] < 22, samus: samus,
                    level: assets.LevelData, samusProjectiles: shots, sharedProjectiles: shared,
                    resolveSamusContactBeforeAi: true);
            }
            else if (row[3] != 0 || row[1] != 0)
            {
                shots = new SamusProjectileSystem();
                var shot = shots.Slots[0];
                shot.Type = row[3] == 0 ? (ushort)0x8010 : row[2];
                shot.Damage = shot.Type switch { 0x8000 => 20, 0x8010 => 60, 0x8008 => 150, 0x8018 => 450,
                    _ => throw new InvalidDataException("Unexpected native shot type.") };
                shot.XPosition = shot.YPosition = 128;
                shot.XRadius = shot.YRadius = 4;
                shot.InstructionPointer = 0x9000;
                shot.InstructionTimer = 1;
                enemies.ResolvePhantoonProjectileHits(bus, shots, shared);
            }
            var state = enemies.Phantoon!;
            var head = state.Body;
            ushort[] actual = [head.Health, head.InvincibilityTimer, head.FlashTimer,
                head.VariableF, head.VariableE, head.Properties, state.Tentacles!.VariableB,
                state.Tentacles.VariableA, state.Tentacles.Parameter2];
            if (!actual.SequenceEqual(row[4..13]) || head.XPosition != row[14] ||
                head.YPosition != row[15] || head.CurrentInstruction != row[16])
            {
                failures++;
                Console.WriteLine($"Phantoon {string.Join(',', row[..4])}: {string.Join(',', actual)} != {string.Join(',', row[4..13])}");
            }
            count++;
        }
        if (count != 268) throw new InvalidDataException("Expected all 268 native contact/freeze/release records.");
        Console.WriteLine($"Native Phantoon Plasma: {count} records, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
