namespace SuperMetroid.Core.Rooms;

/// <summary>One completed WRAM staging buffer and its ordered NMI DMA operations.</summary>
/// <param name="Segments">Transfer descriptors in native producer/consumer order, including unwrapped and wrapped slices; creating the record does not execute them.</param>
public sealed record TilemapStreamUpdate(
    IReadOnlyList<TilemapDmaSegment> Segments)
{
    /// <summary>
    /// Executes the same ordered transfers that <c>$80:8CD8/$80:8DAC</c> perform after
    /// observing the corresponding update flag during NMI.
    /// </summary>
    public void ExecuteTo(SnesVram vram)
    {
        Ensure.NotNull(vram);
        foreach (TilemapDmaSegment segment in Segments)
            segment.ExecuteTo(vram);
    }
}
