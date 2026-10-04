using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingLogo(ISnesAddressSpace bus)
    {
        VerifyEndingLogoDefinitions(bus);
        VerifyEndingLogoPrograms(bus);
        var guarded = new EndingLogoDefinitionReadGuard(bus);
        var cgram = new SnesCgram();
        var nativeCgram = new SnesCgram();
        int landings = 0;
        var logo = new EndingLogo(guarded, cgram, () => landings++);
        var nativeLogo = new EndingLogo(bus, nativeCgram, () => { });
        int frame = 0, fadeStart = 0;
        var poses = new HashSet<string>();
        while (!logo.Completed && frame < 300)
        {
            nativeLogo.Step(nativeCgram);
            logo.Step(cgram, EndingLogoInstructionDefinitions.ReadWord);
            frame++;
            AssertEqual(nativeLogo.Completed, logo.Completed,
                $"compiled logo lifetime at frame {frame}");
            AssertEqual(nativeLogo.CrossfadeStarted, logo.CrossfadeStarted,
                $"compiled logo palette handoff at frame {frame}");
            AssertEqual(nativeLogo.PaletteStep, logo.PaletteStep,
                $"compiled logo palette step at frame {frame}");
            AssertTrue(nativeCgram.Colors.SequenceEqual(cgram.Colors) &&
                    nativeLogo.Draw().LowTable.SequenceEqual(logo.Draw().LowTable) &&
                    nativeLogo.Draw().HighTable.SequenceEqual(logo.Draw().HighTable),
                $"compiled logo palette and OAM at frame {frame}");
            if (logo.CrossfadeStarted && fadeStart == 0) fadeStart = frame;
            if (logo.PaletteStep > 0)
                for (int p = 0; p < 2; p++)
                {
                    int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                        EndingLogoPalettePointerDefinitions.NativeTableAddress +
                        (logo.PaletteStep - 1) * 4 + p * 2);
                    for (int i = 0; i < 16; i++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8c0000 | (pointer - 30 + i * 2)),
                            cgram.Colors[(p == 0 ? 16 : 240) + i], "native logo crossfade palette table entry");
                }
            poses.Add(Convert.ToHexString(logo.Draw().LowTable));
        }
        AssertTrue(logo.Completed, "logo actors reach the final palette handoff");
        AssertEqual(171, fadeStart, "native circle list waits 96+5+5+64 frames before grey-out instruction");
        AssertEqual(187, frame, "logo palette handoff follows sixteen function calls");
        AssertEqual(1, landings, "upper logo half spawns its palette FX exactly once on landing");
        AssertTrue(poses.Count >= 10, "logo approach and circle animation produce changing OAM positions/maps");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "ending logo never rereads compiled actor definitions");
        Console.WriteLine($"  Logo: {fadeStart} actor frames, sixteen exact palette pairs, {poses.Count} OAM poses.");
    }

    private static void VerifyEndingLogoPrograms(ISnesAddressSpace bus)
    {
        ushort[] operands = [0xee5f, 0xee67, 0xee73, 0xee77, 0xee7b, 0xee8d, 0xee91, 0xee95];
        string[] names = ["s-upper", "s-lower", "circle-right-1", "circle-right-2", "circle-right-3",
            "circle-left-1", "circle-left-2", "circle-left-3"];
        var catalog = EndingLogoSpriteDefinitions.Frames;
        AssertEqual(8, catalog.Count, "logo frame count");
        for (int i = 0; i < operands.Length; i++)
        {
            int operand = 0x8b0000 | operands[i];
            ushort pointer = (ushort)(bus.ReadByte(operand) | bus.ReadByte(operand + 1) << 8);
            int header = 0x8c0000 | pointer;
            int count = bus.ReadByte(header) | bus.ReadByte(header + 1) << 8;
            AssertEqual(pointer, catalog[i].Pointer, "logo frame address from original instruction operand");
            AssertEqual(count, catalog[i].StockPartCount, "logo part count from original OAM header");
            AssertEqual(names[i], catalog[i].Name, "logo published asset key");
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "logo catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => _ = catalog[invalid], "logo catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoSpriteDefinitions.FramePointer(invalid), "logo frame address bounds");
        }
        for (int pointer = EndingLogoInstructionDefinitions.Start;
             pointer < EndingLogoInstructionDefinitions.End; pointer += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x8b0000 | pointer),
                EndingLogoInstructionDefinitions.ReadWord((ushort)pointer),
                $"ending logo instruction $8B:{pointer:X4} matches cartridge");
        AssertThrows<InvalidDataException>(() =>
            EndingLogoInstructionDefinitions.ReadWord(EndingLogoInstructionDefinitions.End),
            "logo instruction reader rejects the following definition table");
        AssertThrows<InvalidDataException>(() =>
            EndingLogoInstructionDefinitions.ReadWord(
                unchecked((ushort)(EndingLogoInstructionDefinitions.Start + 1))),
            "logo instruction reader rejects an unaligned address");
        foreach (ushort start in new ushort[] { 0xee5d, 0xee65, 0xee6d, 0xee87 })
        {
            var native = new IntroDiscoverySprite(0, 0, 0, start);
            var generated = new IntroDiscoverySprite(0, 0, 0, start);
            for (int frame = 0; frame < 200; frame++)
            {
                ushort nativeCallback = 0, generatedCallback = 0;
                native.Step(bus, (opcode, cursor) => { nativeCallback = opcode; return cursor; },
                    pointer => (ushort)(bus.ReadByte(0x8b0000 | pointer) | bus.ReadByte(0x8b0000 | (pointer + 1)) << 8));
                generated.Step(bus, (opcode, cursor) => { generatedCallback = opcode; return cursor; },
                    EndingLogoInstructionDefinitions.ReadWord);
                AssertEqual(native.InstructionPointer, generated.InstructionPointer, "logo program cursor");
                AssertEqual(native.SpriteMapPointer, generated.SpriteMapPointer, "logo program frame");
                AssertEqual(nativeCallback, generatedCallback, "logo callback timing");
            }
        }
        foreach (ushort invalid in new ushort[] { 0, 0xee5c, 0xee9b, 0xffff })
            AssertThrows<InvalidDataException>(() => EndingLogoInstructionDefinitions.ReadWord(invalid), "logo program bounds");
    }

    private static void VerifyEndingLogoDefinitions(ISnesAddressSpace bus)
    {
        static ushort ReadWord(ISnesAddressSpace source, int address) => unchecked((ushort)(
            source.ReadByte(address) | source.ReadByte(address + 1) << 8));

        for (int index = 0; index < EndingLogoDefinitions.ActorCount; index++)
        {
            EndingLogoActorDefinition actual = EndingLogoDefinitions.Actor(index);
            int spawn = 0x8be554 + index * 6;
            AssertEqual((byte)0xa0, bus.ReadByte(spawn), "native LDY actor definition" );
            ushort nativePointer = ReadWord(bus, spawn + 1);
            AssertEqual(nativePointer, EndingLogoDefinitions.ActorPointer(index), "calculated actor pointer" );
            AssertEqual(nativePointer, actual.Pointer,
                $"logo actor {index} definition pointer");
            int address = EndingLogoDefinitions.NativeDefinitionBank | nativePointer;
            AssertEqual(ReadWord(bus, address), actual.Initialization,
                $"logo actor {index} initialization callback");
            AssertEqual(ReadWord(bus, address + 2), actual.PreInstruction,
                $"logo actor {index} pre-instruction callback");
            AssertEqual(ReadWord(bus, address + 4), actual.InstructionList,
                $"logo actor {index} initial instruction list");
            int initialize = 0x8b0000 | ReadWord(bus, address);
            AssertEqual((byte)0xa9, bus.ReadByte(initialize), "native LDA X origin");
            AssertEqual((byte)0xa9, bus.ReadByte(initialize + 6), "native LDA Y origin");
            var origin = EndingLogoDefinitions.Origin(index);
            AssertEqual(ReadWord(bus, initialize + 1), origin.X, "native actor X origin");
            AssertEqual(ReadWord(bus, initialize + 7), origin.Y, "native actor Y origin");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoDefinitions.ActorPointer(invalid), "actor pointer bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoDefinitions.Actor(invalid), "actor definition bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoDefinitions.Origin(invalid), "actor origin bounds");
        }
        VerifyEndingLogoPaletteSources(bus);
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingLogoDefinitions.Actor(4),
            "logo actor definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingLogoPalettePointerDefinitions.Source(EndingLogoDefinitions.PaletteSteps, 0),
            "logo palette step boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingLogoPalettePointerDefinitions.Source(0, 2),
            "logo palette selector boundary");
        Console.WriteLine(
            "  Logo definitions: twelve actor words and 32 palette pointers match the cartridge.");
    }

    private sealed class EndingLogoDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address >= 0x8b0000 + EndingLogoInstructionDefinitions.Start &&
                address < 0x8b0000 + EndingLogoInstructionDefinitions.End)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending logo reread compiled instruction byte ${address:X6}.");
            }
            if (address >= EndingLogoPalettePointerDefinitions.NativeTableAddress &&
                address < EndingLogoPalettePointerDefinitions.NativeTableAddress +
                EndingLogoDefinitions.PaletteSteps * 2 * sizeof(ushort))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending logo reread palette pointer byte ${address:X6}.");
            }
            for (int index = 0; index < EndingLogoDefinitions.ActorCount; index++)
            {
                int start = EndingLogoDefinitions.NativeDefinitionBank | EndingLogoDefinitions.ActorPointer(index);
                if (address >= start && address < start + 3 * sizeof(ushort))
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Ending logo reread definition byte ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
