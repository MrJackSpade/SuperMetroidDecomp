using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Projectiles;
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
        new EngineObjectFile(_tree, "objects/elevator.json", "Carrier")
            .PictureOffset(pad)
            .Timeline("rest", EngineTimelines.Of(
                ElevatorAnimations.Segments, ElevatorAnimations.StartingWith(ElevatorSegment.Idle), frame => PictureOf(pad, frame), static (NoAnimationCue _) => null))
            .Write();

        WriteCeresDoors();
        WriteCeresSteam();
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
