namespace SuperMetroid.Core.Game;

/// <summary>One fixed Dead Torizo corpse-transfer descriptor; source bytes remain mutable WRAM.</summary>
internal readonly record struct DeadTorizoVramTransferDefinition(
    ushort SizeInBytes, ushort SourceBankWord, ushort SourceOffset,
    ushort EncodedVramDestination)
{
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
    private const ushort BodyDestination = 0x7060;
    /// <summary>Sand strips at $7E:9500/$9620 upload alternately to VRAM$7000/$7100.</summary>
    private const ushort SandSource = 0x9500;
    private const ushort SandDestination = 0x7000;

    internal static PhaseRows ForPhase(ushort phase) => new((phase & 1) != 0);

    /// <summary>Six body rows and one sand strip; body bounds clip unused tiles on each scanline.</summary>
    internal readonly struct PhaseRows(bool odd) : IReadOnlyList<DeadTorizoVramTransferDefinition>
    {
        public int Count => 7;
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
        public IEnumerator<DeadTorizoVramTransferDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}