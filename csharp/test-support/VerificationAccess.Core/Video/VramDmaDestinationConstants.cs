using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Cartridge values and helpers of <see cref="VramDmaDestination"/> that only verification reads.</summary>
internal static class VramDmaDestinationConstants
{
    /// <summary>The destination <paramref name="wordOffset"/> words into the VRAM described by <paramref name="cell"/>.</summary>
    internal static VramDmaDestination At(PpuLayoutCell cell, int wordOffset = 0, VramIncrement increment = VramIncrement.Row)
    {
        Ensure.NotNull(cell);
        if (cell.Region != PpuLayoutRegion.Vram)
            throw new ArgumentException($"{cell} is not a VRAM region.", nameof(cell));
        if ((uint)wordOffset >= (uint)cell.Count)
            throw new ArgumentOutOfRangeException(nameof(wordOffset), wordOffset, $"{cell.Name} holds {cell.Count} words.");
        return At(cell.Offset + wordOffset, increment);
    }
    /// <summary>The destination at physical VMADD <paramref name="word"/>.</summary>
    internal static VramDmaDestination At(int word, VramIncrement increment) =>
        (uint)word <= VramDmaDestination.WordMask
            ? new((ushort)(word | (increment == VramIncrement.Column ? VramDmaDestination.ColumnFlag : 0)))
            : throw new ArgumentOutOfRangeException(nameof(word), word, "VMADD holds 15 bits.");
}
