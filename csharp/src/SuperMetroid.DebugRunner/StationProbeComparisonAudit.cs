using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Native station setup distinguishes real horizontal contact from direction-$F observation.</summary>
internal static class StationProbeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "7214D1C1E3ABCB039ABD9EAA3D599908ADF30ED2F68632E0A6D393A67B8793AC")
            throw new InvalidDataException("Use the accepted station-probe v2 capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            var row = line.Split(',');
            if (row.Length != 6) throw new InvalidDataException("Malformed station probe row.");
            int kind = int.Parse(row[0]), gap = int.Parse(row[3]);
            bool left = row[1] == "1", probe = row[2] == "1";
            if ((((kind * 2 + (left ? 1 : 0)) * 2 + (probe ? 1 : 0)) * 10 + gap) != cases)
                throw new InvalidDataException("Reordered station probes.");
            int column = left ? 6 : 9;
            int parentColumn = column + (left ? -1 : kind == 0 ? 2 : 1);
            ushort header = kind switch { 0 => RoomPlmHeaders.MapStation, 1 => RoomPlmHeaders.EnergyStation, 2 => RoomPlmHeaders.MissileStation, _ => throw new InvalidDataException() };
            var selected = new StationProbePopulation(bus, header, (byte)parentColumn);
            var level = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], new byte[0x400 * 8]);
            var streamer = level.CreateBackgroundStreamer();
            var plms = new RoomPlmSystem();
            var samus = new SamusState { Health = 50, MaxHealth = 99, MaxMissiles = 5,
                XPosition = (ushort)(left ? 117 + gap : 139 - gap), YPosition = 139,
                Pose = left ? SamusPoseIds.RanIntoWallLeftPose : SamusPoseIds.RanIntoWallRightPose };
            plms.LoadRoomPopulation(selected, level, streamer, new SnesVram(), RemoteItemFixtureData.PopulationPointer,
                new Bank80SystemState(), areaIndex: AreaId.Crateria, getSamus: () => samus, isAreaTorizoDefeated: () => false);
            // Isolate the access tile created by the real station setup from its other tiles.
            for (int i = 0; i < 256; i++) if (i != 8 * 16 + column) { level.SetForegroundEntry(i, 0); level.SetBehavior(i, 0); }
            samus.Kinematics.XRadius = 5; samus.Kinematics.YRadius = 12;
            samus.Kinematics.CollisionPose = samus.Pose;
            var result = probe
                ? SamusBlockCollision.ProbeWallHorizontal(selected, level, samus.Kinematics, (left ? -8 : 8) << 16, plms)
                : SamusBlockCollision.MoveHorizontal(selected, level, samus.Kinematics, (left ? -8 : 8) << 16, plms: plms);
            bool activated = plms.Stations.Single().Triggered;
            if (result.Collided != (row[4] == "1") || activated != (row[5] == "1"))
            {
                if (mismatches++ < 12) Console.WriteLine($"STATION {string.Join(',', row[..4])}: collision={result.Collided}, activation={activated}; native={row[4]}/{row[5]}");
            }
            cases++;
        }
        if (cases != 120) throw new InvalidDataException("Incomplete station matrix.");
        Console.WriteLine($"Station probes: {cases} native comparisons, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}

/// <summary>Authored station population; station definitions and setup remain retail.</summary>
internal sealed class StationProbePopulation(ISnesAddressSpace inner, ushort header, byte column, byte row = 8) : ISnesAddressSpace
{
    public byte ReadByte(int address)
    {
        int offset = address - RemoteItemFixtureData.PopulationAddress;
        ReadOnlySpan<byte> list = [(byte)(header & 255), (byte)(header >> 8), column, row, 0, 0, 0, 0];
        return (uint)offset < list.Length ? list[offset] : inner.ReadByte(address);
    }
    public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
}
