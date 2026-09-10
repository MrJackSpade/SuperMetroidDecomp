using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Five retail Yard trajectories against the native enemy dispatcher on Aqueduct terrain.</summary>
internal static class YardNativeComparisonAudit
{
    public static int Run(string rom, string capture)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(capture))) !=
            "997AB174ED8FEF4FA04C76A42217DED6BF5042D77B469C9504BF0C6E0CE830AD")
            throw new InvalidDataException("Use the accepted yard-room native v1 capture.");
        var rows = File.ReadLines(capture).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 30000) throw new InvalidDataException("Incomplete Yard matrix.");
        int mismatches = 0, cases = 0;
        foreach (var group in rows.GroupBy(row => row[0]))
        {
            if (int.Parse(group.Key) != cases++) throw new InvalidDataException("Reordered Yard focus cases.");
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var room = CartridgeRoomHeader.Load(bus, 0xd5a7);
            var assets = CartridgeRoomAssets.Load(bus, room);
            var vram = new SnesVram(); var cgram = new SnesCgram();
            assets.LoadGraphics(vram, cgram);
            var random = new Bank80SystemState();
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                vram, cgram, random.NextRandom, random.SetRandomNumber);
            var samus = new SamusState { Health = 999, MaxHealth = 999, Pose = SamusPoseIds.FacingRightNormalPose };
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            int compared = 0;
            var reported = new HashSet<int>();
            foreach (var frameRows in group.Chunk(5))
            {
                var seed = frameRows[0];
                ushort Word(int index) => ushort.Parse(seed[index], NumberStyles.HexNumber);
                int frame = int.Parse(seed[1]);
                if (frame * 5 != compared || frameRows.Length != 5) throw new InvalidDataException("Reordered Yard capture.");
                samus.XPosition = Word(5); samus.YPosition = Word(6);
                samus.Pose = Word(7) == 4 ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
                enemies.EarthquakeTimer = Word(8); enemies.EarthquakeType = 20;
                enemies.StepFrame(Word(3), Word(4), false, samus, level: assets.LevelData,
                    nmiFrameCounter8: (byte)(frame + 2), resolveSamusContactBeforeAi: true);
                for (int slot = 0; slot < 5; slot++)
                {
                    var row = frameRows[slot];
                    if (row.Length != 26 || int.Parse(row[2]) != slot) throw new InvalidDataException("Malformed Yard row.");
                    var e = enemies.Slots[slot]; var state = enemies.YardStates[slot]!;
                    string actual = $"{random.RandomNumber:X4},{e.XPosition:X4}{e.XSubposition:X4},{e.YPosition:X4}{e.YSubposition:X4}," +
                        $"{(ushort)e.Properties:X4},{e.CurrentInstruction:X4},{e.InstructionTimer:X4},{e.SpritemapPointer:X4}," +
                        $"{e.VariableA:X4},{e.VariableB:X4},{e.VariableC:X4},{e.VariableD:X4},{e.VariableE:X4},{e.VariableF:X4}," +
                        $"{state.Direction:X4},{state.Behavior:X4},{state.AirborneYVelocity:X4}{state.AirborneYSubvelocity:X4}," +
                        $"{state.AirborneXVelocity:X4}{state.AirborneXSubvelocity:X4}";
                    string expected = string.Join(',', row[9..]);
                    if (actual != expected)
                    {
                        mismatches++;
                        if (reported.Add(slot)) Console.WriteLine($"YARD follow={group.Key} slot={slot} frame={frame}: {actual} != {expected}");
                    }
                    compared++;
                }
            }
            if (compared != 6000) throw new InvalidDataException("Incomplete Yard focus case.");
        }
        if (cases != 5) throw new InvalidDataException("Missing retail Yard focus cases.");
        Console.WriteLine($"Yard native comparison: {rows.Length} actor frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
