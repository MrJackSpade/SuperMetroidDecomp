using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyMaridiaElevatubePlm()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort start = MaridiaElevatubePlmDefinitions.InstructionList;
        foreach (ushort address in new ushort[] { start, (ushort)(start + 2),
                     (ushort)(start + 4), (ushort)(start + 7) })
        {
            AssertTrue(MaridiaElevatubePlmDefinitions.TryReadMechanicsWord(
                    address, out ushort compiled),
                $"elevatube control word $84:{address:X4} is compiled");
            AssertEqual((ushort)(rom.ReadByte(0x840000 | address) |
                    rom.ReadByte(0x840000 | (address + 1)) << 8),
                compiled, $"elevatube control word $84:{address:X4} matches ROM");
        }
        AssertTrue(MaridiaElevatubePlmDefinitions.TryReadMechanicsByte(
                (ushort)(start + 6), out byte sound),
            "elevatube sound operand is compiled");
        AssertEqual(rom.ReadByte(0x840000 | (start + 6)), sound,
            "elevatube sound operand matches ROM");

        RoomPlmShotBlockDrawDefinitions.DrawList draw =
            MaridiaElevatubePlmDefinitions.Draw;
        AssertEqual((ushort)1, draw.Runs.Span[0].DirectionAndCount,
            "elevatube physical draw contains one horizontal block");
        AssertEqual((ushort)0x8180, draw.Runs.Span[0].LevelWords.Span[0],
            "elevatube physical block word matches ROM");
        byte[] drawBytes = [1, 0, 0x80, 0x81, 0, 0];
        for (int offset = 0; offset < drawBytes.Length; offset++)
            AssertEqual(rom.ReadByte(0x840000 | (draw.Pointer + offset)),
                drawBytes[offset], $"elevatube physical draw byte {offset} matches ROM");

        RoomLevelData level = CreateRoom(4, 4, new ushort[16], new byte[16],
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnMaridiaElevatube(level),
            "native door setup allocates the elevatube PLM");
        var guarded = new MaridiaElevatubeSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int blockIndex = level.GetBlockIndex(
            MaridiaElevatubePlmRomData.BlockX,
            MaridiaElevatubePlmRomData.BlockY);
        int heardAtFrame = -1;
        for (int frame = 0; frame < 24 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(guarded, level, streamer, 0, 0, 0);
            if (frame == 0)
                AssertEqual((ushort)0x8180,
                    level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                    "first elevatube handler pass draws its physical block");
            if (plms.SoundRequests.Any(request =>
                    request.SoundEffect == SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        MaridiaElevatubePlmDefinitions.SoundId)))
                heardAtFrame = frame;
        }
        AssertEqual(16, heardAtFrame,
            "elevatube sound follows the native sixteen-frame hold");
        AssertEqual(0, plms.ActiveCount,
            "elevatube PLM deletes after the sound instruction");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "elevatube program and draw execute without their source ROM bytes");
        Console.WriteLine(
            "Maridia elevatube PLM: native delay, draw, sound, and delete pass with source reads forbidden.");
    }

    private sealed class MaridiaElevatubeSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= MaridiaElevatubePlmDefinitions.InstructionList &&
                  pointer < MaridiaElevatubePlmDefinitions.InstructionList + 9) ||
                 (pointer >= MaridiaElevatubePlmDefinitions.DrawPointer &&
                  pointer < MaridiaElevatubePlmDefinitions.DrawPointer + 6)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Elevatube PLM reread compiled source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
