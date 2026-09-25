using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledSpeedBoosterPlmPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] words = SpeedBoosterBlockPlmProgramDefinitions
            .MechanicsWordAddresses().Order().ToArray();
        ushort[] bytes = SpeedBoosterBlockPlmProgramDefinitions
            .MechanicsByteAddresses().Order().ToArray();
        AssertEqual(74, words.Length,
            "five speed-block lists and bomb reveal have 74 compiled words");
        AssertEqual(5, bytes.Length,
            "five speed-block lists have five compiled sound operands");
        AssertEqual(words.Length, words.Distinct().Count(),
            "speed-block program word addresses are unique");
        foreach (ushort pointer in words)
        {
            AssertTrue(SpeedBoosterBlockPlmProgramDefinitions.TryReadMechanicsWord(
                    pointer, out ushort value),
                $"speed-block control word $84:{pointer:X4} is compiled");
            AssertEqual((ushort)(rom.ReadByte(0x840000 | pointer) |
                    rom.ReadByte(0x840000 | (pointer + 1)) << 8),
                value, $"speed-block control word $84:{pointer:X4} matches ROM");
        }
        foreach (ushort pointer in bytes)
        {
            AssertTrue(SpeedBoosterBlockPlmProgramDefinitions.TryReadMechanicsByte(
                    pointer, out byte value),
                $"speed-block sound operand $84:{pointer:X4} is compiled");
            AssertEqual(rom.ReadByte(0x840000 | pointer), value,
                $"speed-block sound operand $84:{pointer:X4} matches ROM");
        }
        AssertTrue(!SpeedBoosterBlockPlmProgramDefinitions.TryReadMechanicsWord(
                0xc9ba, out _),
            "unused Screw Attack list remains outside the reachable Speed Booster domain");

        RoomPlmShotBlockDrawDefinitions.DrawList reveal =
            SpeedBoosterBlockPlmDrawDefinitions.BombReveal;
        AssertEqual((ushort)1, reveal.Runs.Span[0].DirectionAndCount,
            "bombed speed-block reveal has one physical block");
        AssertEqual((ushort)0xb0b6, reveal.Runs.Span[0].LevelWords.Span[0],
            "bombed speed-block reveal retains native type-B collision");
        ushort drawPointer = reveal.Pointer;
        byte[] expectedDraw = [0x01, 0x00, 0xb6, 0xb0, 0x00, 0x00];
        for (int offset = 0; offset < expectedDraw.Length; offset++)
            AssertEqual(rom.ReadByte(0x840000 | (drawPointer + offset)),
                expectedDraw[offset],
                $"bombed speed-block draw byte {offset} matches ROM");

        var cases = new (RoomBlockBehavior Bts, AreaId Area, bool Respawns)[]
        {
            (new RoomBlockBehavior(0x0e), AreaId.Crateria, true),
            (new RoomBlockBehavior(0x0f), AreaId.Crateria, false),
            (new RoomBlockBehavior(0x82), AreaId.Brinstar, true),
            (new RoomBlockBehavior(0x83), AreaId.Brinstar, false),
            (new RoomBlockBehavior(0x84), AreaId.Brinstar, true),
        };
        foreach ((RoomBlockBehavior bts, AreaId area, bool respawns) in cases)
        {
            (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms,
                int blockIndex) = CreateSpeedBoosterBlockFixture(bts, area);
            var guarded = new SpeedBoosterPlmReadGuard(bus);
            SamusState samus = CreateSpeedBoosterCollisionSamus(active: true);
            BlockMoveResult contact = SamusBlockCollision.MoveVertical(
                guarded, level, samus.Kinematics, 4 << 16,
                scanLeftToRight: true, plms: plms);
            AssertTrue(!contact.Collided,
                $"boosted contact with BTS ${bts.Value:X2} admits the real PLM");
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            for (int frame = 0; frame < 100 && plms.ActiveCount != 0; frame++)
                plms.Step(guarded, level, streamer, 0, 0, 0);
            AssertEqual(0, plms.ActiveCount,
                $"BTS ${bts.Value:X2} compiled list reaches native deletion");
            AssertEqual(respawns ? RoomCollisionType.SpecialBlock :
                    RoomCollisionType.Air,
                level.GetCollisionBlockByIndex(blockIndex).CollisionType,
                $"BTS ${bts.Value:X2} retains native restoration behavior");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"BTS ${bts.Value:X2} never rereads its compiled program or draw bytes");
        }

        (TestAddressSpace bombBus, RoomLevelData bombLevel,
            RoomPlmSystem bombPlms, int bombBlock) =
            CreateSpeedBoosterBlockFixture(new RoomBlockBehavior(0x82),
                AreaId.Brinstar);
        var bombGuard = new SpeedBoosterPlmReadGuard(bombBus);
        AssertTrue(bombPlms.TrySpawnBombedSpecialBlock(bombLevel, bombBlock,
                new RoomBlockBehavior(0x82), AreaId.Brinstar, 0x0500),
            "normal bomb allocates the Speed Booster reveal PLM");
        bombPlms.Step(bombGuard, bombLevel,
            bombLevel.CreateBackgroundStreamer(), 0, 0, 0);
        AssertEqual((ushort)0xb0b6,
            bombLevel.GetCollisionBlockByIndex(bombBlock).LevelWord,
            "bombed speed block draws the native physical reveal word");
        bombPlms.Step(bombGuard, bombLevel,
            bombLevel.CreateBackgroundStreamer(), 0, 0, 0);
        AssertEqual(0, bombPlms.ActiveCount,
            "bombed speed-block reveal deletes after its single frame");
        AssertEqual(0, bombGuard.ForbiddenReadAttempts,
            "bombed Speed Booster reveal reads no compiled source bytes");

        Console.WriteLine(
            "Speed-block PLMs: 74 words/five sound bytes and one physical reveal match ROM; all five boosted contacts and bomb reveal execute with source reads forbidden.");
    }

    private sealed class SpeedBoosterPlmReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= 0xc928 && pointer < 0xc92e) ||
                 (pointer >= 0xc951 && pointer < 0xc9ba) ||
                 (pointer >= 0xc9cf && pointer < 0xc9f9) ||
                 (pointer >= 0xa4f3 && pointer < 0xa4f9) ||
                 (pointer >= 0xa345 && pointer < 0xa35d)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Speed-block PLM reread compiled source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
