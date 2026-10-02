using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static (ushort Start, int Frames, ushort End)[] SpeedBlockNativeLayouts() =>
        [(0xc951,7,0xc974),(0xc974,7,0xc997),(0xc997,7,0xc9ba),(0xc9cf,4,0xc9e4),(0xc9e4,4,0xc9f9)];

    private static void VerifySpeedBlockControlMapping(SuperMetroidAddressSpace rom)
    {
        var controls = new HashSet<ushort> {0xc928,0xc92c};
        var ordered = new List<ushort> {0xc928,0xc92a,0xc92c};
        foreach (var layout in SpeedBlockNativeLayouts())
        {
            controls.Add(layout.Start);
            ordered.Add(layout.Start);
            int cursor = layout.Start + 3;
            for (int frame = 0; frame < layout.Frames; frame++, cursor += 4)
            {
                controls.Add((ushort)cursor);
                ordered.Add((ushort)cursor);
                ordered.Add((ushort)(cursor + 2));
            }
            for (; cursor < layout.End; cursor += 2)
            {
                controls.Add((ushort)cursor);
                ordered.Add((ushort)cursor);
            }
        }
        AssertEqual(74,ordered.Count,"speed native word extent");
        AssertTrue(ordered.SequenceEqual(SpeedBoosterBlockPlmProgramDefinitions.MechanicsWordAddresses()),"speed complete native word enumeration");
        var known = ordered.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = SpeedBoosterBlockPlmProgramDefinitions.TryReadMechanicsWord((ushort)address,out ushort value);
            AssertEqual(known.Contains((ushort)address),found,"speed complete word domain including unused Screw Attack gaps");
            if (!found) AssertEqual((ushort)0,value,"speed missing word cleared");
            else if (controls.Contains((ushort)address))
                AssertEqual(ReadBotwoonInstructionWord(rom,0x840000 | address),value,"speed native delays, queue, clone/ordinary restore and deletion");
        }
    }

    private static void VerifySpeedBlockDrawOperandMapping(SuperMetroidAddressSpace rom)
    {
        var addresses = new List<ushort> {0xc92a};
        foreach (var layout in SpeedBlockNativeLayouts())
        for (int frame = 0; frame < layout.Frames; frame++)
            addresses.Add((ushort)(layout.Start + 5 + frame * 4));
        AssertEqual(30,addresses.Count,"speed native draw extent");
        foreach (ushort address in addresses)
        {
            AssertTrue(SpeedBoosterBlockPlmProgramDefinitions.TryReadMechanicsWord(address,out ushort value),"speed native draw operand owned");
            AssertEqual(ReadBotwoonInstructionWord(rom,0x840000 | address),value,"speed native forward/reverse breakup and bomb reveal selection");
        }
    }

    private static void VerifySpeedBlockSoundMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xc953,0xc976,0xc999,0xc9d1,0xc9e6];
        AssertTrue(addresses.SequenceEqual(SpeedBoosterBlockPlmProgramDefinitions.MechanicsByteAddresses()),"speed native sound enumeration");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = SpeedBoosterBlockPlmProgramDefinitions.TryReadMechanicsByte((ushort)address,out byte value);
            AssertEqual(addresses.Contains((ushort)address),found,"speed complete packed sound domain");
            AssertEqual(found ? rom.ReadByte(0x840000 | address) : (byte)0,value,"speed native sound byte and missing output");
        }
    }

    private static void VerifyCompiledSpeedBoosterPlmPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        VerifySpeedBlockControlMapping(rom);
        VerifySpeedBlockDrawOperandMapping(rom);
        VerifySpeedBlockSoundMapping(rom);

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
        : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

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
