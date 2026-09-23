using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Compares every compiled room-FX animation control word to the pinned cartridge,
    /// then runs every complete loop while rejecting reads of those mechanics bytes.
    /// </summary>
    private static void VerifyRoomFxAnimatedTileMechanicsDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Room-FX animated-tile mechanics: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int mechanicsWordCount = 0;
        int frameCount = 0;
        foreach (RoomFxAnimatedTileObjectDefinition definition in
                 RoomFxAnimatedTileMechanicsDefinitions.All)
        {
            VerifyMechanicsWord(definition, bus, definition.ObjectPointer,
                definition.InstructionPointer, "instruction-list pointer");
            VerifyMechanicsWord(definition, bus,
                unchecked((ushort)(definition.ObjectPointer + 2)),
                definition.TransferByteCount, "transfer byte count");
            VerifyMechanicsWord(definition, bus,
                unchecked((ushort)(definition.ObjectPointer + 4)),
                definition.EncodedVramDestination, "VRAM destination");
            mechanicsWordCount += 3;

            foreach (RoomFxAnimatedTileFrameDefinition frame in definition.Frames)
            {
                VerifyMechanicsWord(definition, bus, frame.InstructionPointer,
                    frame.Duration, "frame duration");
                AssertTrue(!definition.TryReadMechanicsWord(
                        frame.SourceOperandPointer, out _),
                    $"object $87:{definition.ObjectPointer:X4} leaves source operand " +
                    $"$87:{frame.SourceOperandPointer:X4} presentation-owned");
                int sourceAddress = RoomFxAnimatedTileArtworkDefinitions.SourceAddress(
                    definition, frame.InstructionPointer);
                AssertEqual((ushort)sourceAddress,
                    RomDataReader.ReadWordFixedBank(bus,
                        RoomFxRomData.Banks.AnimatedTiles | frame.SourceOperandPointer),
                    $"object $87:{definition.ObjectPointer:X4} compiled artwork-source identity");
                mechanicsWordCount++;
                frameCount++;
            }

            VerifyMechanicsWord(definition, bus, definition.GotoInstructionPointer,
                AnimatedTileInstructionCodes.Goto, "loop opcode");
            VerifyMechanicsWord(definition, bus,
                unchecked((ushort)(definition.GotoInstructionPointer + 2)),
                definition.InstructionPointer, "loop target");
            mechanicsWordCount += 2;

            var guarded = new RoomFxAnimatedTileMechanicsForbiddenBus(bus, definition);
            var state = new RoomFxAnimatedTilesState();
            var vram = new SnesVram();
            var writes = new VramWriteQueue();
            state.LoadDefinition(guarded, definition.ObjectPointer);

            int steps = definition.Frames.Sum(frame => frame.Duration) + 1;
            int observedFrames = 0;
            for (int step = 0; step < steps; step++)
            {
                state.Step(guarded, vram, writes);
                if (state.LastSourceAddress.HasValue)
                    observedFrames++;
            }

            AssertEqual(definition.Frames.Count + 1, observedFrames,
                $"object $87:{definition.ObjectPointer:X4} executes one complete loop");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"object $87:{definition.ObjectPointer:X4} performs no mechanics ROM reads");
            AssertEqual(0, guarded.PresentationReadCount,
                $"object $87:{definition.ObjectPointer:X4} reads no compiled artwork-source operands");
        }

        AssertEqual(5, RoomFxAnimatedTileMechanicsDefinitions.All.Count,
            "simple room-FX animated-tile object count");
        AssertEqual(23, frameCount, "simple room-FX animated-tile frame count");
        AssertEqual(48, mechanicsWordCount,
            "simple room-FX animated-tile compiled mechanics word count");
        Console.WriteLine(
            "  Room-FX animated tiles: 48 control words and 23 artwork-source identities across 5 objects are compiled.");
    }

    private static void VerifyMechanicsWord(
        RoomFxAnimatedTileObjectDefinition definition,
        ISnesAddressSpace bus,
        ushort pointer,
        ushort expected,
        string label)
    {
        AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort actual),
            $"object $87:{definition.ObjectPointer:X4} catalogs {label} $87:{pointer:X4}");
        AssertEqual(expected, actual,
            $"object $87:{definition.ObjectPointer:X4} compiled {label}");
        AssertEqual(expected,
            RomDataReader.ReadWordFixedBank(
                bus,
                RoomFxRomData.Banks.AnimatedTiles | pointer),
            $"object $87:{definition.ObjectPointer:X4} cartridge {label}");
    }

    private sealed class RoomFxAnimatedTileMechanicsForbiddenBus(
        ISnesAddressSpace inner,
        RoomFxAnimatedTileObjectDefinition definition) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }
        public int PresentationReadCount { get; private set; }

        public byte ReadByte(int address)
        {
            SnesAddress source = SnesAddress.FromBusAddress(address);
            if (source.Bank == (byte)(RoomFxRomData.Banks.AnimatedTiles >> 16))
            {
                if (definition.TryReadMechanicsWord(source.Offset, out _) ||
                    definition.TryReadMechanicsWord(
                        unchecked((ushort)(source.Offset - 1)), out _))
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Production read compiled room-FX mechanics byte {source}.");
                }

                if (definition.Frames.Any(frame =>
                        source.Offset == frame.SourceOperandPointer ||
                        source.Offset == unchecked((ushort)(frame.SourceOperandPointer + 1))))
                {
                    PresentationReadCount++;
                }
            }

            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
