namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The alternating bank-$A9 corpse VRAM queues at $A9:D549 and $A9:D583.
/// These immutable descriptor words select mutable $7E staging bytes; the
/// rotting algorithm, transfer phase, and pixels are not embedded here.
/// </summary>
internal static class DeadTorizoVramTransferDefinitions
{

    /// <summary>Twelve ten-tile body rows start at $7E:2000 and VRAM word$7060.</summary>
    private const ushort BodySource = 0x2000;
    /// <summary>First destination word for the clipped body rows in VRAM.</summary>
    private const ushort BodyDestination = 0x7060;
    /// <summary>Sand strips at $7E:9500/$9620 upload alternately to VRAM$7000/$7100.</summary>
    private const ushort SandSource = 0x9500;
    /// <summary>First VRAM destination word for the alternating sand-strip upload.</summary>
    private const ushort SandDestination = 0x7000;

    /// <summary>Selects the even or odd corpse-transfer half without copying the mutable WRAM source pixels.</summary>
    internal static PhaseRows ForPhase(ushort phase) => new((phase & 1) != 0);

    /// <summary>Six body rows and one sand strip; body bounds clip unused tiles on each scanline.</summary>
    internal readonly struct PhaseRows(bool odd) : IReadOnlyList<DeadTorizoVramTransferDefinition>
    {
        /// <summary>Six clipped body-row transfers followed by one sand-strip transfer.</summary>
        public int Count => 7;
        /// <summary>Calculates the phase's row-specific source, destination, and byte count, including the alternating sand strip.</summary>
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
        /// <summary>Yields body rows in increasing row order, then the phase's sand strip.</summary>
        public IEnumerator<DeadTorizoVramTransferDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Preserves DMA descriptor order for non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
