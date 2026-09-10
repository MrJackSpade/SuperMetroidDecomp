using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Compare hand collision admission and actual statue/progression writes to native execution.</summary>
internal static class HandProbeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "C6211AFA88A028CEE251A2EE580352253E04EE3A3D3B01AD25CF911E35D7F2E4")
            throw new InvalidDataException("Use the accepted hand-probe v1 capture.");
        int cases = 0, mismatches = 0;
        foreach (string line in File.ReadLines(capture).Skip(1))
        {
            var row = line.Split(',');
            if (row.Length != 8) throw new InvalidDataException("Malformed hand probe row.");
            bool ship = row[0] == "1", probe = row[1] == "1", eligible = row[3] == "1";
            int poseIndex = int.Parse(row[2]);
            if (((((ship ? 1 : 0) * 2 + (probe ? 1 : 0)) * 4 + poseIndex) * 2 + (eligible ? 1 : 0)) != cases)
                throw new InvalidDataException("Reordered hand cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            // Load the actual statue population even for the negative progression control,
            // then vary the flag sampled by the hand setup without rebuilding the room.
            runtime.System.SetBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
            runtime.LoadCartridgeRoomForDebug(ship ? HandProbeFixtureData.WreckedShipRoom : HandProbeFixtureData.LowerNorfairRoom);
            if (!eligible) runtime.System.ClearBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
            var samus = runtime.Samus!; var level = runtime.LevelData!;
            for (int i = 0; i < level.WidthInBlocks * level.HeightInBlocks; i++) { level.SetForegroundEntry(i, 0); level.SetBehavior(i, 0); }
            int block = 10 * level.WidthInBlocks + 8;
            level.SetForegroundEntry(block, 0xb123);
            level.SetBehavior(block, ship ? ChozoStatuePlmRomData.WreckedShipHandBts.Value : ChozoStatuePlmRomData.LowerNorfairHandBts.Value);
            samus.CollectedItems = eligible ? (ushort)SamusEquipmentFlags.SpaceJump : (ushort)0;
            samus.Pose = poseIndex switch { 0 => SamusPoseIds.MorphBallGroundRightPose, 1 => SamusPoseIds.MorphBallGroundLeftPose,
                2 => SamusPoseIds.SpringBallGroundRightPose, 3 => SamusPoseIds.SpringBallGroundLeftPose, _ => throw new InvalidDataException() };
            samus.XPosition = 136; samus.YPosition = 149;
            samus.Kinematics.XRadius = 5; samus.Kinematics.YRadius = 7;
            samus.Kinematics.CollisionPose = samus.Pose;
            var result = SamusBlockCollision.MoveVertical(bus, level, samus.Kinematics, 7 << 16, true, plms: runtime.Plms,
                includeSolidEnemies: false, blockReactionDirection: probe ? SamusCollisionDirection.NonDirectionalProbe : null);
            string actual = $"{(result.Collided ? 1 : 0)},{runtime.Enemies.Slots[0].Parameter1:X4}," +
                $"{level.GetCollisionBlockByIndex(block).LevelWord:X4},{(runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid) ? 1 : 0)}";
            if (actual != string.Join(',', row[4..]))
            {
                if (mismatches++ < 12) Console.WriteLine($"HAND {string.Join(',', row[..4])}: {actual} != {string.Join(',', row[4..])}");
            }
            cases++;
        }
        if (cases != 32) throw new InvalidDataException("Incomplete hand matrix.");
        Console.WriteLine($"Hand probes: {cases} native comparisons, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}

/// <summary>Retail populations used as real actor owners for the isolated contact fixture.</summary>
internal static class HandProbeFixtureData
{
    /// <summary>$8F:B1E5, Lower Norfair Chozo statue room, containing the acid-lowering actor.</summary>
    public const ushort LowerNorfairRoom = 0xb1e5;
    /// <summary>$8F:C98E, Wrecked Ship walking-statue room; defeated-boss state supplies the actor.</summary>
    public const ushort WreckedShipRoom = 0xc98e;
}
