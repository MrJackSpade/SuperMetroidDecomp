using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// What the old pause screen lays over an area's map beside its cells: a sign where each great boss is, crossed out
/// once it is beaten, and the name of the area each elevator leads to.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The folder the pictures laid over the maps are written under.</summary>
    private const string MapLabelFolder = "areas/labels";

    /// <summary>
    /// The labels of <paramref name="area"/>'s map. A boss's sign shows once the area's map is had, as
    /// <paramref name="revealedBy"/> records, until the boss is beaten; then its cross and its sign in other colours
    /// show. An elevator's destination shows once the map is had.
    /// </summary>
    private JsonArray AreaMapLabels(InstalledScreenContent screens, AreaId area, string revealedBy)
    {
        MapLandmarkPositions places = screens.Maps.Landmarks;
        var labels = new JsonArray();

        // A map sprite in one of its colourings, written once, and where its corner lies when it is drawn at a place.
        JsonObject Label(MapSpriteId sprite, MapSpriteColouring colouring, Position16<MapPixels> at)
        {
            string frame = MapSpriteDefinitions.Names[sprite];
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MapSprites, frame);
            string picture = _sheets.WriteSprite(
                _tree, SpriteSheetId.MapSprites, frame, $"{MapLabelFolder}/{EngineSheets.FileName(frame)}_{Named(colouring)}.png",
                MapSpriteShades.Drawn(MapSpriteScreen.Pause, colouring).Key);
            return new JsonObject { ["x"] = at.X - originX, ["y"] = at.Y - originY, ["picture"] = picture };
        }

        foreach (MapBossMarker marker in MapLandmarks.BossMarkers(area))
        {
            Position16<MapPixels> at = places.Get(marker.Id);
            string defeated = EngineSheets.FileName(marker.Boss.ToString()) + "_defeated";
            JsonObject alive = Label(MapSpriteId.MarkerBoss, MapSpriteColouring.Markers, at);
            alive["when"] = revealedBy;
            alive["unless"] = defeated;
            labels.Add(alive);
            JsonObject cross = Label(MapSpriteId.MarkerDefeatedBoss, MapSpriteColouring.Markers, at);
            cross["when"] = defeated;
            labels.Add(cross);
            JsonObject beaten = Label(MapSpriteId.MarkerBoss, MapSpriteColouring.DefeatedBossMarker, at);
            beaten["when"] = defeated;
            labels.Add(beaten);
        }

        // The station above Zebes has no elevators to name.
        if (!AreaIds.IsZebes(area, out ZebesArea zebes))
            return labels;
        foreach (MapElevatorLabel elevator in MapLandmarks.Elevators(zebes))
        {
            JsonObject label = Label(MapLandmarks.ElevatorPicture(elevator.Destination), MapSpriteColouring.Labels, places.Get(elevator.Id));
            label["when"] = revealedBy;
            labels.Add(label);
        }

        return labels;
    }
}
