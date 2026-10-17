namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One DMA sourced from a slice of a staging array. Column segments use VMAIN=$81
/// (destination +32 words); row segments use VMAIN=$80 (destination +1 word).
/// </summary>
/// <param name="SourceWords">Staging array of packed BG character words, shared with any other slices of the same produced row or column.</param>
/// <param name="SourceWordOffset">First source-array element to transfer, measured in words rather than bytes.</param>
/// <param name="WordCount">Number of consecutive source words in this segment.</param>
/// <param name="VramWordDestination">VRAM word address of the first destination character, 0 through 65535.</param>
/// <param name="Direction">Destination stride: one word across a row or 32 words down a column.</param>
public sealed record TilemapDmaSegment(
    ushort[] SourceWords,
    int SourceWordOffset,
    int WordCount,
    int VramWordDestination,
    TilemapDmaDirection Direction)
{
    /// <summary>VRAM word address of the first destination character, 0 through 65535.</summary>
    public int VramWordDestination { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, VramWordDestination);

    /// <summary>Validates the staging slice and immediately transfers its words to VRAM with the row/column stride; does not enqueue another transfer or advance camera state.</summary>
    /// <param name="vram">Destination VRAM receiving packed BG character words.</param>
    /// <exception cref="InvalidDataException">The source offset/count exceeds the staging array.</exception>
    [PpuLayoutBoundary("codec: one BG tilemap stream DMA segment (VMADD, VMAIN increment)")]
    public void ExecuteTo(SnesVram vram)
    {
        Ensure.NotNull(vram);
        if (SourceWordOffset < 0 || WordCount < 0 || SourceWordOffset + WordCount > SourceWords.Length)
            throw new InvalidDataException("Tilemap DMA segment exceeds its staging array.");

        int increment = Direction == TilemapDmaDirection.Column ? 32 : 1;
        vram.ExecuteWordTransfer(
            SourceWords.AsSpan(SourceWordOffset, WordCount),
            checked((ushort)VramWordDestination),
            increment);
    }
}
