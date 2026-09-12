using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Audio;

/// <summary>Original-CPU shared reaction/animation controls for #605, including the full native PLM pool.</summary>
internal static class EnemyBreakableTerrainAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(trace))) !=
            "5A74E32C57E7BEB4B6F2760E8D6F3367B7F6C09E68CB514F3EAD7B5877944FEC")
            throw new InvalidDataException("Use the pinned 96-row original-CPU terrain trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, ShaktoolDigDefinitions.Room);
        int compared = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(',').Select(int.Parse).ToArray())
                     .GroupBy(row => (Bts: row[0], Full: row[1])))
        foreach (int direction in Enumerable.Range(0, 4))
        {
            var assets = CartridgeRoomAssets.Load(bus, room);
            var level = assets.LevelData;
            int blockIndex = 5 * level.WidthInBlocks + 16;
            var plms = new RoomPlmSystem();
            if (group.Key.Full != 0)
                for (int index = 0; index < 40; index++)
                    if (!plms.TrySpawnEnemyBreakableBlock(level, blockIndex)) throw new InvalidDataException("Fixture pool did not fill.");
            level.SetForegroundEntry(blockIndex, 0xa110);
            level.SetBehavior(blockIndex, (byte)group.Key.Bts);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                new SnesVram(), new SnesCgram(), () => 1);
            typeof(RoomEnemySystem).GetField("_collisionPlms", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(enemies, plms);
            var enemy = enemies.Slots[0];
            enemy.XRadius = enemy.YRadius = 4;
            enemy.XSubposition = enemy.YSubposition = 0;
            enemy.XPosition = (ushort)(direction switch { 0 => 252, 1 => 276, _ => 264 });
            enemy.YPosition = (ushort)(direction switch { 2 => 76, 3 => 100, _ => 88 });
            string mover = direction < 2 ? "MoveEnemyHorizontallyIgnoringNonSquareSlopes" : "MoveEnemyVertically";
            int displacement = (direction % 2 == 0 ? 1 : -1) << 16;
            bool collision = (bool)typeof(RoomEnemySystem).GetMethod(mover, BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(enemies, [level, enemy, displacement])!;
            var streamer = level.CreateBackgroundStreamer();
            foreach (int[] expected in group)
            {
                int frame = expected[2];
                if (frame >= 0)
                {
                    var updates = plms.Step(bus, level, streamer, 128, 0, 0);
                    bool draw = frame is 0 or 4 or 8 or 12;
                    if ((updates.Count != 0) != draw)
                        throw new InvalidDataException($"Enemy crumble draw timing differs at {frame}.");
                    if (plms.SoundRequests.Count != (frame == 0 ? 1 : 0))
                        throw new InvalidDataException($"Enemy crumble sound timing differs at {frame}.");
                    if (frame == 0 && plms.SoundRequests[0] != new PlmSoundRequest(
                        SoundEffectId.FromCartridge(SoundEffectLibrary.Library2,
                            bus.ReadByte(0x840000 | (EnemyBreakableTerrainDefinitions.InstructionList + 2))), 3))
                        throw new InvalidDataException("Enemy crumble sound library, ID or queue limit differs.");
                }
                var slots = plms.PopulationSlots;
                var owner = slots.Count == 0 ? default : slots[0];
                if (group.Key.Full == 0 && (owner.HeaderPointer != 0) != (expected[5] != 0))
                    throw new InvalidDataException($"Enemy crumble allocation/deletion differs at {frame}.");
                if (collision != (expected[3] != 0) || level.ForegroundEntries.Span[blockIndex] != expected[4])
                    throw new InvalidDataException($"Enemy terrain native mismatch BTS={group.Key.Bts}, full={group.Key.Full}, frame={frame}.");
                if (group.Key.Full == 0 && expected[5] != 0 &&
                    (owner.HeaderPointer != expected[5] || owner.InstructionPointer != expected[6] || owner.InstructionTimer != expected[7]))
                    throw new InvalidDataException($"Enemy crumble owner/cursor mismatch at {frame}.");
                compared++;
            }
        }
        Console.WriteLine($"Enemy breakable terrain: {compared} comparisons against 96 native rows across four movement directions; BTS mask, solidity, full pool, draw and sound timing verified.");
        if (compared != 384) throw new InvalidDataException("Incomplete native terrain comparison.");
        return 0;
    }
}
