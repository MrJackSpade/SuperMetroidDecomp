using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Positive item/door side effects of the real compact-to-full pose resolver.</summary>
internal static class PoseTriggerComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "D226C25CF44C0A12037EB06B6C33A474A59957A8DB8AF076B6DAFFC4D59152C8")
            throw new InvalidDataException("Use the accepted pose-trigger v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            string[] row = line.Split(',');
            if (row.Length != 10) throw new InvalidDataException("Malformed pose-trigger row.");
            int item = int.Parse(row[0]), above = int.Parse(row[1]), gap = int.Parse(row[2]), parity = int.Parse(row[3]);
            if (((item * 2 + above) * 13 + gap) * 2 + parity != cases)
                throw new InvalidDataException("Reordered pose-trigger matrix.");
            var blocks = new ushort[256];
            if (item == 0) blocks[10 * 16 + 8] = (ushort)((int)RoomCollisionType.DoorBlock << 12);
            var level = new RoomLevelData(16, 16, blocks, new byte[256], new ushort[256], new byte[0x400 * 8],
                doorListPointer: RemoteDoorFixtureData.DoorList);
            var samus = new SamusState { Pose = SamusPoseIds.FallingAimDownRightPose, XPosition = 136,
                YPosition = (ushort)(above != 0 ? 185 + gap : 150 - gap), Health = 99, MaxHealth = 99 };
            samus.Kinematics.YSubposition = 0x3456;
            samus.Kinematics.YDirection = 2;
            samus.HorizontalSpeed.BaseSpeed = 1; samus.HorizontalSpeed.BaseSubspeed = 0x4000;
            samus.RefreshCollisionRadii(bus);
            var plms = new RoomPlmSystem();
            var system = new Bank80SystemState();
            var streamer = level.CreateBackgroundStreamer();
            ISnesAddressSpace selected = item != 0 ? new RemoteItemPopulation(bus, 8, 10) : bus;
            if (item != 0)
            {
                if (plms.LoadRoomPopulation(selected, level, streamer, new SnesVram(), RemoteItemFixtureData.PopulationPointer,
                    system, areaIndex: AreaId.Crateria, getSamus: () => samus, isAreaTorizoDefeated: () => false) != 1)
                    throw new InvalidDataException("Expected one visible missile owner.");
                plms.Step(selected, level, streamer, 0, 0, 0);
            }
            uint initialX = samus.Kinematics.XFixed, initialY = samus.Kinematics.YFixed;
            samus.TryApplyCompactAerialTransition(selected, level, SamusPoseIds.FallingRightPose, (ushort)parity, plms);
            plms.Step(selected, level, streamer, 0, 0, 0);
            bool acquired = plms.CollectiblePickupEvents.Count == 1;
            ushort door = level.PendingDoorTransition?.Pointer ?? 0;
            // The inclusive block boundary differs by one pixel above versus below;
            // preserve the cartridge result instead of forcing geometric symmetry.
            bool withinReach = gap <= (above != 0 ? 9 : 8);
            if ((acquired || door != 0) != withinReach)
                throw new InvalidDataException("Pose-trigger success/failure distance boundary changed.");
            string actual = $"{samus.Kinematics.XFixed:X8},{samus.Kinematics.YFixed:X8},{samus.Pose:X2},{door:X4},{(acquired ? 255 : 0):X4},{samus.HorizontalSpeed.BaseFixed:X8}";
            if (actual != string.Join(',', row[4..]))
            {
                if (mismatches++ < 8) Console.WriteLine($"POSE TRIGGER {string.Join(',', row[..4])}: {actual} != {string.Join(',', row[4..])}");
            }
            if (samus.Kinematics.XFixed != initialX || samus.Kinematics.YFixed != initialY ||
                samus.Missiles != (acquired ? 5 : 0) || samus.MaxMissiles != (acquired ? 5 : 0) || system.HasCollectedItemBit(0) != acquired)
                throw new InvalidDataException("Remote pose check moved Samus or lost actual item acquisition effects.");
            cases++;
        }
        if (cases != 104) throw new InvalidDataException("Incomplete pose-trigger matrix.");
        Console.WriteLine($"Pose trigger: {cases} original-CPU cases, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
