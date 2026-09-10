using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Compare same-pass remote pickup resources and message against the original PLM handler.</summary>
internal static class ItemAcquisitionComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "AF61194A7FD39C287F711FA1518E48D03B11D51AA3EA85538634BF695C06B8A8")
            throw new InvalidDataException("Use the accepted item-acquisition v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, pickups = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            string[] row = line.Split(',');
            if (row.Length != 9) throw new InvalidDataException("Malformed acquisition row.");
            bool left = row[0] == "1";
            int gap = int.Parse(row[1]), blocker = int.Parse(row[2]), timer = int.Parse(row[3]);
            if (((((left ? 1 : 0) * 17 + gap) * 3 + blocker) * 4 + timer - 1) != cases)
                throw new InvalidDataException("Reordered acquisition matrix.");
            int column = left ? 6 : 9;
            var blocks = new ushort[256];
            if (blocker != 0) blocks[(blocker == 1 ? 7 : 9) * 16 + column] =
                (ushort)((int)RoomCollisionType.SolidBlock << 12);
            var level = new RoomLevelData(16, 16, blocks, new byte[256], new ushort[256], new byte[0x400 * 8]);
            var selected = new RemoteItemPopulation(bus, (byte)column);
            var system = new Bank80SystemState();
            var samus = new SamusState { Health = 99, MaxHealth = 99 };
            var plms = new RoomPlmSystem();
            var streamer = level.CreateBackgroundStreamer();
            if (plms.LoadRoomPopulation(selected, level, streamer, new SnesVram(),
                RemoteItemFixtureData.PopulationPointer, system, areaIndex: AreaId.Crateria,
                getSamus: () => samus, isAreaTorizoDefeated: () => false) != 1)
                throw new InvalidDataException("Expected one missile owner.");
            // Reach each remaining timer through real draws, without editing private state.
            for (int step = 0; step <= 4 - timer; step++) plms.Step(selected, level, streamer, 0, 0, 0);
            var state = new SamusKinematicsState
            {
                XPosition = (ushort)(left ? 117 + gap : 139 - gap), YPosition = 136,
                XRadius = 5, YRadius = 12,
                CollisionPose = left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose,
            };
            SamusBlockCollision.ProbeWallHorizontal(selected, level, state, (left ? -8 : 8) << 16, plms);
            if (samus.Missiles != 0 || system.HasCollectedItemBit(0))
                throw new InvalidDataException("Collision must notify, not award before the PLM handler.");
            plms.Step(selected, level, streamer, 0, 0, 0);
            int events = plms.CollectiblePickupEvents.Count;
            if (events > 1) throw new InvalidDataException("Duplicate pickup.");
            int message = events == 0 ? 0 : (byte)plms.CollectiblePickupEvents[0].MessageBoxIndex;
            string actual = $"{(events == 0 ? 0 : 255):X4},{message:X4},{samus.Missiles:X4}," +
                $"{samus.MaxMissiles:X4},{(system.HasCollectedItemBit(0) ? 1 : 0):X4}";
            if (actual != string.Join(',', row[4..]))
                throw new InvalidDataException($"Acquisition {string.Join(',', row[..4])}: {actual} != {string.Join(',', row[4..])}");
            pickups += events;
            cases++;
        }
        if (cases != 408 || pickups != 128) throw new InvalidDataException($"Incomplete acquisition coverage: {cases}/{pickups}.");
        Console.WriteLine($"Item acquisition: {cases} native handler comparisons, {pickups} same-pass pickups, zero mismatches.");
        return 0;
    }
}
