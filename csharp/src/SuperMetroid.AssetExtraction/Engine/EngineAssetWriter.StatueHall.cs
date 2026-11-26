using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;
using Look = SuperMetroid.Core.Fx.Artwork.TourianStatueBlockPicture;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The hall of the statue at the way into Tourian. The old game makes the statue of three things: the room's
/// background picture, which holds Kraid's and Draygon's figures and whose blocks at their eyes change; sprites for
/// Phantoon's and Ridley's figures and the statue's base; and colours that change as a guardian is let go. The Godot
/// hall shows a picture for each guardian's eyes and one for it turned grey, over the room's backdrop.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The hall's data file and the folder its pictures and effects are kept in.</summary>
    private const string StatueHallFile = "rooms/tourian_statue_hall.json", StatueHallFolder = "rooms/tourian_statue_hall";

    /// <summary>A picture being put together a pixel at a time, with where its corner lies.</summary>
    private sealed partial class Canvas(int left, int top, int width, int height)
    {
        internal int Left { get; } = left;
        internal int Top { get; } = top;
        internal int Width { get; } = width;
        internal int Height { get; } = height;
        internal RgbaPixel[] Pixels { get; } = new RgbaPixel[width * height];

        /// <summary>Lays a picture whose corner lies at a place over this one, where it falls inside it.</summary>
        internal void Lay(RgbaPixel[] pixels, int pictureWidth, int pictureHeight, int atX, int atY)
        {
            for (int y = 0; y < pictureHeight; y++)
            {
                for (int x = 0; x < pictureWidth; x++)
                {
                    int toX = atX + x - Left, toY = atY + y - Top;
                    if (toX >= 0 && toY >= 0 && toX < Width && toY < Height)
                        Pixels[toY * Width + toX] = pixels[y * pictureWidth + x];
                }
            }
        }
    }

    /// <summary>Writes the statue hall's data, its pictures and effects, and the patches of its floor.</summary>
    private void WriteStatueHall()
    {
        List<(RoomDefinition Room, RoomStateSettings State)> states = StatesOfRoomsWith(
            state => state.SetupBehavior == RoomSetupBehavior.RunStatueUnlockingAnimations, "holds the statue at the way into Tourian");
        RoomStateSettings hall = states[0].State;
        if (states.Any(placed => placed.State.GraphicsSet != hall.GraphicsSet || placed.State.Background != hall.Background))
            throw new InvalidDataException("The statue hall is drawn more than one way, and its pictures are written for one.");
        TilesetDefinition tileset = RoomTilesets.Get(hall.GraphicsSet);
        string imageSheet = BackdropSheet(hall) ?? throw new InvalidDataException("The statue hall has no background picture of its own.");
        string blockSheet = RoomBlockSheet.NameOf(tileset);
        string spawner = EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.TourianStatue);
        string entrance = TourianStatueShades.Entrance.Key;

        // The floor: clear when the way was opened before; crumbling away row by row as it opens.
        const string access = GameInstallationLayout.RoomObjectTourianAccessVisualDirectoryName;
        static (string Frame, RoomBlockShape Shape) FloorDrawn(RoomObjectDraw draw) => draw switch
        {
            RoomObjectFrameDraw<TourianAccessDraws.CrumbleDraw> crumbling =>
                (RoomObjectTourianAccessVisualFiles.VisualId(draw), TourianAccessDraws.CrumbleFamily.Shape(crumbling.Frame)),
            RoomObjectFrameDraw<TourianAccessDraw> floor => (RoomObjectTourianAccessVisualFiles.VisualId(draw), TourianAccessDraws.Family.Shape(floor.Frame)),
            _ => throw new InvalidDataException("The floor of the statue hall draws something that is not one of its own frames."),
        };
        TerrainChangeAt(states, "floor_clear", TourianAccessObjectDefinitions.FloorBlock, TourianAccessFloors.ForState(TourianAccessFloor.Clear),
            access, FloorDrawn, leaves: "floor_clear");
        JsonObject floorCrumble = TerrainChangeAt(states, "floor_crumbles", TourianAccessObjectDefinitions.FloorBlock,
            TourianAccessFloors.ForState(TourianAccessFloor.Crumble), access, FloorDrawn);

        // The background picture, and the blocks of it that change with a guardian's eyes.
        (RgbaPixel[] plain, int imageWidth, int imageHeight) = _sheets.FrameShown(RoomBlockFamily, imageSheet, RoomBackgroundImage.FrameName);
        string manifest = Path.Combine(_installation.ContentDirectory, RoomBlockFamily, RoomBlockSheetFiles.BackgroundManifestFileName);
        JsonArray cells = JsonNode.Parse(File.ReadAllText(manifest))?["cells"]?[imageSheet]?.AsArray()
            ?? throw new InvalidDataException($"The installation lists no changing blocks for the background {imageSheet}.");
        const int cellSize = RoomGridConversions.BlockPixels;
        int columns = imageWidth / cellSize;

        // The pictures that stand in the room, each with the frame it is cut from and the colourings it is cut in, and
        // each guardian's data with the shade of the light of its eyes: once the light is made, the cartridge shows
        // the statue's sprites in that shade, laid over its entrance colours and under its other looks.
        var inRoom = new List<(string Path, string Frame, string[] Shades)>();
        var glows = new List<(JsonObject Data, string Glow)>();

        var statues = new JsonObject();
        foreach (TourianStatue statue in new[] { TourianStatue.Kraid, TourianStatue.Phantoon, TourianStatue.Draygon, TourianStatue.Ridley })
        {
            TourianStatueAnimationDefinition definition = TourianStatueAnimationDefinitions.Statues[statue];
            string name = EngineSheets.FileName(statue.ToString()), folder = $"{StatueHallFolder}/{name}";
            var data = new JsonObject();

            // As it is let go it greys in steps, each held so many updates and the last left in place. The cartridge
            // steps its colours through colourings that are the straight blend from its own colours to its grey: the
            // first is its own, the last its grey, and each between a step further, to within the cartridge's
            // rounding of a colour (read from the installation's sheets, 2026-11-26). So the Godot hall shows the
            // grey picture over it so much the more opaque at each step.
            var greyingUpdates = new JsonArray();
            foreach (AnimationBeat<int, SuperMetroid.Core.Fx.ColourEffects.ColourEffectCue>.Frame step in
                     SuperMetroid.Core.Fx.ColourEffects.ColourEffectShownFrames.Steps(definition.ColourEffect)
                         .OfType<AnimationBeat<int, SuperMetroid.Core.Fx.ColourEffects.ColourEffectCue>.Frame>())
            {
                greyingUpdates.Add(step.Duration);
            }

            if (greyingUpdates.Count < 2)
                throw new InvalidDataException($"The grey-out {definition.ColourEffect} of the statue of {statue} has fewer than two colourings, and so no steps from its own colours to grey.");
            data["greying_updates"] = greyingUpdates;
            if (definition.GreyedSprite is { } sprite)
            {
                // A figure that is a sprite: each look of its eyes is a picture of the whole figure.
                TourianStatueDecoration placed = TourianStatueDecorationDefinitions.SpawnOrder.Single(decoration => decoration.Kind == sprite switch
                {
                    TourianSpriteStatue.Phantoon => RoomEnemyProjectileKind.TourianStatuePhantoon,
                    TourianSpriteStatue.Ridley => RoomEnemyProjectileKind.TourianStatueRidley,
                    _ => throw new InvalidDataException($"The statue has a sprite figure this does not know: {sprite}."),
                });
                EnemyProjectileFrameId figure = sprite == TourianSpriteStatue.Phantoon ? EnemyProjectileFrameId.TourianStatuePhantoon : EnemyProjectileFrameId.TourianStatueRidley;
                string artwork = EncounterArtworkKeys.Installed.Of(TourianSpriteStatueEyeLookDefinitions.Encounters[sprite]);
                string Frame(Look look) => $"{EnemySheetDefinitions.ProjectileFrameName(figure)}|{artwork}={TourianSpriteStatueEyeLookDefinitions.LookNames[look]}";
                string[] looks = [.. Enum.GetValues<Look>().Select(Frame)];

                // Every look is cut in each of the figure's colourings, so that all lie alike: as it stands, with its
                // eyes put out, and grey.
                EngineSheetPictures standing = _sheets.CutSpawnedShades(_tree, folder, spawner, looks, entrance);
                EngineSheetPictures cleared = _sheets.CutSpawnedShades(_tree, $"{folder}/cleared", spawner, looks, entrance, definition.ClearedShade.Key);
                EngineSheetPictures grey = _sheets.CutSpawnedShades(_tree, $"{folder}/grey", spawner, looks, entrance, definition.ClearedShade.Key, definition.GreyShade.Key);
                data["in_room"] = true;
                data["bright"] = standing.Of(Frame(Look.EyesBright));
                data["dim"] = standing.Of(Frame(Look.EyesDim));
                data["dark"] = standing.Of(Frame(Look.EyesDark));
                data["released"] = cleared.Of(Frame(Look.Released));
                data["grey"] = grey.Of(Frame(Look.Released));
                data["offset"] = new JsonObject { ["x"] = placed.Place.X + standing.OffsetX, ["y"] = placed.Place.Y + standing.OffsetY };
                data["grey_offset"] = new JsonObject { ["x"] = placed.Place.X + grey.OffsetX, ["y"] = placed.Place.Y + grey.OffsetY };
                foreach (Look look in new[] { Look.EyesBright, Look.EyesDim, Look.EyesDark })
                    inRoom.Add((standing.Of(Frame(look)), Frame(look), [entrance]));
                inRoom.Add((cleared.Of(Frame(Look.Released)), Frame(Look.Released), [entrance, definition.ClearedShade.Key]));
                inRoom.Add((grey.Of(Frame(Look.Released)), Frame(Look.Released), [entrance, definition.ClearedShade.Key, definition.GreyShade.Key]));
            }
            else
            {
                // A figure that is part of the background picture: the blocks at its eyes change, and its colours.
                string animation = $"animation {definition.Animation.Name}";
                var eyes = new List<(int X, int Y, string Picture)>();
                foreach (JsonNode? cell in cells)
                {
                    if (cell!["inputs"]!.AsArray().Any(input => input!.GetValue<string>() == animation))
                    {
                        int at = cell["cell"]!.GetValue<int>();
                        eyes.Add((at % columns * cellSize, at / columns * cellSize, cell["picture"]!.GetValue<string>()));
                    }
                }

                if (eyes.Count == 0)
                    throw new InvalidDataException($"The background {imageSheet} has no blocks that change with {definition.Animation.Name}.");

                // The picture with the figure's blocks showing one look, in the colourings named.
                Canvas Shown(Look look, params string[] shades)
                {
                    var whole = new Canvas(0, 0, imageWidth, imageHeight);
                    (RgbaPixel[] image, int _, int _) = _sheets.FrameShown(RoomBlockFamily, imageSheet, RoomBackgroundImage.FrameName, shades);
                    whole.Lay(image, imageWidth, imageHeight, 0, 0);
                    foreach ((int x, int y, string picture) in eyes)
                    {
                        (RgbaPixel[] block, int width, int height) = _sheets.FrameShown(RoomBlockFamily, blockSheet, $"{picture}|{animation}={definition.Picture(look).Name}", shades);
                        whole.Lay(block, width, height, x, y);
                    }

                    return whole;
                }

                // What of a picture differs from the plain background, with the figure's eye blocks, cut down to the
                // rectangle given or, with none given, to what it covers.
                (string Path, int Left, int Top) Written(string file, Canvas shown, (int Left, int Top, int Right, int Bottom)? within)
                {
                    bool Kept(int x, int y) =>
                        shown.Pixels[y * imageWidth + x] != plain[y * imageWidth + x] ||
                        eyes.Any(eye => x >= eye.X && x < eye.X + cellSize && y >= eye.Y && y < eye.Y + cellSize);
                    (int left, int top, int right, int bottom) = within ?? Bounds(Kept);
                    var cut = new Canvas(left, top, right - left, bottom - top);
                    for (int y = top; y < bottom; y++)
                    {
                        for (int x = left; x < right; x++)
                        {
                            if (Kept(x, y))
                                cut.Pixels[(y - top) * cut.Width + x - left] = shown.Pixels[y * imageWidth + x];
                        }
                    }

                    string path = $"{folder}/{file}.png";
                    _tree.WritePicture(path, cut.Width, cut.Height, cut.Pixels);
                    return (path, left, top);
                }

                (int Left, int Top, int Right, int Bottom) Bounds(Func<int, int, bool> kept)
                {
                    int left = imageWidth, top = imageHeight, right = 0, bottom = 0;
                    for (int y = 0; y < imageHeight; y++)
                    {
                        for (int x = 0; x < imageWidth; x++)
                        {
                            if (!kept(x, y))
                                continue;
                            (left, top, right, bottom) = (Math.Min(left, x), Math.Min(top, y), Math.Max(right, x + 1), Math.Max(bottom, y + 1));
                        }
                    }

                    return (left, top, right, bottom);
                }

                // The looks of its eyes share one rectangle: that of its eyes put out, which changes the most.
                Canvas released = Shown(Look.Released, definition.ClearedShade.Key);
                (string releasedPath, int left, int top) = Written("released", released, null);
                (int, int, int, int) eyeBounds = Bounds((x, y) =>
                    released.Pixels[y * imageWidth + x] != plain[y * imageWidth + x] ||
                    eyes.Any(eye => x >= eye.X && x < eye.X + cellSize && y >= eye.Y && y < eye.Y + cellSize));
                (string greyPath, int greyLeft, int greyTop) = Written("grey", Shown(Look.Released, definition.ClearedShade.Key, definition.GreyShade.Key), null);
                data["in_room"] = false;
                data["bright"] = Written("bright", Shown(Look.EyesBright), eyeBounds).Path;
                data["dim"] = Written("dim", Shown(Look.EyesDim), eyeBounds).Path;
                data["dark"] = Written("dark", Shown(Look.EyesDark), eyeBounds).Path;
                data["released"] = releasedPath;
                data["grey"] = greyPath;
                data["offset"] = new JsonObject { ["x"] = left, ["y"] = top };
                data["grey_offset"] = new JsonObject { ["x"] = greyLeft, ["y"] = greyTop };
            }

            // What rises and falls as it is let go is coloured by the light of its eyes.
            string[] glowing = [entrance, definition.Eye.Glow.Key];
            (string Name, TourianStatueProjectileSegment Segment, bool Loops)[] effects =
            [
                ("eye_glow", TourianStatueProjectileSegment.EyeGlow, false), ("particle", TourianStatueProjectileSegment.Particle, true),
                ("tail", TourianStatueProjectileSegment.Tail, false), ("splash", TourianStatueProjectileSegment.Splash, false),
                ("soul", TourianStatueProjectileSegment.Soul, true),
            ];
            foreach ((string effect, TourianStatueProjectileSegment segment, bool loops) in effects)
            {
                IEnumerable<string> frames = TourianStatueProjectileAnimations.Segments.Beats(segment)
                    .Select(beat => beat.ShownFrame).Where(frame => frame.HasValue).Select(frame => EnemySheetDefinitions.ProjectileFrameName(frame!.Value));
                EngineSheetPictures pictures = _sheets.CutSpawnedShades(_tree, $"{folder}/{effect}", spawner, frames, glowing);

                // The hall itself does what these raise: the burst, the sparks, their tails.
                JsonObject timeline = EngineTimelines.Of(TourianStatueProjectileAnimations.Segments, segment, loops,
                    frame => pictures.Of(EnemySheetDefinitions.ProjectileFrameName(frame)), static (TourianStatueProjectileCue _) => null);
                timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
                string file = $"{folder}/{effect}.json";
                _tree.WriteData(file, timeline);
                data[effect] = file;
            }

            statues[name] = data;
            glows.Add((data, definition.Eye.Glow.Key));
        }

        // The statue's base, a sprite that stands in the room.
        TourianStatueDecoration baseDecoration = TourianStatueDecorationDefinitions.SpawnOrder.Single(decoration => decoration.Kind == RoomEnemyProjectileKind.TourianStatueBaseDecoration);
        string baseFrame = EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.TourianStatueBaseDecoration);
        EngineSheetPictures basePictures = _sheets.CutSpawnedShades(_tree, $"{StatueHallFolder}/base", spawner, [baseFrame], entrance);
        inRoom.Add((basePictures.Of(baseFrame), baseFrame, [entrance]));

        // The colours the light of each guardian's eyes changes on the pictures that stand in the room, picture by
        // picture: each colour as the picture is cut, and as it shows with the light's shade laid over its entrance
        // colours.
        JsonObject EyeGlowColours(string glow)
        {
            static string Written(RgbaPixel pixel) =>
                ((pixel.R << 16) | (pixel.G << 8) | pixel.B).ToString("x6", System.Globalization.CultureInfo.InvariantCulture);
            var written = new JsonObject();
            foreach ((string path, string frame, string[] shades) in inRoom)
            {
                string sheet = _sheets.SheetHolding(spawner, frame);
                (RgbaPixel[] unlit, int _, int _) = _sheets.FrameShown(GameInstallationLayout.EnemySheetDirectoryName, sheet, frame, shades);
                (RgbaPixel[] lit, int _, int _) = _sheets.FrameShown(GameInstallationLayout.EnemySheetDirectoryName, sheet, frame, [shades[0], glow, .. shades[1..]]);
                var changed = new SortedDictionary<string, string>(StringComparer.Ordinal);
                for (int i = 0; i < unlit.Length; i++)
                {
                    if (unlit[i].A == 0)
                        continue;
                    string from = Written(unlit[i]), to = Written(lit[i]);
                    if (changed.TryGetValue(from, out string? shown) && shown != to)
                        throw new InvalidDataException($"The light {glow} shows the colour {from} of {path} as {shown} in one place and as {to} in another.");
                    changed[from] = to;
                }

                var colours = new JsonObject();
                foreach ((string from, string to) in changed)
                {
                    if (from != to)
                        colours[from] = to;
                }

                if (colours.Count > MostSwapsShown)
                    throw new InvalidDataException($"The light {glow} changes {colours.Count} colours of {path}; a thing shows at most {MostSwapsShown} changed at once.");
                if (colours.Count > 0)
                    written[path] = colours;
            }

            return written;
        }

        foreach ((JsonObject data, string glow) in glows)
            data["eye_glow_colours"] = EyeGlowColours(glow);

        // The dust the sinking statue raises: so many puffs at once, each thrown a chance distance, so many times over.
        Animation<ChozoTourianDustSegment> descent = ChozoTourianDustAnimations.TourianDescentDust;
        List<IAnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>> puff = [.. ChozoTourianDustAnimations.Segments.Beats(descent.Phases[0].Segment)];
        EnemyProjectileScatter thrown = puff.OfType<AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event>()
            .Select(raised => raised.Raised).OfType<EnemyProjectileCommonCue.Scatter>().Single().Spread;
        string dust = $"{StatueHallFolder}/dust.json";
        _tree.WriteData(dust, SharedTimeline(dust, puff, null, EnemySheetDefinitions.ProjectileFrameName,
            static (EnemyProjectileCommonCue cue) => cue is EnemyProjectileCommonCue.Scatter or EnemyProjectileCommonCue.Signal
                ? null
                : throw new InvalidDataException($"The dust of the sinking statue raises {cue}, which this does not know.")));

        _tree.WriteData(StatueHallFile, new JsonObject
        {
            ["statues"] = statues,
            ["base"] = new JsonObject
            {
                ["picture"] = basePictures.Of(baseFrame),
                ["offset"] = new JsonObject { ["x"] = baseDecoration.Place.X + basePictures.OffsetX, ["y"] = baseDecoration.Place.Y + basePictures.OffsetY },
            },
            ["dust"] = dust,
            ["dust_puffs"] = TourianStatueSequenceDefinitions.DescentDustPuffs,
            ["dust_rounds"] = descent.Phases[0].Plays,
            ["dust_updates"] = puff.Sum(beat => beat is AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame shown ? shown.Duration : 0),
            ["dust_scatter"] = new JsonObject
            {
                ["width"] = thrown.Span.X,
                ["height"] = thrown.Span.Y,
                ["middle_x"] = thrown.Center.X,
                ["middle_y"] = thrown.Center.Y,
            },
            ["floor_crumble"] = floorCrumble,
            ["quake_sound"] = EngineNames.Sound(EnemySound.Lavaquake),
            ["release_sound"] = EngineNames.Sound(EnemySound.TourianStatueParticle),
        });
    }
}
