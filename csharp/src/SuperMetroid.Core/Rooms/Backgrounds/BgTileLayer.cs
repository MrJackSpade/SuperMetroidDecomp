namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>A background drawn tile by tile from characters, as a renderer shows it.</summary>
/// <param name="Tiles">The tiles.</param>
public sealed record BgTileLayer(IBgTileSource Tiles) : IBgSource
{
    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
    {
        bool shown = Tiles.TrySample(scrolled, out BgTileSample tile);
        sample = BgSample.OfTile(tile);
        return shown;
    }
}
