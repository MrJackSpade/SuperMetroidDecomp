using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The map of where a saved game was saved, shown between the options and the game (<c>menus/save_map.json</c>):
/// the planet and its areas' names, each area's own map as that screen colours it with its stations, its ship and
/// its bosses marked, and the window that opens from one to the other. With it, what that screen and the pause
/// screen's map page share: how a map is laid behind a frame and moved, its arrows, their highlight, and the
/// pulsing sign of a place.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>
    /// How a map screen lays an area's map behind its frame and moves it, as <c>ScrollingMap</c> reads it: the size
    /// of a cell and of the screen, the opening of the frame, where the map first lies, and how far a press moves it.
    /// </summary>
    private static JsonObject MapFramingData(MapFraming framing) => new()
    {
        ["cell"] = GameplayHudDefinitions.MinimapSquarePixels,
        ["screen_width"] = ScreenGeometry.ScreenWidthPixels,
        ["screen_height"] = ScreenGeometry.ScreenHeightPixels,
        // The opening of the frame the map shows through, in the frame's own picture.
        ["opening"] = new JsonObject
        {
            ["x"] = MapFrameDefinitions.OpeningCorner.X, ["y"] = MapFrameDefinitions.OpeningCorner.Y,
            ["width"] = MapFrameDefinitions.OpeningSize.X, ["height"] = MapFrameDefinitions.OpeningSize.Y,
        },
        ["centre_line"] = framing.CentreLine,
        ["left_edge"] = framing.LeftEdge,
        ["right_edge"] = framing.RightEdge,
        ["top_edge"] = framing.TopEdge,
        ["bottom_edge"] = framing.BottomEdge,
        ["centre_column"] = MapFramingDefinitions.CentreColumn,
        ["row_middle_above_centre"] = MapFramingDefinitions.RowMiddleAboveCentredLine,
        ["here_leftmost"] = MapFramingDefinitions.MarkerLeftmostColumn,
        ["here_rightmost"] = MapFramingDefinitions.MarkerRightmostColumn,
        ["here_top"] = MapFramingDefinitions.MarkerTopLine,
        ["lowest_map_top"] = MapFramingDefinitions.LowestMapTop,
        // The cells its bounds are taken to be while none of it shows.
        ["unrevealed"] = new JsonObject
        {
            ["left"] = MapFramingDefinitions.UnrevealedLeftColumn, ["right"] = MapFramingDefinitions.UnrevealedRightColumn,
            ["top"] = MapFramingDefinitions.UnrevealedTopRow, ["bottom"] = MapFramingDefinitions.UnrevealedBottomRow,
        },
        ["scroll_step"] = MapScrollDefinitions.StepPixels,
        ["scroll_updates"] = MapScrollDefinitions.StepTicks,
        ["scroll_moves_at"] = MapScrollDefinitions.PulseTick,
    };

    /// <summary>
    /// The highlight a map's arrows are coloured by: how long each of its looks lasts, and how long the first lasts
    /// the first time, which is what the old highlight has left as it is made.
    /// </summary>
    private static JsonObject MapHighlightData(InstalledScreenContent screens) => new()
    {
        ["first_updates"] = new MapHighlightAnimation(screens.Maps.HighlightCycle).player.UpdatesLeft,
        ["updates"] = new JsonArray([.. screens.Maps.HighlightCycle.Frames.Select(look => (JsonNode)screens.Maps.HighlightCycle.Duration(look))]),
    };

    /// <summary>
    /// The arrows that say which ways a map can be moved: each lies with its own point at its place, as far down the
    /// screen from where the old game keeps it as the screen's framing says, and has a picture for each look of the
    /// highlight, written under <paramref name="folder"/>.
    /// </summary>
    private JsonObject MapArrowData(InstalledScreenContent screens, MapFraming framing, string folder)
    {
        var arrows = new JsonObject();
        foreach (MapScrollDirection direction in MapScrollControls.Directions)
        {
            string frame = MapSpriteDefinitions.Names[MapArrowDefinitions.ArrowPictures[direction]];
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MapSprites, frame);
            var looks = new JsonArray();
            foreach (MapHighlightFrame look in screens.Maps.HighlightCycle.Frames)
            {
                looks.Add(_sheets.WriteSprite(
                    _tree, SpriteSheetId.MapSprites, frame, $"{folder}/map_{EngineSheets.FileName(frame)}_{look.Frame}.png", MapSpriteShades.Highlight(look).Key));
            }

            var at = screens.Maps.Arrows.Get(direction).Position;
            arrows[Named(direction)] = new JsonObject { ["x"] = at.X - originX, ["y"] = at.Y + framing.ArrowDrop - originY, ["pictures"] = looks };
        }

        return arrows;
    }

    /// <summary>
    /// The timeline of the sign that swells and shrinks about a place of a map: where Samus is on the pause screen,
    /// where the game was saved on the map of a saved game. Its pictures are placed from that place. Its first look
    /// lasts, the first time, only as long as the old pulse has left as it is made; each time it comes round after
    /// that, its first look gives <paramref name="cue"/>, where one is named.
    /// </summary>
    private JsonObject MapIndicatorPulse(MapSpriteScreen screen, string folder, string? cue)
    {
        JsonObject Step(MapSpriteId sprite, int updates)
        {
            string frame = MapSpriteDefinitions.Names[sprite];
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MapSprites, frame);
            return new JsonObject
            {
                ["picture"] = _sheets.WriteSprite(
                    _tree, SpriteSheetId.MapSprites, frame, $"{folder}/map_{EngineSheets.FileName(frame)}.png",
                    MapSpriteShades.Drawn(screen, MapSpriteColouring.Markers).Key),
                ["updates"] = updates,
                ["offset"] = At(-originX, -originY),
            };
        }

        var looks = new List<AnimationBeat<MapSpriteId, MapIndicatorCue>.Frame>();
        IReadOnlyList<AnimationBeat<MapSpriteId, MapIndicatorCue>> beats = PauseMapIndicatorAnimation.Segments.Beats(MapIndicatorSegment.Pulse);
        for (int beat = 0; beat < beats.Count; beat++)
        {
            if (beats[beat] is AnimationBeat<MapSpriteId, MapIndicatorCue>.Frame look)
                looks.Add(look);
            else if (beats[beat] is not AnimationBeat<MapSpriteId, MapIndicatorCue>.Event { Raised: MapIndicatorCue.PulseCompleted } || beat != beats.Count - 1)
                throw new InvalidDataException("The map's pulsing sign does something other than show its looks and then say it has come round, which this does not know.");
        }

        // The first look, cut short the first time; the rest; and the first again as the pulse comes round to it.
        var steps = new JsonArray(Step(looks[0].Shown, new MapIndicatorPulse().player.UpdatesLeft));
        foreach (AnimationBeat<MapSpriteId, MapIndicatorCue>.Frame look in looks.Skip(1))
            steps.Add(Step(look.Shown, look.Duration));
        JsonObject round = Step(looks[0].Shown, looks[0].Duration);
        if (cue is not null)
            round["cues"] = new JsonArray(cue);
        steps.Add(round);
        return new JsonObject { ["steps"] = steps, ["loops_to"] = 1 };
    }

    /// <summary>
    /// The markers of <paramref name="area"/>'s map on the map of a saved game: each missile, energy and map station
    /// once the cell it stands in has been explored, and the ship where it landed, always.
    /// </summary>
    private JsonArray SaveMapMarkers(InstalledScreenContent screens, AreaId area, string folder)
    {
        var markers = new JsonArray();
        JsonObject Marker(MapSpriteId sprite, Position16<MapPixels> at, int column, int row, string when)
        {
            string frame = MapSpriteDefinitions.Names[sprite];
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MapSprites, frame);
            return new JsonObject
            {
                ["column"] = column, ["row"] = row,
                ["picture"] = _sheets.WriteSprite(
                    _tree, SpriteSheetId.MapSprites, frame, $"{folder}/{EngineSheets.FileName(frame)}.png",
                    MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, MapSpriteColouring.Markers).Key),
                ["x"] = at.X - originX, ["y"] = at.Y - originY, ["when"] = when,
            };
        }

        // In the old order of drawing: the missile stations, the energy stations, the map stations, then the ship.
        (MapStationKind Kind, MapSpriteId Sprite)[] kinds =
            [(MapStationKind.Missile, MapSpriteId.StationMissile), (MapStationKind.Energy, MapSpriteId.StationEnergy), (MapStationKind.Map, MapSpriteId.StationMap)];
        foreach ((MapStationKind kind, MapSpriteId sprite) in kinds)
        {
            foreach (MapStationDiscoveryRule rule in MapStationDiscoveryRules.Get(area, kind))
                markers.Add(Marker(sprite, screens.Maps.Stations.Get(rule.Id), rule.Cell.X, rule.Cell.Y, "explored"));
        }

        if (MapLandmarks.ShowsGunship(area))
        {
            Position16<MapPixels> at = screens.Maps.Landmarks.Get(MapLandmarkDefinitions.Gunship);
            const int cell = GameplayHudDefinitions.MinimapSquarePixels;
            markers.Add(Marker(MapSpriteId.MarkerGunship, at, at.X / cell, at.Y / cell, "always"));
        }

        return markers;
    }

    /// <summary>
    /// The entry point a game saved at <paramref name="station"/> resumes at in the Godot game: the ship's own name
    /// for the ship, and for any other the marker the room generator places for it.
    /// </summary>
    private static string SavedEntryPoint(LoadStation station) => station == LoadStationDefinitions.Gunship ? "Gunship" : "At" + station.Name;

    /// <summary>Writes the map of where a saved game was saved.</summary>
    private void WriteSaveMap()
    {
        const string folder = "menus/save_map", areaFolder = folder + "/areas";
        InstalledScreenContent screens = _installation.LoadMaps();
        MapFraming framing = MapFramingDefinitions.FileSelect;

        // Each area's map as this screen colours it, with what it lays over the cells.
        WriteAreaMaps(
            screens, areaFolder, SpriteSheetId.RoomMapPictures, cell => RoomMapCellFrame(cell, MapCellShade.Explored), cell => RoomMapCellFrame(cell, MapCellShade.Unexplored),
            null, (area, revealedBy) => AreaMapLabels(screens, area, revealedBy, MapSpriteScreen.FileSelect, areaFolder + "/labels"),
            area => AreaIds.IsZebes(area) ? SaveMapMarkers(screens, area, areaFolder + "/markers") : []);

        // A map sprite in one of this screen's colourings, with where its corner lies when it is drawn at a place.
        JsonObject Sprite(MapSpriteId sprite, MapSpriteColouring colouring, int x, int y)
        {
            string frame = MapSpriteDefinitions.Names[sprite];
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.MapSprites, frame);
            JsonObject at = At(x - originX, y - originY);
            at["picture"] = _sheets.WriteSprite(
                _tree, SpriteSheetId.MapSprites, frame, $"{folder}/{EngineSheets.FileName(frame)}_{Named(colouring)}.png",
                MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, colouring).Key);
            return at;
        }

        var areas = new JsonObject();
        var frames = new JsonObject();
        foreach (ZebesArea zebes in Enum.GetValues<ZebesArea>())
        {
            AreaId area = AreaIds.Area(zebes);
            string name = Named(area);
            Offset16<ScreenPixels> labelAt = screens.Maps.Labels.Get(zebes);
            JsonObject lit = Sprite(MapSpriteDefinitions.WorldLabels[zebes], MapSpriteColouring.Labels, labelAt.X, labelAt.Y);
            JsonObject dim = Sprite(MapSpriteDefinitions.WorldLabels[zebes], MapSpriteColouring.DimLabels, labelAt.X, labelAt.Y);
            FileSelectMapWindowMotion reveal = FileSelectMapWindowMotions.Get(zebes);
            areas[name] = new JsonObject
            {
                ["backdrop"] = Colour(screens.Backdrops.World(area)),
                // The planet and the area's glow over it, each a whole picture in the colours this area gives the planet.
                ["foreground"] = _sheets.WriteSprite(
                    _tree, ScreenPictureSheetDefinitions.AreaMapForegrounds[area], AreaMapPictures.WorldForegroundFrame, $"{folder}/world_{name}_foreground.png"),
                ["glow"] = _sheets.WriteSprite(
                    _tree, ScreenPictureSheetDefinitions.AreaMapBackgrounds[area], AreaMapPictures.WorldBackgroundFrame, $"{folder}/world_{name}_glow.png"),
                ["label"] = new JsonObject
                {
                    ["x"] = lit["x"]!.GetValue<int>(), ["y"] = lit["y"]!.GetValue<int>(),
                    ["lit"] = lit["picture"]!.GetValue<string>(), ["dim"] = dim["picture"]!.GetValue<string>(),
                },
                // The window onto the area's map opens from its name: how long its edges move, and how far each
                // moves an update, in 65536ths of a pixel.
                ["label_at"] = At(labelAt.X, labelAt.Y),
                ["reveal"] = new JsonObject
                {
                    ["updates"] = reveal.MovingUpdates, ["left"] = reveal.Left.RawValue, ["right"] = reveal.Right.RawValue,
                    ["top"] = reveal.Top.RawValue, ["bottom"] = reveal.Bottom.RawValue,
                },
            };

            // The frame of the area's map lies as far up the screen as the old game scrolls it; one the installation
            // lists as showing nothing has no picture.
            string roomFrame = AreaMapPictures.RoomFrameName(area);
            frames[name] = new JsonObject
            {
                ["picture"] = _sheets.SpriteIsBlank(SpriteSheetId.RoomMapPictures, roomFrame)
                    ? "" : _sheets.WriteSprite(_tree, SpriteSheetId.RoomMapPictures, roomFrame, $"{folder}/frame_{name}.png"),
                ["x"] = 0, ["y"] = -FileSelectMapDefinitions.RoomFrameVerticalScroll,
            };
        }

        // The places a game can be saved at that this map marks, by the entry point a game saved there resumes at.
        var stations = new JsonObject();
        foreach (LoadStation station in LoadStationDefinitions.Usable.Where(MapSaveMarkers.IsMarked))
        {
            Position16<MapPixels> marker = screens.Maps.SaveMarkers.Get(station), anchor = FileSelectMapLoadAnchors.Get(station);
            stations[SavedEntryPoint(station)] = new JsonObject
            {
                ["area"] = Named(station.Area), ["x"] = marker.X, ["y"] = marker.Y, ["anchor"] = At(anchor.X, anchor.Y),
            };
        }

        JsonObject map = MapFramingData(framing);
        JsonObject backing = Sprite(MapSpriteId.IndicatorBacking, MapSpriteColouring.Markers, 0, 0);

        // The load's fade takes a level an update from full, so its length is the number of levels.
        if (FileSelectMapDefinitions.LoadFadeFrames != DisplayBrightness.StepsFromBlackToFull)
            throw new InvalidDataException("The old load from the map fades for other than one update a level of brightness, which the Godot screen fades by.");
        _tree.WriteData("menus/save_map.json", new JsonObject
        {
            ["fade_levels"] = DisplayBrightness.StepsFromBlackToFull,
            ["entry"] = new JsonObject
            {
                ["black_updates"] = FileSelectMapDefinitions.EntryBlackHoldUpdates,
                ["left"] = FileSelectMapDefinitions.EntryWindowLeft, ["right"] = FileSelectMapDefinitions.EntryWindowRight,
                ["top"] = FileSelectMapDefinitions.EntryWindowTop, ["speed"] = FileSelectMapDefinitions.EntryWindowSpeed,
            },
            ["world"] = new JsonObject
            {
                ["title"] = Sprite(
                    MapSpriteId.WorldTitle, MapSpriteColouring.Labels, FileSelectMapDefinitions.WorldTitlePosition.X, FileSelectMapDefinitions.WorldTitlePosition.Y),
                ["order"] = new JsonArray([.. FileSelectMapAreaOrder.Labels.Select(zebes => (JsonNode)Named(AreaIds.Area(zebes)))]),
                ["areas"] = areas,
            },
            ["room"] = new JsonObject
            {
                ["backdrop"] = Colour(screens.Backdrops.Menus),
                ["frames"] = frames,
                ["map"] = map,
                ["area_folder"] = areaFolder,
                ["highlight"] = MapHighlightData(screens),
                ["arrows"] = MapArrowData(screens, framing, folder),
                ["marker"] = new JsonObject
                {
                    ["backing"] = backing,
                    // Each time the pulse comes round, the backing shows or hides.
                    ["pulse"] = MapIndicatorPulse(MapSpriteScreen.FileSelect, folder, "completed"),
                },
            },
            ["return"] = new JsonObject
            {
                ["setup_updates"] = FileSelectMapDefinitions.ReturnSetupFrames,
                ["inset"] = FileSelectMapDefinitions.ReturnWindowInset,
                ["fewer_updates"] = FileSelectMapDefinitions.ReturnWindowTimerReduction,
            },
            ["load"] = new JsonObject
            {
                ["prelude_updates"] = FileSelectMapDefinitions.LoadPreludeFrames,
                ["black_updates"] = FileSelectMapDefinitions.LoadBlackFrames,
            },
            ["stations"] = stations,
            ["sounds"] = new JsonObject
            {
                ["confirm"] = EngineNames.Sound(SamusSound.MenuConfirm),
                ["expand"] = EngineNames.Sound(SamusSound.MapExpand),
                ["return"] = EngineNames.Sound(SamusSound.MapReturn),
                ["scroll"] = EngineNames.Sound(SamusSound.MapScroll),
                ["highlight"] = EngineNames.Sound(RoomSound.PauseBeep),
            },
        });
    }
}
