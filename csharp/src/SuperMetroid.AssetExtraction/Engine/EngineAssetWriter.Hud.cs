using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What is shown over play: the strip across the top of the screen, the escape's countdown, and the areas' maps.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>Writes the HUD's data file and pictures, and each area's map as the HUD and as the pause screen show it.</summary>
    private void WriteHud()
    {
        const string folder = "hud";
        InstalledScreenContent screens = _installation.LoadMaps();
        GameplayHudPresentation hud = screens.GameplayHud;
        string Picture(HudImage image) => _sheets.WriteSprite(_tree, SpriteSheetId.PauseHud, image.FrameName, $"{folder}/{EngineSheets.FileName(image.FrameName)}.png");
        static JsonObject Corner(PlacedHudImage placed) => At(placed.Corner.X, placed.Corner.Y);

        // Its background is the old game's top row and the rows under it, as one picture.
        (RgbaPixel[] top, int topWidth, int topHeight) = _sheets.SpritePixels(SpriteSheetId.PauseHud, GameplayHudPresentation.TopRow.Image.FrameName);
        (RgbaPixel[] rest, int restWidth, int restHeight) = _sheets.SpritePixels(SpriteSheetId.PauseHud, GameplayHudPresentation.Template.Image.FrameName);
        if (topWidth != restWidth || GameplayHudPresentation.TopRow.Corner != default || GameplayHudPresentation.Template.Corner != new Position16<ScreenPixels>(0, topHeight))
            throw new InvalidDataException("The HUD's top row and the rows under it do not lie one over the other, as this takes them to.");
        var template = new RgbaPixel[topWidth * (topHeight + restHeight)];
        top.CopyTo(template, 0);
        rest.CopyTo(template, top.Length);
        _tree.WritePicture(folder + "/template.png", topWidth, topHeight + restHeight, template);

        var digits = new JsonArray();
        var ammoDigits = new JsonArray();
        for (int numeral = 0; numeral < GameplayHudDefinitions.Numerals; numeral++)
        {
            digits.Add(Picture(new HudEnergyDigitImage(numeral)));
            ammoDigits.Add(Picture(new HudAmmoDigitImage(numeral)));
        }

        var tankPlaces = new JsonArray();
        for (int tank = 0; tank < GameplayHudDefinitions.EnergyTankCount; tank++)
            tankPlaces.Add(Corner(hud.EnergyTank(tank, HudEnergyTank.Full)));

        // Where a count's first digit goes: the old game lays a count of nothing out from there.
        JsonObject Counter(HudAmmoCounter counter)
        {
            PlacedHudImage[] placed = [.. hud.Ammo(counter, 0)];
            JsonObject at = Corner(placed[0]);
            at["digits"] = placed.Length;
            return at;
        }

        // An icon she has shows over the background, lit when it is what she has selected.
        (string Selects, HudIcon Icon)[] selectable =
        [
            ("missiles", HudIcon.Missile), ("super_missiles", HudIcon.SuperMissile), ("power_bombs", HudIcon.PowerBomb),
            ("grapple_beam", HudIcon.Grapple), ("xray_scope", HudIcon.XRay),
        ];
        var icons = new JsonObject();
        foreach ((string selects, HudIcon icon) in selectable)
        {
            PlacedHudImage plain = hud.Icon(icon, added: true, HudIconShade.Deselected)
                ?? throw new InvalidDataException($"The HUD shows nothing for {icon} when she has it.");
            PlacedHudImage lit = hud.Icon(icon, added: true, HudIconShade.Selected)
                ?? throw new InvalidDataException($"The HUD shows nothing for {icon} when it is selected.");
            JsonObject at = Corner(plain);
            at["picture"] = Picture(plain.Image);
            at["selected_picture"] = Picture(lit.Image);
            icons[selects] = at;
        }

        PlacedHudImage full = hud.AutoReserve(HudReserveIndicator.Filled);
        JsonObject reserve = Corner(full);
        reserve["full"] = Picture(full.Image);
        reserve["empty"] = Picture(hud.AutoReserve(HudReserveIndicator.Empty).Image);
        reserve["cleared"] = Picture(hud.AutoReserve(HudReserveIndicator.Cleared).Image);

        PlacedHudImage hidden = hud.MinimapCell(default, MinimapHiddenCell.Instance);

        // The countdown: its label and digits lie about one place, each by the point of its picture the old game puts there.
        EscapeTimerPresentation timer = screens.EscapeTimer;
        string TimerPicture(EscapeTimerGlyph glyph) => _sheets.WriteSprite(
            _tree, SpriteSheetId.EscapeTimer, EscapeTimerComposition.FrameName(glyph), $"{folder}/timer_{EngineSheets.FileName(EscapeTimerComposition.FrameName(glyph))}.png");
        JsonObject TimerAt(EscapeTimerAnchor anchor, EscapeTimerGlyph glyph)
        {
            (int _, int _, int originX, int originY) = _sheets.SpriteSize(SpriteSheetId.EscapeTimer, EscapeTimerComposition.FrameName(glyph));
            return At(timer.anchors[anchor].X - originX, timer.anchors[anchor].Y - originY);
        }

        var timerDigits = new JsonArray();
        for (int digit = 0; digit < GameplayHudDefinitions.Numerals; digit++)
            timerDigits.Add(TimerPicture(EscapeTimerComposition.DigitGlyph(digit)));
        EscapeTimerGlyph nought = EscapeTimerComposition.DigitGlyph(0);

        JsonObject data = [];
        EngineDefaults.Fill(data, "Hud");
        data["select_sound"] = EngineNames.Sound(SamusSound.HudWeaponSelect);
        data["area_folder"] = folder + "/areas";
        data["template"] = folder + "/template.png";
        data["digits"] = digits;
        data["ammo_digits"] = ammoDigits;
        data["tank_full"] = Picture(new HudEnergyTankImage(HudEnergyTank.Full));
        data["tank_empty"] = Picture(new HudEnergyTankImage(HudEnergyTank.Empty));
        data["tank_places"] = tankPlaces;
        data["energy_at"] = Corner(hud.EnergyCount(0).First());
        data["counters"] = new JsonObject
        {
            ["missiles"] = Counter(HudAmmoCounter.Missiles),
            ["super_missiles"] = Counter(HudAmmoCounter.SuperMissiles),
            ["power_bombs"] = Counter(HudAmmoCounter.PowerBombs),
        };
        data["icons"] = icons;
        data["auto_reserve"] = reserve;
        data["minimap_at"] = Corner(hidden);
        data["minimap_backing"] = Picture(hidden.Image);
        data["timer"] = new JsonObject
        {
            ["label"] = TimerPicture(EscapeTimerGlyph.Label),
            ["label_at"] = TimerAt(EscapeTimerAnchor.Label, EscapeTimerGlyph.Label),
            ["digits"] = timerDigits,
            ["minutes_at"] = TimerAt(EscapeTimerAnchor.Minutes, nought),
            ["seconds_at"] = TimerAt(EscapeTimerAnchor.Seconds, nought),
            ["hundredths_at"] = TimerAt(EscapeTimerAnchor.Centiseconds, nought),
            ["digit_spacing"] = timer.DigitSpacing,
        };
        _tree.WriteData("hud/hud.json", data);

        // The map as the HUD shows it, and as the pause screen does.
        CheckMapsNeverWrap(screens);
        WriteAreaMaps(
            screens, folder + "/areas", SpriteSheetId.PauseHud,
            cell => new HudMinimapCellImage(cell, MinimapShade.Explored).FrameName, cell => new HudMinimapCellImage(cell, MinimapShade.Unexplored).FrameName,
            cell => new HudMinimapCellImage(cell, MinimapShade.SamusLocation).FrameName, null, null);

        // The pause screen's own pictures of the cells are in its own colours.
        WriteAreaMaps(
            screens, "areas", SpriteSheetId.PausePictures, cell => RoomMapCellFrame(cell, MapCellShade.Explored), cell => RoomMapCellFrame(cell, MapCellShade.Unexplored),
            null, (area, revealedBy) => AreaMapLabels(screens, area, revealedBy, MapSpriteScreen.Pause, "areas/labels"), null);
    }

    /// <summary>The name of the picture of a cell of an area's map, as the pause screen and the map of a saved game show it.</summary>
    private static string RoomMapCellFrame(MapCell cell, MapCellShade shade) => $"Map.Cell.{cell.Name}.{MapCellShadeKeys.Installed.Of(shade)}";

    /// <summary>
    /// Checks that nothing of the old game's maps reaches across their left and right edges. The old map wraps: a
    /// column past its right edge is the column as far from its left, in what is explored and in the little map of
    /// the HUD. The Godot maps do not, and need not while no room's screens pass the map's edges and no cell of a
    /// map lies within the little map's reach of the left or right edge, which is so of the game's own maps.
    /// </summary>
    /// <exception cref="InvalidDataException">A room or a cell of a map would be seen across the map's edge.</exception>
    private static void CheckMapsNeverWrap(InstalledScreenContent screens)
    {
        foreach (SuperMetroid.Core.Rooms.RoomDefinition room in EngineLayouts.States().Select(state => state.Room).Distinct())
        {
            if (room.MapPosition.X < 0 || room.MapPosition.Y < 0 || room.MapPosition.X + room.Size.X > AreaMapSize.WidthInMapTiles
                || room.MapPosition.Y + room.Size.Y > AreaMapSize.HeightInMapTiles)
            {
                throw new InvalidDataException($"The room {room.Name} runs past the edge of its area's map, where the old map wraps and the Godot map does not.");
            }
        }

        int reach = MinimapGrid.Reach.X;
        foreach (AreaId id in AreaIds.All)
        {
            IAreaMapView map = screens.Get(id);
            for (int row = 0; row < AreaMapSize.HeightInMapTiles; row++)
            {
                foreach (int column in Enumerable.Range(0, reach).Concat(Enumerable.Range(AreaMapSize.WidthInMapTiles - reach, reach)))
                {
                    if (map.Cell(new Position16<MapTiles>(column, row)) != MapCell.Empty)
                        throw new InvalidDataException($"The map of {id} has a cell in column {column}, which the old little map shows from across the map's edge and the Godot one does not.");
                }
            }
        }
    }

    /// <summary>
    /// Writes every area's map: its cells and what each shows, which of them the area's map station does not reveal,
    /// and the sheets of small pictures its cells are drawn from, explored, unexplored and, where there is such a
    /// picture, as the cell Samus is in.
    /// </summary>
    private void WriteAreaMaps(
        InstalledScreenContent screens, string folder, SpriteSheetId sheet, Func<MapCell, string> explored, Func<MapCell, string> unexplored, Func<MapCell, string>? here,
        Func<AreaId, string, JsonArray>? labels, Func<AreaId, JsonArray>? markers)
    {
        foreach (AreaId id in AreaIds.All)
        {
            IAreaMapView map = screens.Get(id);
            string name = Named(id);
            var cells = new JsonArray();
            var kinds = new List<MapCell>();
            for (int row = 0; row < AreaMapSize.HeightInMapTiles; row++)
            {
                for (int column = 0; column < AreaMapSize.WidthInMapTiles; column++)
                {
                    var place = new Position16<MapTiles>(column, row);
                    MapCell cell = map.Cell(place);
                    if (cell == MapCell.Empty)
                        continue;
                    if (!kinds.Contains(cell))
                        kinds.Add(cell);
                    var entry = new JsonObject { ["column"] = column, ["row"] = row, ["shows"] = EngineSheets.FileName(cell.Name) };
                    if (!map.IsRevealedByMapStation(place))
                        entry["secret"] = true;

                    // The low piece of a sloped hallway's floor has the hallway's other half in the cell above it:
                    // the cartridge explores that cell with it.
                    if (cell.Slope is MapCellSlope.RightwardFloorLow or MapCellSlope.LeftwardFloorLow)
                        entry["explores_above"] = true;
                    cells.Add(entry);
                }
            }

            // An area with no map (the station above Zebes) still has a file, of no cells.
            const int size = GameplayHudDefinitions.MinimapSquarePixels;
            var tiles = new JsonObject();
            for (int kind = 0; kind < kinds.Count; kind++)
                tiles[EngineSheets.FileName(kinds[kind].Name)] = new JsonObject { ["column"] = kind, ["row"] = 0 };
            string Sheet(string shade, Func<MapCell, string> frame)
            {
                string path = $"{folder}/{name}_{shade}.png";
                int across = Math.Max(1, kinds.Count);
                var strip = new RgbaPixel[across * size * size];
                for (int kind = 0; kind < kinds.Count; kind++)
                {
                    // A picture that shows nothing leaves its cell of the sheet clear.
                    if (!_sheets.SpriteShows(sheet, frame(kinds[kind])))
                        continue;
                    (RgbaPixel[] pixels, int cellWidth, int cellHeight) = _sheets.SpritePixels(sheet, frame(kinds[kind]));
                    if (cellWidth != size || cellHeight != size)
                        throw new InvalidDataException($"The map picture {frame(kinds[kind])} is {cellWidth} by {cellHeight}, not {size} square.");
                    for (int y = 0; y < size; y++)
                        pixels.AsSpan(y * size, size).CopyTo(strip.AsSpan((y * across + kind) * size, size));
                }

                _tree.WritePicture(path, across * size, size, strip);
                return path;
            }

            // The fact the Godot engine's map station records for the area it is in.
            string revealedBy = $"map revealed: {name}";
            var data = new JsonObject
            {
                ["name"] = name,
                ["revealed_by"] = revealedBy,
                ["sheet"] = Sheet("explored", explored),
                ["unexplored_sheet"] = Sheet("unexplored", unexplored),
            };
            if (here is not null)
                data["here_sheet"] = Sheet("here", here);
            data["tile_size"] = size;
            data["tiles"] = tiles;
            data["cells"] = cells;
            if (markers?.Invoke(id) is { Count: > 0 } marked)
                data["markers"] = marked;
            if (labels?.Invoke(id, revealedBy) is { Count: > 0 } laid)
                data["labels"] = laid;
            _tree.WriteData($"{folder}/{name}.json", data);
        }
    }
}
