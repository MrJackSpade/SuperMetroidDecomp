namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One native transfer and its planar-byte position in the sixteen-character replacement sheet.</summary>
/// <param name="Asset">Stable installed-artwork identity retained by deferred VRAM writes instead of embedding image bytes in pending simulation state.</param>
/// <param name="AtlasOffset">Byte offset into the sheet's encoded 4-bpp characters, not a PNG pixel offset: 0/32/64/96 for endpoints and 128/256/384 for orientation groups.</param>
/// <param name="ByteCount">Transfer length in bytes: 32 for one endpoint character or 128 for four rope characters; not a VRAM word count.</param>
public readonly record struct GrappleTileTransfer(InstalledArtworkId Asset, int AtlasOffset, int ByteCount)
{
    /// <summary>Transfer length in bytes, 0 through 65535.</summary>
    public int ByteCount { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, ByteCount);
}
