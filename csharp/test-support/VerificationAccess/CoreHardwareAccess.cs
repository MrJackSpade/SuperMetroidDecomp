using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="NativeWordCounter"/> members production does not use.</summary>
internal static class NativeWordCounterAccess
{
    extension(NativeWordCounter)
    {
        /// <summary>
        /// Decrements a nonzero word and leaves zero saturated, matching the common
        /// <c>LDA timer / BEQ done / DEC timer</c> pattern rather than unconditional DEC.
        /// </summary>
        internal static ushort DecrementSaturating(ushort value) =>
            value == 0 ? (ushort)0 : unchecked((ushort)(value - 1));
    }
}

/// <summary>Verification access to <see cref="NativeWordCounterStep"/> members production does not use.</summary>
internal static class NativeWordCounterStepAccess
{
    extension(NativeWordCounterStep self)
    {
        /// <summary>Whether decrementing zero wrapped through <c>$FFFF</c>.</summary>
        internal bool Underflowed => self.Value == ushort.MaxValue;
    }
}

/// <summary>Verification access to <see cref="SnesAddress"/> members production does not use.</summary>
internal static class SnesAddressAccess
{
    extension(SnesAddress)
    {
        /// <summary>
        /// Creates an address that must refer to an upper LoROM cartridge window.
        /// </summary>
        internal static SnesAddress FromUpperLoRom(byte bank, ushort offset)
        {
            var address = new SnesAddress(bank, offset);
            if (!address.IsUpperLoRomWindow)
            {
                throw new ArgumentOutOfRangeException(
                    "offset",
                    offset,
                    "An upper LoROM cartridge address must be in the $8000-$FFFF window.");
            }
            return address;
        }
    }
}

/// <summary>Verification access to <see cref="SnesAngle"/> members production does not use.</summary>
internal static class SnesAngleAccess
{
    extension(SnesAngle self)
    {
        /// <summary>The byte offset for a table containing one 16-bit sample per angle.</summary>
        internal int SineTableByteOffset => self.TableIndex * sizeof(ushort);

        /// <summary>
        /// Returns the native signed shortest delta from this angle to <paramref name="target"/>.
        /// The exactly-opposite case is <see cref="short.MinValue"/>, matching 65816 subtraction.
        /// </summary>
        internal short SignedDeltaTo(SnesAngle target) =>
            unchecked((short)(target.RawValue - self.RawValue));

        /// <summary>
        /// Returns the signed high-byte delta used by byte-angle dispatchers. The opposite
        /// direction is <see cref="sbyte.MinValue"/>, matching native eight-bit truncation.
        /// </summary>
        internal sbyte SignedTableDeltaTo(SnesAngle target) =>
            unchecked((sbyte)(target.TableIndex - self.TableIndex));
    }
}

/// <summary>Verification access to <see cref="SnesBgTilemapWord"/> members production does not use.</summary>
internal static class SnesBgTilemapWordAccess
{
    extension(SnesBgTilemapWord self)
    {
        /// <summary>Replaces only the priority bit.</summary>
        internal SnesBgTilemapWord WithPriority(bool priority) =>
            new(unchecked((ushort)(priority ? self.Raw | PrivateState.StaticField<ushort>(typeof(SnesBgTilemapWord), "PriorityMask") : self.Raw & ~PrivateState.StaticField<ushort>(typeof(SnesBgTilemapWord), "PriorityMask"))));
    }
}

/// <summary>Verification access to <see cref="SnesCgram"/> members production does not use.</summary>
internal static class SnesCgramAccess
{
    extension(SnesCgram self)
    {
        /// <summary>Clears all 256 colors to black.</summary>
        internal void Clear() => Array.Clear(PrivateState.Field<ushort[]>(self, "_colors"));
    }
}

/// <summary>Verification access to <see cref="SnesSignedEightEight"/> members production does not use.</summary>
internal static class SnesSignedEightEightAccess
{
    extension(SnesSignedEightEight)
    {
        /// <summary>Creates an exact whole-pixel velocity and rejects values outside 8.8 range.</summary>
        internal static SnesSignedEightEight FromWholePixels(int pixels) =>
            new(unchecked((ushort)(checked((sbyte)pixels) << 8)));
    }

    extension(SnesSignedEightEight self)
    {
        /// <summary>Signed whole-pixel byte.</summary>
        internal sbyte WholePixels => unchecked((sbyte)(self.RawValue >> 8));

        /// <summary>Unsigned 1/256-pixel fraction byte.</summary>
        internal byte Fraction => unchecked((byte)self.RawValue);

        /// <summary>Adds raw 1/256-pixel units with native 16-bit wraparound.</summary>
        internal SnesSignedEightEight AddRawWrapping(int rawDelta) =>
            new(unchecked((ushort)(self.RawValue + rawDelta)));
    }
}

/// <summary>Verification access to <see cref="SnesSignedSixteenSixteen"/> members production does not use.</summary>
internal static class SnesSignedSixteenSixteenAccess
{
    extension(SnesSignedSixteenSixteen)
    {
        /// <summary>Creates an exact whole-pixel delta with checked signed-word conversion.</summary>
        internal static SnesSignedSixteenSixteen FromWholePixels(int pixels) =>
            SnesSignedSixteenSixteen.FromParts(checked((short)pixels), 0);
    }
}

/// <summary>Verification access to <see cref="SnesVram"/> members production does not use.</summary>
internal static class SnesVramAccess
{
    extension(SnesVram self)
    {
        /// <summary>
        /// Executes a literal DMA channel transfer, including the SNES DAS-zero convention.
        /// </summary>
        /// <param name="memory">Live WRAM/SRAM from which the DMA channel reads; never a cartridge source.</param>
        /// <param name="sourceAddress">Fixed source bank plus initial 16-bit offset.</param>
        /// <param name="dmaSize">
        /// Raw 16-bit DAS register. Values one through <c>$FFFF</c> transfer that many bytes;
        /// zero transfers <c>$10000</c> bytes because the channel decrements through the full
        /// sixteen-bit counter before reaching zero again.
        /// </param>
        /// <param name="encodedDestination">
        /// Initial VMADD word plus this model's VMAIN-column marker in bit 15.
        /// </param>
        internal void ExecuteHardwareMemoryDmaWrite(
            ISnesMutableMemory memory,
            int sourceAddress,
            ushort dmaSize,
            ushort encodedDestination)
        {
            ArgumentNullException.ThrowIfNull(memory);
            if ((uint)sourceAddress > 0x00ff_ffff)
                throw new ArgumentOutOfRangeException(nameof(sourceAddress), sourceAddress, "DMA source must be a 24-bit CPU address.");

            int effectiveSize = dmaSize == 0 ? 0x10000 : dmaSize;
            PrivateState.Invoke(self, "ExecuteDmaWrite", (ISnesMutableMemory)(memory), (SnesAddress)(SnesAddress.FromBusAddress(sourceAddress)), (int)(effectiveSize), (ushort)(encodedDestination));
        }
    }
}
