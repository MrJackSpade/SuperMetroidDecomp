using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Original bank-$94 wall checks must activate real item owners without moving Samus.</summary>
internal static class RemoteItemComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "86F1FB74D050FBA44FC68FCF63225FEC6ECF860F578331AEBBC2027D8B2A7FE5")
            throw new InvalidDataException("Use the accepted remote-item v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            string[] row = line.Split(',');
            if (row.Length != 13) throw new InvalidDataException("Malformed remote-item row.");
            bool left = row[0] == "1";
            int gap = int.Parse(row[1]), blocker = int.Parse(row[2]), fraction = int.Parse(row[3]), distance = int.Parse(row[4]);
            int index = ((((left ? 1 : 0) * 17 + gap) * 3 + blocker) * 3 + fraction) * 9 + distance - 1;
            if (index != cases) throw new InvalidDataException("Reordered remote-item matrix.");
            var blocks = new ushort[256];
            int column = left ? 6 : 9;
            blocks[8 * 16 + column] = 0;
            if (blocker != 0) blocks[(blocker == 1 ? 7 : 9) * 16 + column] = (ushort)((int)RoomCollisionType.SolidBlock << 12);
            var level = new RoomLevelData(16, 16, blocks, new byte[256], new ushort[256], new byte[0x400 * 8]);
            var state = new SamusKinematicsState
            {
                XPosition = (ushort)(left ? 117 + gap : 139 - gap), YPosition = 136,
                XSubposition = (ushort)(fraction == 0 ? 0 : fraction == 1 ? 0x8000 : 0xffff),
                YSubposition = 0x3456, XRadius = 5, YRadius = 12,
                CollisionPose = left ? SamusPoseIds.SpinJumpLeftPose : SamusPoseIds.SpinJumpRightPose,
            };
            var selected = new RemoteItemPopulation(bus, (byte)column);
            var system = new Bank80SystemState();
            var samus = new SamusState { Health = 99, MaxHealth = 99 };
            var plms = new RoomPlmSystem();
            var streamer = level.CreateBackgroundStreamer();
            if (plms.LoadRoomPopulation(selected, level, streamer, new SnesVram(),
                RemoteItemFixtureData.PopulationPointer, system, areaIndex: AreaId.Crateria,
                getSamus: () => samus, isAreaTorizoDefeated: () => false) != 1)
                throw new InvalidDataException("Expected exactly one missile owner.");
            // Native seed represents the visible item after its first draw instruction.
            // Let the managed loader perform that same production draw before probing.
            plms.Step(selected, level, streamer, 0, 0, 0);
            var result = SamusBlockCollision.ProbeWallHorizontal(selected, level, state, (left ? -distance : distance) << 16, plms);
            bool expectedPickup = row[11] == "00FF";
            // Consume the actual collision notification through the production PLM handler,
            // then check resources, permanent bit and event instead of a proximity mock.
            plms.Step(selected, level, streamer, 0, 0, 0);
            bool acquired = plms.CollectiblePickupEvents.Count == 1;
            if (acquired != expectedPickup || samus.Missiles != (expectedPickup ? 5 : 0) ||
                samus.MaxMissiles != (expectedPickup ? 5 : 0) || system.HasCollectedItemBit(0) != expectedPickup)
                throw new InvalidDataException($"Remote item {string.Join(',', row[..5])}: expected={expectedPickup}, event={acquired}, ammo={samus.Missiles}/{samus.MaxMissiles}, bit={system.HasCollectedItemBit(0)}.");
            string actual = $"{state.XPosition:X4},{state.XSubposition:X4},{state.YPosition:X4},{state.YSubposition:X4}," +
                $"{(result.Collided ? 1 : 0):X4},{Math.Abs(result.AcceptedDisplacement):X8},{(acquired ? 255 : 0):X4},{RoomPlmHeaders.ExposedMissileTank:X4}";
            if (actual != string.Join(',', row[5..]))
            {
                if (mismatches++ < 12) Console.WriteLine($"ITEM {string.Join(',', row[..5])}: {actual} != {string.Join(',', row[5..])}");
            }
            cases++;
        }
        if (cases != 2754) throw new InvalidDataException("Incomplete remote-item matrix.");
        Console.WriteLine($"Remote item: {cases} original-CPU probes, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
