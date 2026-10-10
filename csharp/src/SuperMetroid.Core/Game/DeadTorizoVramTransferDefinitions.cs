namespace SuperMetroid.Core.Game;

/// <summary>One fixed Dead Torizo corpse-transfer descriptor; source bytes remain mutable WRAM.</summary>
/// <param name="SizeInBytes">Number of source bytes copied by the DMA descriptor.</param>
/// <param name="SourceBankWord">Bank word used with the source offset to form the 24-bit source address.</param>
/// <param name="SourceOffset">Offset within the source bank of the mutable staged bytes.</param>
/// <param name="EncodedVramDestination">Destination word as encoded by the native transfer table.</param>
internal readonly record struct DeadTorizoVramTransferDefinition(
    ushort SizeInBytes, ushort SourceBankWord, ushort SourceOffset,
    ushort EncodedVramDestination)
{
    /// <summary>Combines the source bank and offset into the 24-bit address read by the transfer.</summary>
    internal int SourceAddress => ((SourceBankWord & 0xff00) << 8) | SourceOffset;
}

/// <summary>
/// The alternating bank-$A9 corpse VRAM queues at $A9:D549 and $A9:D583.
/// These immutable descriptor words select mutable $7E staging bytes; the
/// rotting algorithm, transfer phase, and pixels are not embedded here.
/// </summary>
internal static class DeadTorizoVramTransferDefinitions
{

    /// <summary>Twelve ten-tile body rows start at $7E:2000 and VRAM word$7060.</summary>
    private const ushort BodySource = 0x2000;
    /// <summary>Starting VRAM word for the first body tile row in the corpse upload pattern.</summary>
    private const ushort BodyDestination = 0x7060;
    /// <summary>Sand strips at $7E:9500/$9620 upload alternately to VRAM$7000/$7100.</summary>
    private const ushort SandSource = 0x9500;
    /// <summary>Starting VRAM word for the first alternating sand-strip upload.</summary>
    private const ushort SandDestination = 0x7000;

    /// <summary>Selects the seven body-row and sand-strip transfers for a corpse-update phase.</summary>
    /// <param name="phase">Native phase counter; its low bit selects which half of the body rows is uploaded.</param>
    /// <returns>An allocation-free ordered view of that phase's transfer descriptors.</returns>
    internal static PhaseRows ForPhase(ushort phase) => new((phase & 1) != 0);

    /// <summary>Six body rows and one sand strip; body bounds clip unused tiles on each scanline.</summary>
    /// <param name="odd">Selects the alternate set of six body rows and corresponding sand-strip source and destination.</param>
    internal readonly struct PhaseRows(bool odd) : IReadOnlyList<DeadTorizoVramTransferDefinition>
    {
        /// <summary>Number of descriptors emitted for every phase: six body rows and one sand strip.</summary>
        public int Count => 7;

        /// <summary>Gets the transfer descriptor at its fixed position in the phase's ordered upload sequence.</summary>
        /// <param name="index">Descriptor index from zero through six, with the sand strip last.</param>
        /// <returns>The body-row or sand-strip DMA parameters for this phase.</returns>
        /// <exception cref="IndexOutOfRangeException">The index is outside the seven descriptors.</exception>
        public DeadTorizoVramTransferDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (index == 6)
                    return new(odd ? (ushort)0x100 : (ushort)0x120, 0x7e00,
                        (ushort)(SandSource + (odd ? 0 : 0x120)),
                        (ushort)(SandDestination + (odd ? 0 : 0x100)));
                int row = index + (odd ? 6 : 0);
                int firstTile = row < 2 ? 3 : Math.Clamp(10 - row, 0, 2);
                int tileCount = row < 2 ? 6 : 10 - firstTile;
                return new((ushort)(32 * tileCount), 0x7e00,
                    (ushort)(BodySource + row * 320 + firstTile * 32),
                    (ushort)(BodyDestination + row * 256 + firstTile * 16));
            }
        }
        /// <summary>Enumerates this phase's six body-row transfers followed by its sand strip.</summary>
        /// <returns>An enumerator over the descriptors in upload order.</returns>
        public IEnumerator<DeadTorizoVramTransferDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
