namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>One compiled source segment in the installed room-FX artwork strip.</summary>
/// <param name="SourceAddress">Full native SNES artwork identity, used to resolve installed frame transfers without cartridge reads.</param>
/// <param name="ByteCount">Number of ordered 2bpp planar bytes contributed to the PNG strip, not a pixel count.</param>
/// <param name="IsFrame">True for a complete directly resolvable native DMA frame; false for the shared statue strip whose overlapping transfer windows are resolved separately.</param>
[IndependentFlag("whether an atlas segment is an animation frame, a fact of the atlas data")]
public readonly record struct RoomFxAtlasSegment(int SourceAddress, int ByteCount, bool IsFrame);
