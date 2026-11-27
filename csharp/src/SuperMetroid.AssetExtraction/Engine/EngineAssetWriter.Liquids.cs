using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// What fills a room or lies over all of it: the liquids, and the rain, fog, spores and haze, each with its pictures
/// once for every set of graphics a room shows it in.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The equipment that lets Samus move in a liquid as in air, and that keeps lava from hurting her.</summary>
    private const string GravitySuit = "gravity_suit";

    /// <summary>
    /// The name of the liquid one of the old game's effects fills its room with, as the room scenes name its type: the
    /// kind of liquid, then what tells it from others of its kind that are coloured differently.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect shows no liquid, or one coloured as none of its kind is known to be.</exception>
    private static string LiquidFileName(RoomEffectDefinition effect)
    {
        string? name = (effect.Type, effect.Tint) switch
        {
            (RoomFxType.Water or RoomFxType.TourianEntranceStatue, RoomEffectTint.WaterAndAcid) => "water",
            (RoomFxType.Water, RoomEffectTint.GreenMaridiaWater) => "water_green",
            (RoomFxType.Water, RoomEffectTint.IndigoMaridiaWater) => "water_indigo",
            (RoomFxType.Water, RoomEffectTint.PurpleMaridiaWater) => "water_purple",
            (RoomFxType.Water, RoomEffectTint.TealMaridiaWater) => "water_teal",
            (RoomFxType.Lava, RoomEffectTint.Lava) => "lava",
            (RoomFxType.Acid, RoomEffectTint.None) => "acid",
            (RoomFxType.Acid, RoomEffectTint.Lava) => "acid_lava",
            (RoomFxType.Acid, RoomEffectTint.WaterAndAcid) => "acid_water",
            _ => null,
        };
        return name ?? throw new InvalidDataException($"No liquid is known for the effect {effect.Type} coloured {effect.Tint}.");
    }

    /// <summary>The name a liquid's type goes by, which is the name of Samus's way of moving in it and of the damage it does.</summary>
    private static string LiquidKind(RoomFxType type) => type switch
    {
        RoomFxType.Lava => "lava",
        RoomFxType.Acid => "acid",
        _ when RoomFxTypes.UsesWater(type) => "water",
        _ => throw new InvalidDataException($"The effect {type} is no liquid."),
    };

    /// <summary>The word a look's data has for the way one of the old game's effects is laid over its room.</summary>
    private static string BlendName(RoomEffectDefinition effect) =>
        (effect.EffectBlend ?? throw new InvalidDataException($"The effect {effect.Type} shows on the overlay and says nothing of how.")).Operation switch
        {
            BlendOperation.Add => "add",
            BlendOperation.Subtract => "subtract",
            BlendOperation other => throw new InvalidDataException($"The effect {effect.Type} is laid over its room in a way this does not know: {other}."),
        };

    /// <summary>The sheet of the pictures a room's effect shows, for one state of a room.</summary>
    private static string EffectSheet(RoomStateSettings state) =>
        WrittenPictureSheet.NameOf(WrittenPictureLayer.FxOverlay, RoomTilesets.Get(state.GraphicsSet));

    /// <summary>
    /// The name of the frame of an effect sheet that shows <paramref name="subject"/> while the animation named, if
    /// any, shows the frame named and nothing else that changes the sheet's pictures is going on.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame, or more than one.</exception>
    private string EffectFrame(string sheet, string subject, BlockAnimation? animation, string shown)
    {
        string? own = animation is { } playing ? $"animation {BlockAnimationFrames.Frame(playing, shown).Animation.Name}" : null;
        var found = new HashSet<string>(StringComparer.Ordinal);
        foreach (string frame in _sheets.FramesOf(RoomBlockFamily, sheet))
        {
            string whole = frame.EndsWith(PictureInFront, StringComparison.Ordinal) ? frame[..^PictureInFront.Length] : frame;
            string[] parts = whole.Split('|');
            if (parts[0] != subject)
                continue;
            bool matches = true, ownSeen = own is null;
            foreach (string part in parts.Skip(1))
            {
                int equals = part.IndexOf('=', StringComparison.Ordinal);
                if (equals < 0)
                    throw new InvalidDataException($"The frame {frame} of the sheet {sheet} has a part that gives no value.");
                (string input, string value) = (part[..equals], part[(equals + 1)..]);
                if (input == own)
                {
                    ownSeen = true;
                    matches &= value == shown;
                }
                else
                {
                    matches &= value == "-";
                }
            }

            if (matches && ownSeen)
                found.Add(whole);
        }

        return found.Count == 1 ? found.Single()
            : throw new InvalidDataException($"The sheet {sheet} has {found.Count} frames for {subject} showing \"{shown}\", where one was expected.");
    }

    /// <summary>What follows the name of a picture's part that lies in front, in the name of its frame.</summary>
    private const string PictureInFront = "^front";

    /// <summary>
    /// One picture of an effect sheet in one shade, whole: its part behind with its part in front laid over it.
    /// </summary>
    private (RgbaPixel[] Pixels, int Width, int Height) EffectPicture(string sheet, string frame, string shade)
    {
        bool behind = _sheets.HasFrame(RoomBlockFamily, sheet, frame), front = _sheets.HasFrame(RoomBlockFamily, sheet, frame + PictureInFront);
        if (!behind && !front)
            throw new InvalidDataException($"The sheet {sheet} has no frame {frame}.");
        if (!front)
            return _sheets.FrameShown(RoomBlockFamily, sheet, frame, shade);
        (RgbaPixel[] over, int width, int height) = _sheets.FrameShown(RoomBlockFamily, sheet, frame + PictureInFront, shade);
        if (!behind)
            return (over, width, height);
        (RgbaPixel[] under, int underWidth, int underHeight) = _sheets.FrameShown(RoomBlockFamily, sheet, frame, shade);
        if (underWidth != width || underHeight != height)
            throw new InvalidDataException($"The two parts of the frame {frame} of the sheet {sheet} differ in size.");
        for (int pixel = 0; pixel < under.Length; pixel++)
        {
            if (over[pixel].A != 0)
                under[pixel] = over[pixel];
        }

        return (under, width, height);
    }

    /// <summary>
    /// Writes the pictures of one thing an effect shows, for one state's set of graphics, and gives the timeline that
    /// plays them: the frames of the effect's animation in turn where it has one, and one still picture otherwise. The
    /// timeline names the pictures with the set of graphics left for the room to put in, and lays them so that the
    /// pixel the old game places lies where the thing is: a liquid's strip has its surface a little way down it.
    /// </summary>
    private JsonObject EffectTimeline(RoomStateSettings state, RoomEffectDefinition effect, string subject, string folder, string file, HashSet<string> written)
    {
        string sheet = EffectSheet(state);
        string shade = RoomEffectTintShading.Of(effect.Tint).Key;
        string graphics = GraphicsFolder(state);

        (int X, int Y)? placed = null;

        string Picture(BlockAnimation? animation, string shown, string suffix)
        {
            string named = $"{folder}/{{0}}/{file}{suffix}.png";
            string path = string.Format(System.Globalization.CultureInfo.InvariantCulture, named, graphics);
            string frame = EffectFrame(sheet, subject, animation, shown);
            (int originX, int originY, int _, int _) = _sheets.FramePlace(RoomBlockFamily, sheet,
                _sheets.HasFrame(RoomBlockFamily, sheet, frame) ? frame : frame + PictureInFront);
            if ((placed ??= (originX, originY)) != (originX, originY))
                throw new InvalidDataException($"The pictures of {subject} on the sheet {sheet} are not all placed alike.");
            if (written.Add(path))
            {
                (RgbaPixel[] pixels, int width, int height) = EffectPicture(sheet, frame, shade);
                _tree.WritePicture(path, width, height, pixels);
            }

            return string.Format(System.Globalization.CultureInfo.InvariantCulture, named, "{graphics}");
        }

        JsonObject timeline = BlockAnimationLoopDefinitions.EffectOverlayAnimations.TryGetValue(effect.Type, out BlockAnimation animation)
            ? EngineTimelines.Of<ArtworkFrame, NoAnimationCue>([], BlockAnimationLoopDefinitions.IdleLoops[animation].Select(IAnimationBeat<ArtworkFrame, NoAnimationCue> (beat) => beat),
                frame => Picture(animation, frame.Name, "_" + EngineSheets.FileName(frame.Name)), _ => null)
            : new JsonObject
            {
                ["steps"] = new JsonArray(new JsonObject { ["picture"] = Picture(null, "", ""), ["updates"] = 1 }),
                ["loops_to"] = 0,
            };
        (int placedX, int placedY) = placed ?? throw new InvalidDataException($"The effect {effect.Type} shows no picture of {subject}.");
        if ((placedX, placedY) != (0, 0))
            timeline["offset"] = new JsonObject { ["x"] = -placedX, ["y"] = -placedY };
        return timeline;
    }

    /// <summary>
    /// Writes every liquid's type, and every overlay's look, with their pictures for each set of graphics a room shows
    /// them in; and the haze of the space station.
    /// </summary>
    /// <exception cref="InvalidDataException">Two rooms' effects of one name differ in what the name's file says.</exception>
    private void WriteLiquidsAndOverlays()
    {
        var pictures = new HashSet<string>(StringComparer.Ordinal);
        var files = new Dictionary<string, string>(StringComparer.Ordinal);

        void Write(string path, JsonObject data)
        {
            string text = data.ToJsonString();
            if (!files.TryGetValue(path, out string? before))
            {
                files[path] = text;
                _tree.WriteData(path, data);
            }
            else if (before != text)
            {
                throw new InvalidDataException($"Two rooms' effects are both {path} and differ: {before} and {text}.");
            }
        }

        foreach ((RoomDefinition _, RoomStateSettings state) in EngineLayouts.States())
        {
            foreach (RoomEffectDefinition effect in state.Fx?.Effects ?? [])
            {
                if (RoomFxTypes.ShowsLiquid(effect.Type))
                {
                    string name = LiquidFileName(effect);
                    Write($"liquids/{name}.json", LiquidType(effect,
                        EffectTimeline(state, effect, RoomEffectPictures.SurfaceName(effect.Type), "liquids", name + "_surface", pictures),
                        EffectTimeline(state, effect, RoomEffectPictures.BodyName(effect.Type), "liquids", name + "_body", pictures)));
                }
                else if (RoomFxOverlayState.ShowsOnOverlay(effect.Type))
                {
                    string name = EngineSheets.FileName(effect.Type.ToString());
                    Write($"effects/{name}.json", OverlayLook(effect,
                        EffectTimeline(state, effect, RoomEffectPictures.AtmosphereName(effect.Type), "effects", name, pictures)));
                }
            }
        }

        // How what lies behind a room wavers with the room's effect, where the effect says so (the rooms' generator
        // places it): behind water the rows below the surface are moved across, each step drawing the table a row
        // further up; behind lava and acid, and in the heat of a room with neither in sight, every row sways up and
        // down, each step drawing it a row further down, its first step counted as shown for an update as the room
        // loads.
        _tree.WriteData("effects/backdrop_wave_water.json", BackdropWave(
            WaterWaveDefinitions.BackgroundWaveform, WaterWaveDefinitions.BackgroundWavePhaseDuration, down: false, belowSurface: true, shownAtStart: 0));
        _tree.WriteData("effects/backdrop_wave_heat.json", BackdropWave(
            LavaAcidWaveDefinitions.HazeWaveforms[HeatHazeDirection.Vertical], LavaAcidWaveDefinitions.VerticalWavePhaseDuration,
            down: true, belowSurface: false, shownAtStart: LavaAcidWaveDefinitions.HazeStart.UpdatesShown));
        WriteCeresHaze();
    }

    /// <summary>
    /// A wave of the rows of what lies behind a room, as the engine's effect for it reads one: its table of whole pixels, the
    /// updates each step shows for, which way the rows are moved, whether only the rows below the surface are, and
    /// how many updates its first step counts as shown as the room loads. One that waves every row moves down a row
    /// each step, and one that waves below the surface moves up.
    /// </summary>
    private static JsonObject BackdropWave(IEnumerable<int> offsets, int updates, bool down, bool belowSurface, int shownAtStart, bool? movesDown = null) => new()
    {
        ["offsets"] = new JsonArray([.. offsets.Select(offset => (JsonNode)offset)]),
        ["updates"] = updates,
        ["down"] = down,
        ["below_surface"] = belowSurface,
        ["moves_down"] = movesDown ?? !belowSurface,
        ["shown_at_start"] = shownAtStart,
    };

    /// <summary>The data of a liquid's type: what it does to Samus, and how it looks.</summary>
    private static JsonObject LiquidType(RoomEffectDefinition effect, JsonObject surface, JsonObject body)
    {
        string kind = LiquidKind(effect.Type);
        var type = new JsonObject
        {
            ["name"] = kind,

            // Samus has her own way of moving in each liquid, so none slows her by a share of her speed in air.
            ["speed_times"] = 1,
            ["speed_over"] = 1,
            ["slowing_cancelled_by"] = new JsonArray(GravitySuit),
            ["surface"] = surface,
            ["body"] = body,
            ["blend"] = BlendName(effect),
            ["drift"] = effect.Liquid is RoomWater ? -WaterWaveDefinitions.SurfaceDriftSpeed.Pixels : 0m,
        };

        // Water's own pictures ripple below its surface: their rows moved across by the surface's table, counted
        // from the row below the surface, each step drawing it a row further down. Lava and acid lie still.
        if (effect.Liquid is RoomWater)
        {
            type["ripple"] = BackdropWave(
                WaterWaveDefinitions.SurfaceWaveform, WaterWaveDefinitions.OverlayWavePhaseDuration, down: false, belowSurface: true, shownAtStart: 0, movesDown: true);
        }

        // Lava and acid throw up nothing as she crosses their surface: their spray is Samus's, while they harm her.
        switch (effect.Type)
        {
            case RoomFxType.Lava:
                type["damage_hundredths"] = Hundredths(SamusLiquidDamageDefinitions.Lava);
                type["damage_cancelled_by"] = new JsonArray(GravitySuit);
                break;
            case RoomFxType.Acid:
                type["damage_hundredths"] = Hundredths(SamusLiquidDamageDefinitions.Acid);
                break;
            default:
                type["splash"] = SamusParticleFile(SamusParticleKind.DivingSplash);
                type["splash_sound"] = EngineNames.Sound(EnemySound.SplashIn);
                type["splash_out_sound"] = EngineNames.Sound(EnemySound.SplashOut);
                break;
        }

        return type;
    }

    /// <summary>
    /// The look of an overlay: its pictures and how they move. The old game moves what the screen shows of a picture,
    /// so its pictures move the other way.
    /// </summary>
    private static JsonObject OverlayLook(RoomEffectDefinition effect, JsonObject timeline)
    {
        (SignedEightEight across, SignedEightEight down, bool followsRoom) = effect.Type switch
        {
            RoomFxType.Rain => (SignedEightEight.Zero, AtmosphereDriftDefinitions.RainFallSpeed, false),
            RoomFxType.Spores => (SignedEightEight.Zero, AtmosphereDriftDefinitions.SporesRiseSpeed, true),
            RoomFxType.Fog => (-AtmosphereDriftDefinitions.FogDriftSpeed, AtmosphereDriftDefinitions.FogRiseSpeed, true),
            _ => throw new InvalidDataException($"How the overlay of the effect {effect.Type} moves is not known."),
        };
        var look = new JsonObject
        {
            ["timeline"] = timeline,
            ["drift"] = new JsonObject { ["x"] = across.Pixels, ["y"] = down.Pixels },
            ["follows_room"] = followsRoom ? 1 : 0,
            ["blend"] = BlendName(effect),
            ["behind"] = effect.EffectBlend is OverlayBehindRoomBlend,
        };
        if (effect.Type == RoomFxType.Rain)
        {
            var winds = new JsonArray();
            foreach (RainWind wind in Enum.GetValues<RainWind>())
                winds.Add(-AtmosphereDrift.RainHorizontalVelocity(wind).Pixels);
            look["winds"] = winds;
            look["winds_from"] = SuperMetroid.Core.Compatibility.RainWindRandomRoll.Wind.Shift;
        }

        return look;
    }

    /// <summary>
    /// Writes the haze of the space station, in the colour it has while Ridley is there and the one it has once he is
    /// gone: no colour at the top of the screen, and from part way down a band of it, each band stronger than the one
    /// above, laid over the room to brighten it.
    /// </summary>
    private void WriteCeresHaze()
    {
        CeresHazeTints tints = _installation.LoadMaps().CeresHaze;
        foreach ((string name, RgbColor tint) in new[] { ("ceres_haze", tints.WhileRidleyLives), ("ceres_haze_escape", tints.AfterRidleyDies) })
        {
            const int width = ScreenGeometry.ScreenWidthPixels, height = ScreenGeometry.ScreenHeightPixels;
            const int steps = CeresHazeRenderDefinitions.MaximumComponent;
            var pixels = new RgbaPixel[width * height];
            for (int line = CeresHazeRenderDefinitions.RampFirstLine; line < height; line++)
            {
                int strength = Math.Min(steps, 1 + (line - CeresHazeRenderDefinitions.RampFirstLine) / CeresHazeRenderDefinitions.BandHeight);
                var shown = new RgbaPixel(Channel(tint.Red), Channel(tint.Green), Channel(tint.Blue));
                pixels.AsSpan(line * width, width).Fill(shown);

                byte Channel(int whole) => (byte)Math.Min(RgbColor.FullChannel, (int)Math.Round(whole * strength / (double)steps, MidpointRounding.AwayFromZero));
            }

            _tree.WritePicture($"effects/{name}.png", width, height, pixels);
            _tree.WriteData($"effects/{name}.json", new JsonObject
            {
                ["timeline"] = new JsonObject
                {
                    ["steps"] = new JsonArray(new JsonObject { ["picture"] = $"effects/{name}.png", ["updates"] = 1 }),
                    ["loops_to"] = 0,
                },
                ["drift"] = new JsonObject { ["x"] = 0, ["y"] = 0 },
                ["follows_room"] = 0,
                ["blend"] = "add",
                ["behind"] = false,
            });
        }
    }
}
