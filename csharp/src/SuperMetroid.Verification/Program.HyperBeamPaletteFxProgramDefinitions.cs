using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares the compiled Hyper Beam palette-FX entry, frame timers, terminators, and loop behavior with the native control words.
    /// </summary>
    /// <param name="rom">The cartridge address space used as the reference for palette-FX control data.</param>
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

    /// <summary>
    /// Reads two adjacent cartridge bytes and combines them as a little-endian Hyper Beam control word.
    /// </summary>
    /// <param name="rom">The cartridge address space containing the control bytes.</param>
    /// <param name="address">The address of the word's low byte.</param>
    /// <returns>The unsigned 16-bit control value.</returns>
    private static ushort ReadHyperBeamControlWord(
        SuperMetroidAddressSpace rom,
        int address) =>
        unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

    /// <summary>
    /// Rejects production reads of Hyper Beam palette-FX control bytes while forwarding other address-space operations.
    /// </summary>
    /// <param name="source">The underlying address space used for reads and writes outside guarded control data.</param>
    private sealed class HyperBeamPaletteFxControlReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Gets the number of attempted reads from the guarded Hyper Beam control ranges.
        /// </summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>
        /// Rejects palette-FX control reads before forwarding other byte reads to the wrapped address space.
        /// </summary>
        /// <param name="address">The bus address to inspect and read.</param>
        /// <returns>The source byte when the address is not a guarded control byte.</returns>
        public byte ReadByte(int address)
        {
            RejectControlRead(address);
            return source.ReadByte(address);
        }

        /// <summary>
        /// Applies the control-read guard to an importer read and forwards permitted cartridge accesses.
        /// </summary>
        /// <param name="address">The cartridge address requested by the importer.</param>
        /// <returns>The source cartridge byte when the address is outside the guarded control ranges.</returns>
        public byte ReadCartridgeByte(int address)
        {
            RejectControlRead(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Hyper Beam test source requires cartridge data.")).ReadCartridgeByte(address);
        }

        /// <summary>
        /// Records and rejects an address that belongs to entry, loop, timer, or terminator control data.
        /// </summary>
        /// <param name="address">The bus address being checked before a read.</param>
        private void RejectControlRead(int address)
        {
            if (IsControlByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Hyper Beam palette-FX attempted control read ${address:X6}.");
            }
        }

        /// <summary>
        /// Forwards a write to the wrapped address space without changing its address or value.
        /// </summary>
        /// <param name="address">The destination bus address.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        /// <summary>
        /// Determines whether an address falls in an entry command, loop command, frame timer, or frame terminator word.
        /// </summary>
        /// <param name="address">The bus address to classify.</param>
        /// <returns><see langword="true"/> for a control-data byte that production must not read; otherwise, <see langword="false"/>.</returns>
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
