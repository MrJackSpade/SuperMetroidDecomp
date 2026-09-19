using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHyperBeamPaletteFxProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(SamusPaletteRomData.HyperBeamFx.SetColorIndex,
            ReadHyperBeamControlWord(
                rom, HyperBeamPaletteFxProgramDefinitions.NativeEntryControlAddress),
            "Hyper Beam palette-FX entry command");
        AssertEqual(SamusPaletteRomData.HyperBeamFx.DestinationByteIndex,
            ReadHyperBeamControlWord(rom,
                HyperBeamPaletteFxProgramDefinitions.NativeEntryControlAddress + 2),
            "Hyper Beam palette-FX destination");

        for (int index = 0;
             index < HyperBeamPaletteFxProgramDefinitions.FrameCount;
             index++)
        {
            int timerAddress =
                HyperBeamPaletteFxProgramDefinitions.NativeFirstFrameTimerAddress +
                index * HyperBeamPaletteFxProgramDefinitions.FrameByteCount;
            AssertEqual(HyperBeamPaletteFxProgramDefinitions.FrameDuration,
                ReadHyperBeamControlWord(rom, timerAddress),
                $"Hyper Beam palette-FX frame {index} duration");
            AssertEqual(SamusPaletteRomData.HyperBeamFx.Done,
                ReadHyperBeamControlWord(rom, timerAddress +
                    HyperBeamPaletteFxProgramDefinitions.FrameByteCount - sizeof(ushort)),
                $"Hyper Beam palette-FX frame {index} terminator");

            ushort pointer = unchecked((ushort)(
                HyperBeamPaletteFxProgramDefinitions.FirstFramePointer +
                index * HyperBeamPaletteFxProgramDefinitions.FrameByteCount));
            HyperBeamPaletteFxFrame frame =
                HyperBeamPaletteFxProgramDefinitions.ResolveFrame(pointer, out bool frameLooped);
            AssertTrue(!frameLooped, $"Hyper Beam palette-FX frame {index} is not loop entry");
            AssertEqual(index, frame.Index, $"Hyper Beam palette-FX frame {index} index");
            AssertEqual(HyperBeamPaletteFxProgramDefinitions.FrameDuration,
                frame.Duration, $"Hyper Beam palette-FX frame {index} compiled duration");
        }

        AssertEqual(SamusPaletteRomData.HyperBeamFx.Goto,
            ReadHyperBeamControlWord(
                rom, HyperBeamPaletteFxProgramDefinitions.NativeLoopControlAddress),
            "Hyper Beam palette-FX loop command");
        AssertEqual(HyperBeamPaletteFxProgramDefinitions.FirstFramePointer,
            ReadHyperBeamControlWord(
                rom, HyperBeamPaletteFxProgramDefinitions.NativeLoopControlAddress + 2),
            "Hyper Beam palette-FX loop target");

        HyperBeamPaletteFxFrame entry =
            HyperBeamPaletteFxProgramDefinitions.ResolveFrame(
                HyperBeamPaletteFxProgramDefinitions.InitialInstructionPointer,
                out bool entryLooped);
        AssertTrue(!entryLooped && entry.Index == 0,
            "Hyper Beam palette-FX entry resolves to frame zero without a completed loop");
        HyperBeamPaletteFxFrame loop =
            HyperBeamPaletteFxProgramDefinitions.ResolveFrame(
                HyperBeamPaletteFxProgramDefinitions.LoopInstructionPointer,
                out bool looped);
        AssertTrue(looped && loop.Index == 0,
            "Hyper Beam palette-FX terminal command resolves to frame zero and publishes a loop");
        AssertThrows<InvalidDataException>(
            () => HyperBeamPaletteFxProgramDefinitions.ResolveFrame(0xd905, out _),
            "Hyper Beam palette-FX rejects an unaligned restored pointer");
        AssertThrows<InvalidDataException>(
            () => HyperBeamPaletteFxProgramDefinitions.ResolveFrame(0xd9d0, out _),
            "Hyper Beam palette-FX rejects a post-program restored pointer");

        Console.WriteLine(
            "Hyper Beam palette-FX control: entry, ten timers/terminators, loop, and " +
            "restored-pointer domain match the cartridge.");
    }

    private static ushort ReadHyperBeamControlWord(
        SuperMetroidAddressSpace rom,
        int address) =>
        unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

    private sealed class HyperBeamPaletteFxControlReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (IsControlByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Hyper Beam palette-FX attempted control read ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsControlByte(int address)
        {
            if (address >= HyperBeamPaletteFxProgramDefinitions.NativeEntryControlAddress &&
                address < HyperBeamPaletteFxProgramDefinitions.NativeEntryControlAddress + 4)
            {
                return true;
            }

            if (address >= HyperBeamPaletteFxProgramDefinitions.NativeLoopControlAddress &&
                address < HyperBeamPaletteFxProgramDefinitions.NativeLoopControlAddress + 4)
            {
                return true;
            }

            int relative = address -
                HyperBeamPaletteFxProgramDefinitions.NativeFirstFrameTimerAddress;
            if (relative < 0 ||
                relative >= HyperBeamPaletteFxProgramDefinitions.FrameCount *
                    HyperBeamPaletteFxProgramDefinitions.FrameByteCount)
            {
                return false;
            }

            int inFrame = relative % HyperBeamPaletteFxProgramDefinitions.FrameByteCount;
            return inFrame < sizeof(ushort) ||
                inFrame >= HyperBeamPaletteFxProgramDefinitions.FrameByteCount - sizeof(ushort);
        }
    }
}
