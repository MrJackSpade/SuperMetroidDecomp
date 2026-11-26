using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The things of the Godot game that are not enemies there but were enemies of the old game: their pictures are on
/// the old enemies' sheets, and their data is begun as a room object's.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>
    /// Cuts the pictures of old enemies' sheets into <paramref name="folder"/>, for a thing that is not an enemy of
    /// the Godot game. Each sheet is named as the old enemy is.
    /// </summary>
    private EngineSheetPictures EnemyPictures(string folder, params ReadOnlySpan<string> enemies)
    {
        var named = new List<string>();
        foreach (string enemy in enemies)
            named.Add(_sheets.EnemySheet(enemy));
        return _sheets.Cut(_tree, GameInstallationLayout.EnemySheetDirectoryName, folder, [.. named]);
    }

    /// <summary>
    /// The colours one shade of an old enemy's sheet shows changed, as the engine reads one place's change of colours:
    /// every colour the shade changes as the sheet's pictures have it, with the colour the shade shows for it, each as
    /// six hexadecimal digits.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The shade changes more colours than a thing shows changed at once, or changes a colour in one region of the
    /// pictures only, which pictures cut from an enemy's sheet have no map of.
    /// </exception>
    private JsonObject EnemySheetSwaps(string sheet, string shade)
    {
        static string Written(int colour) => colour.ToString("x6", System.Globalization.CultureInfo.InvariantCulture);
        IReadOnlyList<ShadeSwap> swaps = _sheets.ShadeSwaps(GameInstallationLayout.EnemySheetDirectoryName, sheet, shade);
        if (swaps.Count > MostSwapsShown)
            throw new InvalidDataException($"The shade {shade} changes {swaps.Count} colours of {sheet}; a thing shows at most {MostSwapsShown} changed at once.");
        var written = new JsonObject();
        foreach (ShadeSwap swap in swaps)
        {
            if (swap.Region != ShadeSwap.Everywhere)
                throw new InvalidDataException($"The shade {shade} changes a colour of {sheet} in one region of its pictures only, and pictures cut from an enemy's sheet have no map of regions.");
            written[Written(swap.From)] = Written(swap.To);
        }

        return written;
    }

    /// <summary>The picture of one frame among pictures cut from old enemies' sheets; none for the frame that shows nothing.</summary>
    private static string PictureOf(EngineSheetPictures pictures, EnemyFrameId frame) =>
        frame == EnemyFrameId.PointAtOrigin ? "" : pictures.Of(EnemySheetDefinitions.FrameName(frame));

    /// <summary>A timeline of frames shown one after another, each for its updates, played once or over and over.</summary>
    private static JsonObject FramesTimeline(EngineSheetPictures pictures, IEnumerable<(EnemyFrameId Frame, int Updates)> frames, bool loops)
    {
        var steps = new JsonArray();
        foreach ((EnemyFrameId frame, int updates) in frames)
            steps.Add(new JsonObject { ["picture"] = PictureOf(pictures, frame), ["updates"] = updates });
        var timeline = new JsonObject { ["steps"] = steps };
        if (loops)
            timeline["loops_to"] = 0;
        return timeline;
    }

    /// <summary>A timeline that shows nothing.</summary>
    private static JsonObject NothingShown() =>
        new() { ["steps"] = new JsonArray(new JsonObject { ["picture"] = "", ["updates"] = 1 }), ["loops_to"] = 0 };

    /// <summary>The frames a run of beats shows, each with how long it shows.</summary>
    private static List<(EnemyFrameId Frame, int Updates)> FramesOf<TEvent>(IEnumerable<IAnimationBeat<EnemyFrameId, TEvent>> beats)
        where TEvent : notnull =>
        [.. beats.OfType<AnimationBeat<EnemyFrameId, TEvent>.Frame>().Select(frame => (frame.Shown, frame.Duration))];

    /// <summary>Writes the things that were enemies of the old game and are room objects of the Godot game.</summary>
    private void WriteObjects()
    {
        // The elevator's pad blinks whether it is at rest or carrying her.
        EngineSheetPictures pad = EnemyPictures("objects/elevator", EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.Elevator));
        new EngineObjectFile(_tree, "objects/elevator.json", "Elevator")
            .PictureOffset(pad)
            .Timeline("rest", EngineTimelines.Of(
                ElevatorAnimations.Segments, ElevatorAnimations.StartingWith(ElevatorSegment.Idle), frame => PictureOf(pad, frame), static (NoAnimationCue _) => null))
            .Write();

        WriteCeresDoors();
        WriteCeresSteam();
        WriteMorphBallEye();
        WriteGunship();
    }

    /// <summary>The gunship: its two halves, its hatch opening and closing, and the dust its engines raise.</summary>
    private void WriteGunship()
    {
        const string folder = "objects/gunship";
        EngineSheetPictures pictures = EnemyPictures(
            folder, EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.ShipTop), EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.ShipBottomEntrance));
        string Pad(string name, GunshipSegment segment)
        {
            string file = $"{folder}/{name}.json";
            _tree.WriteData(file, EngineTimelines.Of(
                GunshipAnimations.Segments, GunshipAnimations.StartingWith(segment), frame => PictureOf(pictures, frame), static (NoAnimationCue _) => null));
            return file;
        }

        static EnemyFrameId Held(GunshipSegment segment) =>
            GunshipAnimations.Segments.Beats(segment).Select(beat => beat.ShownFrame).FirstOrDefault(frame => frame.HasValue)
            ?? throw new InvalidDataException($"The gunship's {segment} shows nothing.");

        // The dust clouds, in the order of the places they rise from.
        var dust = new JsonArray();
        foreach (GunshipDustPlacement cloud in GunshipDustDefinitions.LiftoffClouds)
        {
            (List<IAnimationBeat<EnemyProjectileFrameId, GunshipDustCue>> lead, List<IAnimationBeat<EnemyProjectileFrameId, GunshipDustCue>>? cycle) =
                EngineTimelines.Played(GunshipDustAnimations.Segments, GunshipDustAnimations.StartingWith(cloud.Animation));
            string file = $"{folder}/dust_{EngineSheets.FileName(cloud.Animation.ToString())}.json";
            IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue)
                .Select(frame => EnemySheetDefinitions.ProjectileFrameName(frame!.Value));
            EngineSheetPictures clouds = _sheets.CutSpawned(_tree, file[..file.LastIndexOf('.')], EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.ShipTop), frames);
            JsonObject timeline = EngineTimelines.Of(lead, cycle, frame => clouds.Of(EnemySheetDefinitions.ProjectileFrameName(frame)), static (GunshipDustCue _) => null);
            timeline["offset"] = new JsonObject { ["x"] = clouds.OffsetX, ["y"] = clouds.OffsetY };
            _tree.WriteData(file, timeline);
            dust.Add(file);
        }

        // Its lights' glow: one of the room's changes of colours, on the pictures of both its sheets.
        string glow = $"{folder}/glow.json";
        IReadOnlyList<string> glowSheets =
        [
            _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.ShipTop)),
            _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.ShipBottomEntrance)),
        ];
        _tree.WriteData(glow, ColourSteps(GunshipLightDefinitions.Glow, shade => ShadeChange(
            shade, GameInstallationLayout.EnemySheetDirectoryName, new Dictionary<string, IReadOnlyList<string>> { [folder] = glowSheets })));

        new EngineObjectFile(_tree, "objects/gunship.json", "Gunship")
            .PictureOffset(pictures)
            .Value("glow", glow)
            .Value("top", PictureOf(pictures, Held(GunshipSegment.TopHull)))
            .Value("bottom", PictureOf(pictures, Held(GunshipSegment.BottomHull)))
            .Value("bottom_below_top", GunshipMotionDefinitions.TopHullAboveSamus + GunshipMotionDefinitions.BottomHullBelowSamus)
            .Value("pad_above_top", GunshipMotionDefinitions.PadAboveTopHull)
            .Value("pad_closed", Pad("pad_closed", GunshipSegment.BottomEntrancePad))
            .Value("pad_opening", Pad("pad_opening", GunshipSegment.EntrancePadOpening))
            .Value("pad_closing", Pad("pad_closing", GunshipSegment.EntrancePadClosing))
            .Value("hover_sound", EngineNames.SoundOrStop(EnemySound.GunshipHover))
            .Value("pad_opening_sound", EngineNames.SoundOrStop(RoomSound.GunshipPadOpening))
            .Value("pad_closing_sound", EngineNames.SoundOrStop(RoomSound.GunshipPadClosing))
            .Value("saving_sound", EngineNames.Sound(SamusSound.Saving))
            .Value("saving_updates", MessageBoxPauses.UpdatesBeforeGunshipSavingWait + GameplayMessageBoxDefinitions.Timing.GunshipSavingSoundFrames)
            .Value("saved_message_hold_updates", MessageHold(GameplayMessageId.SaveCompleted))
            .Value("saved_message_before_opening_updates", UpdatesOf(pauses => pauses.BeginBeforeCompletionNotice()))
            .Group("dust_effects", dust)
            .Write();
    }

    /// <summary>The eye in the wall by the Morph Ball: shut, opening and closing for each way it faces, its looks round the turn, and its sockets.</summary>
    private void WriteMorphBallEye()
    {
        EngineSheetPictures pictures = EnemyPictures("objects/morph_ball_eye", EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.MorphBallEye));
        JsonObject Look(MorphBallEyeSegment segment) => EngineTimelines.Of(
            MorphBallEyeAnimations.Segments, MorphBallEyeAnimations.StartingWith(segment), frame => PictureOf(pictures, frame), static (NoAnimationCue _) => null);

        // The old game's looks go round from straight up, clockwise; the Godot game's from the right.
        IReadOnlyList<EnemyFrameId> looks = MorphBallEyeAnimations.AimFrames.InTurnOrder;
        if (looks.Count % 4 != 0)
            throw new InvalidDataException("The Morph Ball eye's looks do not divide a quarter turn evenly.");
        var aims = new JsonArray();
        for (int index = 0; index < looks.Count; index++)
            aims.Add(PictureOf(pictures, looks[(index + looks.Count / 4) % looks.Count]));

        var mounts = new JsonObject();
        foreach ((string side, MorphBallEyeMountSide mounted) in new[]
                 {
                     ("left", MorphBallEyeMountSide.Left), ("right", MorphBallEyeMountSide.Right),
                     ("above", MorphBallEyeMountSide.Above), ("below", MorphBallEyeMountSide.Below),
                 })
        {
            MorphBallEyeMountDefinition mount = MorphBallEyeGeometry.Mount(mounted);
            EnemyFrameId socket = MorphBallEyeAnimations.Segments.Beats(mount.Animation).Select(beat => beat.ShownFrame).FirstOrDefault(frame => frame.HasValue)
                ?? throw new InvalidDataException($"The Morph Ball eye's socket on its {side} side shows nothing.");
            mounts[side] = new JsonObject { ["picture"] = PictureOf(pictures, socket), ["x"] = mount.Offset.X, ["y"] = mount.Offset.Y };
        }

        new EngineObjectFile(_tree, "objects/morph_ball_eye.json", "MorphBallEye")
            .PictureOffset(pictures)
            .Value("transition_updates", MorphBallEyeWatchDefinitions.TransitionDuration)
            .Value("activation_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.EyeActivationSound))
            .Value("beam_stop_sound", EngineNames.SoundOrStop(EnemySoundEffectDefinitions.EyeRaySoundStop))
            .Group("aims", aims)
            .Group("mounts", mounts)
            .Timeline("closed_left", Look(MorphBallEyeSegment.FacingLeftClosed))
            .Timeline("closed_right", Look(MorphBallEyeSegment.FacingRightClosed))
            .Timeline("opening_left", Look(MorphBallEyeSegment.FacingLeftActivating))
            .Timeline("opening_right", Look(MorphBallEyeSegment.FacingRightActivating))
            .Timeline("closing_left", Look(MorphBallEyeSegment.FacingLeftDeactivating))
            .Timeline("closing_right", Look(MorphBallEyeSegment.FacingRightDeactivating))
            .Write();
    }

    /// <summary>
    /// The doors of the Ceres station, which slide open as Samus comes near and shut behind her, and the door of
    /// Ridley's room, which shuts as she comes in and is an ordinary door once he has gone.
    /// </summary>
    private void WriteCeresDoors()
    {
        RoomEnemyDefinition definition = RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.CeresDoor];
        (int halfWidth, int halfHeight) = (definition.Radius.X, definition.Radius.Y);

        EngineObjectFile Door(string file, EngineSheetPictures pictures, HorizontalDirection facing)
        {
            AnimationSegments<CeresDoorSegment, EnemyFrameId, CeresDoorCue> segments = CeresDoorAnimations.Segments(facing);
            // It slides shut a picture at a time, and open the same way backwards; shut, it shows its last picture.
            List<(EnemyFrameId Frame, int Updates)> closing = FramesOf(segments.Beats(CeresDoorSegment.Close));
            List<(EnemyFrameId Frame, int Updates)> shut = FramesOf(segments.Beats(CeresDoorSegment.ClosedWait));
            if (closing.Count == 0 || shut.Count == 0)
                throw new InvalidDataException("A Ceres door has no pictures of itself shutting, or of itself shut.");
            return new EngineObjectFile(_tree, file, "Barrier")
                .PictureOffset(pictures)
                // Nothing wears it down: it opens only by itself.
                .Value("health", 1)
                .Value("any_hit", 0)
                .Value("open_sound", EngineNames.Sound(RoomSound.CeresDoorOpening))
                .Collider("Solid", -halfWidth, -halfHeight, 2 * halfWidth, 2 * halfHeight)
                // It was an enemy that ignores shots, solid to Samus alone: shut, a shot is spent on its size unseen
                // and a plasma beam flies on, by the reactions its old definition comes to.
                .Collider("Hurtbox", -halfWidth, -halfHeight, 2 * halfWidth, 2 * halfHeight)
                .Group("reactions", EngineReactions.Of(RoomEnemyDefinitionCatalog.CeresDoorAndWall))
                .Timeline("closed", FramesTimeline(pictures, [(shut[0].Frame, 1)], loops: true))
                .Timeline("opening", FramesTimeline(pictures, Enumerable.Reverse(closing), loops: false))
                .Timeline("open", NothingShown())
                .Timeline("closing", FramesTimeline(pictures, closing, loops: false));
        }

        EngineSheetPictures door = EnemyPictures("objects/ceres_door", EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.CeresDoor));
        foreach (HorizontalDirection facing in new[] { HorizontalDirection.Left, HorizontalDirection.Right })
            Door($"objects/ceres_door_{EngineNames.Side(facing)}.json", door, facing).Write();

        // The door of Ridley's room faces right. Held shut, it first shows itself sliding shut and then stays so.
        EngineSheetPictures ridleys = EnemyPictures("objects/ceres_ridley_room_door", EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.CeresRidleyRoomDoor));
        List<(EnemyFrameId Frame, int Updates)> sealing =
            FramesOf(CeresDoorAnimations.Segments(HorizontalDirection.Right).Beats(CeresDoorSegment.RidleyRoomDoor));
        if (sealing.Count == 0)
            throw new InvalidDataException("The door of Ridley's room has no pictures of itself shutting.");
        Door("objects/ceres_ridley_room_door.json", ridleys, HorizontalDirection.Right)
            .Timeline("sealing", FramesTimeline(ridleys, sealing, loops: false))
            .Timeline("sealed", FramesTimeline(ridleys, [(sealing[^1].Frame, 1)], loops: true))
            .Write();

        // The blocked way to the elevator, which the station's self-destruct blows apart.
        EngineSheetPictures blocked = EnemyPictures("objects/ceres_elevator_door", EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.CeresElevatorDoor));
        new EngineObjectFile(_tree, "objects/ceres_elevator_door.json", "CeresElevatorDoor")
            .PictureOffset(blocked)
            .Value("blocked", PictureOf(blocked, EnemyFrameId.CeresElevatorDoorBlocked))
            .Value("explosion", DustFile(DustExplosionAnimation.SmallExplosion))
            .Value("smoke", DustFile(DustExplosionAnimation.Smoke))
            .Value("explosion_sound", EngineNames.Sound(EnemySound.BigExplosion))
            .Write();

        // Once Ridley has escaped the old game draws the station's doors in other colours. The pictures are written
        // once, in their sheets' own; the change of colours is written beside them, place by place, and the rooms'
        // doors are shown it where they are drawn.
        var doorSheets = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            ["objects/ceres_door"] = [_sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.CeresDoor))],
            ["objects/ceres_ridley_room_door"] = [_sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.CeresRidleyRoomDoor))],
            ["objects/ceres_elevator_door"] = [_sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.CeresElevatorDoor))],
        };
        _tree.WriteData("objects/ceres_doors_escaping.json", new JsonObject
        {
            ["colours"] = ShadeChange(SuperMetroid.Core.Areas.Ceres.Artwork.CeresDoorShades.Escaping.Key, GameInstallationLayout.EnemySheetDirectoryName, doorSheets),
        });
    }

    /// <summary>
    /// The steam of the Ceres station: a vent puffs, its plume reaching farther with each picture, and then rests.
    /// What it covers follows the plume; touching it is a hit that does no damage.
    /// </summary>
    private void WriteCeresSteam()
    {
        EngineSheetPictures pictures = EnemyPictures("objects/ceres_steam", EnemyDefinitionIdKeys.Installed.Of(EnemyDefinitionId.SteamVent));
        RoomEnemyDefinition definition = RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.SteamVent];
        foreach ((string name, CeresSteamWay way) in new[]
                 {
                     ("up", CeresSteamWay.Up), ("down", CeresSteamWay.Down), ("left", CeresSteamWay.Left), ("right", CeresSteamWay.Right),
                 })
        {
            var steps = new JsonArray();
            foreach (IAnimationBeat<EnemyFrameId, CeresSteamCueKind> beat in way.Segments.Beats(CeresSteamSegment.Puffing))
            {
                if (beat is not AnimationBeat<EnemyFrameId, CeresSteamCueKind>.Frame frame)
                    throw new InvalidDataException("A puff of Ceres steam has a beat that is not a picture.");
                if (!CeresSteamCollisionDefinitions.Plumes.TryGetValue(frame.Shown, out EnemyHitboxRectangle? plume))
                    throw new InvalidDataException($"The steam picture {frame.Shown} has no plume given for it.");
                // The old rectangle names its last column and row; the Godot game's region is a width and a height.
                string reach = plume is { } covers
                    ? $"area:{covers.Left}:{covers.Top}:{covers.Right - covers.Left + 1}:{covers.Bottom - covers.Top + 1}"
                    : "safe";
                steps.Add(new JsonObject { ["picture"] = PictureOf(pictures, frame.Shown), ["updates"] = frame.Duration, ["cues"] = new JsonArray(reach) });
            }

            // Between puffs it shows nothing and covers nothing.
            steps.Add(new JsonObject { ["picture"] = "", ["updates"] = CeresSteamAnimations.RestUpdates, ["cues"] = new JsonArray("safe") });
            EngineObjectFile steam = new EngineObjectFile(_tree, $"objects/ceres_steam_{name}.json", "Hazard")
                .PictureOffset(pictures)
                .Value("hit_damage", definition.Damage)
                .Timeline("rest", new JsonObject { ["steps"] = steps, ["loops_to"] = 0 });
            if (definition.Damage == 0)
                steam.Value("hits_without_damage", true);
            steam.Write();
        }
    }
}
