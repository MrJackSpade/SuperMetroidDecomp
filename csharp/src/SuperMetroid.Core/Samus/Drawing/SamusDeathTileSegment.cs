namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>One immutable death-tile DMA source/destination pairing.</summary>
/// <param name="SourceAddress">Full 24-bit SNES CPU source address in bank $9B, identifying one $0400-byte death-artwork page.</param>
/// <param name="EncodedVramDestination">Queued VRAM word address, not a byte address; native death pages use $6000..$6800 with bit fifteen clear for one-word increments.</param>
public readonly record struct SamusDeathTileSegment(
    int SourceAddress,
    ushort EncodedVramDestination);
