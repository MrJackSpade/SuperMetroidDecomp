namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// One ordered strip of characters in the installed room-FX artwork atlas (#1415): a picture of an animated-tile
/// object, or the shared Tourian statue strip. Each kind is its own record; extraction maps each to where the cartridge
/// stores it.
/// </summary>
public abstract record RoomFxAtlasSegment
{
    /// <summary>Only the nested kinds derive.</summary>
    private RoomFxAtlasSegment()
    {
    }

    /// <summary>Number of ordered 2bpp planar bytes contributed to the PNG strip, not a pixel count.</summary>
    public abstract int ByteCount { get; }

    /// <summary>One complete picture of an animated-tile object, resolved directly by its identity.</summary>
    /// <param name="Picture">The picture the segment holds.</param>
    /// <param name="Bytes">The picture's bytes.</param>
    public sealed record Image(AnimatedTileImage Picture, int Bytes) : RoomFxAtlasSegment
    {
        /// <inheritdoc/>
        public override int ByteCount => Bytes;
    }

    /// <summary>
    /// The shared Tourian statue strip, whose overlapping picture windows
    /// <see cref="TourianStatueTileStripDefinitions"/> places.
    /// </summary>
    public sealed record TourianStatueStrip : RoomFxAtlasSegment
    {
        /// <inheritdoc/>
        public override int ByteCount => TourianStatueTileStripDefinitions.ByteCount;
    }
}
