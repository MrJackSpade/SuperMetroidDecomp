using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Frontend.Map.Artwork.MapScreenDefinitions;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// Lists the map screen's pages (<see cref="MapScreenDefinitions"/>) and accepts only Zebes areas for them.
/// </summary>
public static class MapScreens
{
    /// <summary>Returns <paramref name="area"/> when it is a playable Zebes area, rejecting menu or other non-area identities.</summary>
    internal static AreaId RequireZebesArea(AreaId area) => AreaIds.IsZebes(area)
        ? area : throw new ArgumentOutOfRangeException(nameof(area));

    /// <summary>Named pages select one of the authored PNG atlases, never a VRAM address.</summary>
    public static IEnumerable<MapScreenPage> Pages()
    {
        yield return new(WorldForeground, WorldMapArtworkFormat.TileColumns, WorldMapArtworkFormat.ForegroundTileCount);
        foreach (AreaId area in AreaIds.Zebes)
        {
            yield return new(WorldBackground(area), WorldMapArtworkFormat.TileColumns, WorldMapArtworkFormat.BackgroundTileCount);
            yield return new(RoomFrame(area), MapTileColumns, MapTileCount);
        }
    }
}
