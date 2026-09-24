using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingLogo(ISnesAddressSpace bus)
    {
        VerifyEndingLogoDefinitions(bus);
        var guarded = new EndingLogoDefinitionReadGuard(bus);
        var cgram = new SnesCgram();
        int landings = 0;
        var logo = new EndingLogo(guarded, cgram, () => landings++);
        int frame = 0, fadeStart = 0;
        var poses = new HashSet<string>();
        while (!logo.Completed && frame < 300)
        {
            logo.Step(cgram); frame++;
            if (logo.CrossfadeStarted && fadeStart == 0) fadeStart = frame;
            if (logo.PaletteStep > 0)
                for (int p = 0; p < 2; p++)
                {
                    int pointer = RomDataReader.ReadWordFixedBank(bus,
                        EndingLogoPalettePointerDefinitions.NativeTableAddress +
                        (logo.PaletteStep - 1) * 4 + p * 2);
                    for (int i = 0; i < 16; i++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(bus, 0x8c0000 | (pointer - 30 + i * 2)),
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

    private static void VerifyEndingLogoDefinitions(ISnesAddressSpace bus)
    {
        static ushort ReadWord(ISnesAddressSpace source, int address) => unchecked((ushort)(
            source.ReadByte(address) | source.ReadByte(address + 1) << 8));

        for (int index = 0; index < EndingLogoDefinitions.Actors.Length; index++)
        {
            EndingLogoActorDefinition actual = EndingLogoDefinitions.Actor(index);
            AssertEqual(EndingLogoDefinitions.Actors[index], actual.Pointer,
                $"logo actor {index} definition pointer");
            int address = EndingLogoDefinitions.NativeDefinitionBank | actual.Pointer;
            AssertEqual(ReadWord(bus, address), actual.Initialization,
                $"logo actor {index} initialization callback");
            AssertEqual(ReadWord(bus, address + 2), actual.PreInstruction,
                $"logo actor {index} pre-instruction callback");
            AssertEqual(ReadWord(bus, address + 4), actual.InstructionList,
                $"logo actor {index} initial instruction list");
        }
        for (int step = 0; step < EndingLogoDefinitions.PaletteSteps; step++)
        for (int palette = 0; palette < 2; palette++)
            AssertEqual(ReadWord(bus,
                    EndingLogoPalettePointerDefinitions.NativeTableAddress + step * 4 + palette * 2),
                EndingLogoPalettePointerDefinitions.Source(step, palette),
                $"logo fade step {step} palette {palette} source pointer");
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
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= EndingLogoPalettePointerDefinitions.NativeTableAddress &&
                address < EndingLogoPalettePointerDefinitions.NativeTableAddress +
                EndingLogoDefinitions.PaletteSteps * 2 * sizeof(ushort))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending logo reread palette pointer byte ${address:X6}.");
            }
            foreach (ushort pointer in EndingLogoDefinitions.Actors)
            {
                int start = EndingLogoDefinitions.NativeDefinitionBank | pointer;
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
