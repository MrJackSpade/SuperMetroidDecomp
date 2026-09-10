using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Compare native save access acceptance for ordinary downward movement and pose observations.</summary>
internal static class SaveProbeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "7DF0EFBAE344C53029170497C3A575363CBFFED8704F910EF3CB6C7657698120")
            throw new InvalidDataException("Use the accepted save-probe v1 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            var row = line.Split(',');
            if (row.Length != 6) throw new InvalidDataException("Malformed save probe row.");
            bool probe = row[0] == "1", center = row[1] == "1";
            int gap = int.Parse(row[2]), parity = int.Parse(row[3]);
            if (((((probe ? 1 : 0) * 2 + (center ? 1 : 0)) * 10 + gap) * 2 + parity) != cases)
                throw new InvalidDataException("Reordered save probes.");
            var selected = new StationProbePopulation(bus, RoomPlmHeaders.SaveStation, 8, 10);
            var level = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], new byte[0x400 * 8]);
            var streamer = level.CreateBackgroundStreamer();
            var plms = new RoomPlmSystem();
            var samus = new SamusState { Health = 99, MaxHealth = 99,
                XPosition = (ushort)(center ? 136 : 132), YPosition = (ushort)(149 - gap),
                Pose = SamusPoseIds.FacingRightNormalPose };
            plms.LoadRoomPopulation(selected, level, streamer, new SnesVram(), RemoteItemFixtureData.PopulationPointer,
                new Bank80SystemState(), areaIndex: AreaId.Crateria, getSamus: () => samus, isAreaTorizoDefeated: () => false);
            for (int i = 0; i < 256; i++) if (i != 168) { level.SetForegroundEntry(i, 0); level.SetBehavior(i, 0); }
            samus.Kinematics.XRadius = 5; samus.Kinematics.YRadius = 12;
            samus.Kinematics.CollisionPose = samus.Pose;
            var result = SamusBlockCollision.MoveVertical(selected, level, samus.Kinematics, 7 << 16,
                scanLeftToRight: parity == 0, plms: plms,
                blockReactionDirection: probe ? SamusCollisionDirection.NonDirectionalProbe : null);
            bool activated = plms.Stations.Single().Triggered;
            if (result.Collided != (row[4] == "1") || activated != (row[5] == "1"))
            {
                if (mismatches++ < 12) Console.WriteLine($"SAVE {string.Join(',', row[..4])}: collision={result.Collided}, activation={activated}; native={row[4]}/{row[5]}");
            }
            cases++;
        }
        if (cases != 80) throw new InvalidDataException("Incomplete save matrix.");
        Console.WriteLine($"Save probes: {cases} native comparisons, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
