using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Original elevator actor admission before/after the actual Flash entry callback.</summary>
internal static class FlashElevatorEntryAudit
{
    public static int Run(string rom, string trace)
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(retail.Rom)) != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72" ||
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != "67113DB05F6FB27EC0107C15773E685BABCA645AABFB3DFDB240E7FF13B9F794")
            throw new InvalidDataException("Use the pinned ROM and native Flash/elevator trace.");
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 32 || rows.Any(row => row.Length != 12)) throw new InvalidDataException("Incomplete elevator entry matrix.");
        int mismatches = 0;
        foreach (var row in rows)
        {
            var bus = new PopulationSelectionAddressSpace(retail,
                [new RoomEnemyPopulationRecord(0xd73f, 136, 256, 0, 0, 0, ushort.Parse(row[0]), 0)]);
            var samus = new SamusState { Pose = row[1] == "1" ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose,
                XPosition = 136, YPosition = 235, Health = 49, MaxHealth = 99, Missiles = 10, SuperMissiles = 10, PowerBombs = 10 };
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, PopulationSelectionAddressSpace.PopulationPointer, PopulationSelectionAddressSpace.TilesetPointer,
                new SnesVram(), new SnesCgram(), () => 0, samus: samus);
            if (row[2] == "1") enemies.PublishElevatorDoorContact();
            if (row[4] == "0" && !samus.CrystalFlash.TryBegin(bus, samus, 0x470, 0x40)) throw new InvalidDataException("Early Flash rejected.");
            enemies.StepFrame(0, 144, false, samus, newlyPressedControllerInput: (ushort)(row[3] == "1" ? 0x470 : 0));
            if (row[4] == "1" && !samus.CrystalFlash.TryBegin(bus, samus, 0x470, 0x40)) throw new InvalidDataException("Late Flash rejected.");
            string actual = $"{(ushort)enemies.ElevatorStatus:X4},{samus.Pose:X2},{samus.XPosition:X4},{samus.YPosition:X4}," +
                $"{(samus.CrystalFlash.Phase == CrystalFlashPhase.Raising ? 1 : 0)},{samus.SharedShineTimer:X4},{samus.CrystalFlash.SpecialPaletteType:X4}";
            string expected = string.Join(',', row[5..]);
            if (actual == expected) continue;
            mismatches++;
            Console.WriteLine($"Flash/elevator {string.Join(',', row[..5])}: {actual} != {expected}");
        }
        Console.WriteLine($"Flash/elevator entry: {rows.Length} cases, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
