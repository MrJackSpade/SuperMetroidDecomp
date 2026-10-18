namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>One immutable death-tile DMA source/destination pairing.</summary>
/// <param name="SourceAddress">Full 24-bit SNES CPU source address in bank $9B, identifying one $0400-byte death-artwork page.</param>
/// <param name="Destination">Queued VRAM destination; native death pages start at words $6000..$6800 with bit fifteen clear for one-word increments.</param>
public readonly record struct SamusDeathTileSegment(
    int SourceAddress,
    VramDmaDestination Destination);
