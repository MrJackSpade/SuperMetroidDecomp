namespace SuperMetroid.Core.Game;

/// <summary>One seven-byte transfer record consumed by the Ceres escape graphics dispatcher.</summary>
/// <param name="Pointer">Native list pointer that identifies this transfer command.</param>
/// <param name="ByteCount">Number of source bytes copied by the command.</param>
/// <param name="SourceAddress">Cartridge address of the transfer's source data.</param>
/// <param name="DestinationWord">VRAM destination expressed as a word address.</param>
internal readonly record struct CeresEscapeVramTransferDefinition(
    ushort Pointer, ushort ByteCount, int SourceAddress, ushort DestinationWord);

/// <summary>
/// Fixed transfer metadata from the three bank-$A6 Ceres escape lists. These
/// addresses describe DMA ordering and destinations; the source pixels remain
/// presentation data, not compiled mechanics.
/// </summary>
internal static class CeresEscapeVramTransferDefinitions
{

    /// <summary>Escape timer sprite transfers beginning at $A6:C4CB.</summary>
    internal const ushort TimerSprites = 0xc4cb;

    /// <summary>Ceres escape timer BG1/BG2 and door transfers beginning at $A6:C4FE.</summary>
    internal const ushort TimerBackgrounds = 0xc4fe;

    /// <summary>Japanese-language warning overlay transfers beginning at $A6:C3B8.</summary>
    internal const ushort JapaneseOverlay = 0xc3b8;

    /// <summary>First warning-text character transfer at $A6:C4D9.</summary>
    internal const ushort WarningTextFirstTransfer = 0xc4d9;

    /// <summary>$A6:C3F4: four packed Japanese overlay tilemap rows.</summary>
    private const int OverlaySource = 0xa6c3f4;
    /// <summary>$B0:C000: timer sprite characters, two consecutive transfers.</summary>
    private const int TimerSource = 0xb0c000;
    /// <summary>$B7:DA00: warning characters reused in object and background VRAM.</summary>
    private const int WarningSource = 0xb7da00;
    /// <summary>$B0:BA00: three consecutive Ceres door character blocks.</summary>
    private const int DoorSource = 0xb0ba00;
    /// <summary>$528A: first Japanese overlay tilemap row; succeeding rows are 32 words apart.</summary>
    private const ushort OverlayDestination = 0x528a;
    /// <summary>$7E00: first timer object character word.</summary>
    private const ushort TimerDestination = 0x7e00;
    /// <summary>$7820: first warning object character word.</summary>
    private const ushort WarningObjectDestination = 0x7820;
    /// <summary>$1820: first warning background character word.</summary>
    internal const ushort WarningBackgroundDestination = 0x1820;
    /// <summary>$0D00: first door background character word.</summary>
    private const ushort DoorDestination = 0x0d00;

    /// <summary>Calculates the nineteen ordered transfer records from the native Ceres escape lists.</summary>
    private sealed class TransferRecords : IReadOnlyList<CeresEscapeVramTransferDefinition>
    {
        /// <summary>Number of transfer commands covered by the compiled Ceres escape metadata.</summary>
        public int Count => 19;

        /// <summary>Gets the transfer command at its position in native list order.</summary>
        /// <param name="index">Zero-based ordinal among the nineteen transfer commands.</param>
        /// <returns>The derived source, destination, size, and native list pointer for that command.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The ordinal is outside the transfer list.</exception>
        public CeresEscapeVramTransferDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index < 4)
                    return new((ushort)(JapaneseOverlay + 7 * index),
                        (ushort)(index < 2 ? 24 : 22),
                        OverlaySource + 24 * Math.Min(index, 2) + 22 * Math.Max(index - 2, 0),
                        (ushort)(OverlayDestination + 32 * index));
                if (index < 6)
                {
                    int block = index - 4;
                    return new((ushort)(TimerSprites + 7 * block),
                        (ushort)(block == 0 ? 0x200 : 0x120),
                        TimerSource + 0x200 * block, (ushort)(TimerDestination + 0x100 * block));
                }
                if (index < 16)
                {
                    bool background = index >= 11;
                    int block = index - (background ? 11 : 6);
                    return new((ushort)((background ? TimerBackgrounds : WarningTextFirstTransfer) + 7 * block),
                        (ushort)(block == 4 ? 0x100 : 0x200), WarningSource + 0x200 * block,
                        (ushort)((background ? WarningBackgroundDestination : WarningObjectDestination) + 0x100 * block));
                }
                int doorBlock = index - 16;
                return new((ushort)(TimerBackgrounds + 7 * (5 + doorBlock)), 0x200,
                    DoorSource + 0x200 * doorBlock, (ushort)(DoorDestination + 0x100 * doorBlock));
            }
        }
        /// <summary>Enumerates all transfer commands in their native list order.</summary>
        /// <returns>An iterator over the nineteen derived transfer records.</returns>
        public IEnumerator<CeresEscapeVramTransferDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Read-only ordered view of all compiled Ceres escape transfer commands.</summary>
    internal static readonly IReadOnlyList<CeresEscapeVramTransferDefinition> Records = new TransferRecords();

    /// <summary>Identifies native end markers that terminate one of the Ceres escape transfer lists.</summary>
    /// <param name="pointer">List pointer word to check.</param>
    /// <returns><see langword="true"/> when the word is a recognized terminator.</returns>
    internal static bool IsTerminator(ushort pointer) =>
        pointer is 0xc3d4 or 0xc4fc or 0xc536;

    /// <summary>Looks up the compiled transfer record whose native command pointer matches the supplied word.</summary>
    /// <param name="pointer">Native transfer-list pointer to search for.</param>
    /// <param name="transfer">Receives the matching record, or the default value when no record matches.</param>
    /// <returns><see langword="true"/> if a transfer command uses <paramref name="pointer"/>.</returns>
    internal static bool TryGet(ushort pointer,
        out CeresEscapeVramTransferDefinition transfer)
    {
        foreach (CeresEscapeVramTransferDefinition candidate in Records)
        {
            if (candidate.Pointer == pointer)
            {
                transfer = candidate;
                return true;
            }
        }
        transfer = default;
        return false;
    }
}
