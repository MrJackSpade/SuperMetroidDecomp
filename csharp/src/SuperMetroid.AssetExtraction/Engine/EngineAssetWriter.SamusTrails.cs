using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Spatial;
using Segment = SuperMetroid.Core.Samus.Projectiles.SamusProjectileSegment;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The trails Samus's shots leave: every so many updates of its flight a shot leaves a sparkle on each side of it, or
/// smoke behind a missile, each placed from where the shot stands by the picture it shows and the way it flies, and
/// each playing a short timeline of its own in the colours of the shot's beams. A shot's trail is a file of its own
/// for each way it flies, one entry for each step of its flying timeline.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The folder the trails and their sparkles' timelines are written in.</summary>
    private const string TrailFolder = "samus/weapons/trails";

    /// <summary>Each sparkle's timeline written so far, by the folder of its sheet and its name, with what it shows.</summary>
    private readonly Dictionary<string, string> _sparkles = new(StringComparer.Ordinal);

    /// <summary>
    /// One way a shot flies, as its trail is written for it: the name of the way in the trail's file name (none for a
    /// shot that flies one way only), the old direction of it and of its twin where two give the one way, and the
    /// animation it plays flying so.
    /// </summary>
    private sealed record ShotWay(string? Name, SamusProjectileDirection Direction, SamusProjectileDirection? Twin, Segment Animation);

    /// <summary>The ways a shot that can be launched every way flies, each with the animation it plays.</summary>
    private static ShotWay[] FlyingWays(Func<SamusProjectileDirection, Segment> animation) =>
        [.. ShotDirections.Select(way => new ShotWay(way.Name, way.Direction, way.Twin, animation(way.Direction)))];

    /// <summary>
    /// The updates between a beam shot's trails after its first: its flight's, which for a shot with the wave beam is
    /// shorter while it is uncharged and has neither the spazer nor the plasma beam.
    /// </summary>
    private static int BeamTrailInterval(SamusBeamCombination beams, bool charged)
    {
        SamusShotFiring flight = SamusBeamShotBehaviors.Resolve(charged, beams);
        return flight switch
        {
            WaveBeamFlight wave => wave.TrailInterval,
            BeamFlight => ProjectileTimingDefinitions.TrailInterval,
            _ => throw new InvalidDataException($"A {beams} beam shot flies as {flight.GetType().Name}, whose trail this does not know."),
        };
    }

    /// <summary>
    /// Writes the trail a shot of <paramref name="type"/>, pictured with <paramref name="pictured"/>, leaves flying each of
    /// <paramref name="ways"/>, and gives the path that names it, with the way's place in it where there is more than
    /// one; null, and nothing written, for a shot that leaves no sparkle whichever way it flies.
    /// </summary>
    private string? ShotTrail(string name, SamusProjectileType type, SamusBeamCombination pictured, int every, IReadOnlyList<ShotWay> ways)
    {
        // The two sparkles' timelines; a side the shot leaves nothing on has none.
        ProjectileTrailAnimations pair = ProjectileTrailSelection.Select(type);
        string? left = SparkleTimeline(pictured, pair, left: true), right = SparkleTimeline(pictured, pair, left: false);

        // Each step leaves the sparkles its picture's placement gives, from where the shot stands; a picture the list
        // has no placement for leaves none, and so does a shot whose beams name no list.
        JsonObject Trail(SamusProjectileDirection direction, Segment animation)
        {
            ProjectileTrailPlacementList? list = type.Echo == SamusBeamEcho.SpazerComboTrail
                ? ProjectileTrailPlacements.SpazerAttackTrailingPairList(direction)
                : ProjectileTrailPlacements.List(type.IsChargedBeam ? ProjectileTrailFamilyGroup.Charged : ProjectileTrailFamilyGroup.Uncharged,
                    type.BeamCombination, direction);
            var steps = new JsonArray();
            foreach (JsonNode? step in NamedShotTimeline(animation)["steps"]!.AsArray())
            {
                var sparkles = new JsonArray();
                SamusProjectileFrameId frame = ShotFrameNamed(step!["picture"]!.GetValue<string>());
                if (list is { } placements && ProjectileTrailPlacements.Start(placements, frame) is { } placement)
                {
                    if (left is not null)
                        sparkles.Add(new JsonObject { ["x"] = placement.Left.X, ["y"] = placement.Left.Y, ["timeline"] = left });
                    if (right is not null)
                        sparkles.Add(new JsonObject { ["x"] = placement.Right.X, ["y"] = placement.Right.Y, ["timeline"] = right });
                }

                steps.Add(sparkles);
            }

            return new JsonObject { ["first_after"] = ProjectileTimingDefinitions.TrailInterval, ["every"] = every, ["steps"] = steps };
        }

        var written = new List<(string File, JsonObject Trail)>();
        foreach (ShotWay way in ways)
        {
            JsonObject trail = Trail(way.Direction, way.Animation);
            if (way.Twin is { } twin && !JsonNode.DeepEquals(trail, Trail(twin, way.Animation)))
                throw new InvalidDataException($"Samus's {name} shots leave one trail fired straight {way.Name} facing right and another facing left, which the Godot game cannot say.");
            written.Add((way.Name is null ? $"{TrailFolder}/{name}.json" : $"{TrailFolder}/{name}_{way.Name}.json", trail));
        }

        if (written.All(one => one.Trail["steps"]!.AsArray().All(step => step!.AsArray().Count == 0)))
            return null;
        foreach ((string file, JsonObject trail) in written)
            _tree.WriteData(file, trail);
        return ways.Count == 1 && ways[0].Name is null ? $"{TrailFolder}/{name}.json" : $"{TrailFolder}/{name}_{{direction}}.json";
    }

    /// <summary>
    /// Writes, once for each sheet, the timeline one side of a trail of <paramref name="pair"/> plays in the pictures of
    /// <paramref name="pictured"/>, and gives its path; null for a side the trail leaves nothing on. Each step is a
    /// picture of the sheet, shown as far below where the sparkle was left as the old trail shows it there.
    /// </summary>
    private string? SparkleTimeline(SamusBeamCombination pictured, ProjectileTrailAnimations pair, bool left)
    {
        ProjectileTrailSegment segment = left ? pair.Left : pair.Right, other = left ? pair.Right : pair.Left;
        List<(ProjectileTrailFrame Frame, int Below)> played = SparklePlayed(pair, left);
        if (played.Count == 0)
            return null;

        // A sparkle is named for what it plays; one that falls otherwise beside the sparkle it is left with than its
        // animation does alone on its own side is named for that as well.
        static string Named(ProjectileTrailSegment sparkle) => sparkle is ProjectileTrailSegment.LeftIce or ProjectileTrailSegment.RightIce
            ? "ice"
            : EngineSheets.FileName(sparkle.ToString());
        bool ownSideIsLeft = segment != ProjectileTrailSegment.RightIce;
        ProjectileTrailAnimations alone = ownSideIsLeft
            ? new ProjectileTrailAnimations(segment, ProjectileTrailSegment.EndsWithoutSparkle)
            : new ProjectileTrailAnimations(ProjectileTrailSegment.EndsWithoutSparkle, segment);
        string name = SparklePlayed(alone, ownSideIsLeft).SequenceEqual(played)
            ? Named(segment)
            : $"{Named(segment)}_{(left ? "left" : "right")}_beside_{Named(other)}";

        EngineSheetPictures pictures = ShotPictures(pictured);
        string folder = _shotFolders[SpriteSheetDefinitions.FileNames[ProjectileSheetFrames.For(pictured)]];
        string file = $"{TrailFolder}/sparkles/{folder}/{name}.json";
        string shows = string.Join(",", played.Select(one => $"{one.Frame}@{one.Below}"));
        if (_sparkles.TryGetValue(file, out string? before))
        {
            return before == shows
                ? file
                : throw new InvalidDataException($"Two of the old trails' sparkles named {name} play differently, which one file cannot say.");
        }

        var steps = new JsonArray();
        for (int update = 0; update < played.Count;)
        {
            int run = 1;
            while (update + run < played.Count && played[update + run] == played[update])
                run++;
            var step = new JsonObject
            {
                ["picture"] = pictures.Of(ProjectileSheetFrames.TrailFrame(played[update].Frame)),
                ["updates"] = run,
            };
            if (played[update].Below != 0)
                step["offset"] = new JsonObject { ["x"] = 0, ["y"] = played[update].Below };
            steps.Add(step);
            update += run;
        }

        _tree.WriteData(file, new JsonObject { ["steps"] = steps, ["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY } });
        _sparkles[file] = shows;
        return file;
    }

    /// <summary>
    /// What one side of a trail of <paramref name="pair"/> shows on each update from the one it is left on, as the old
    /// trail plays both its sides: the frame, and how far below where it was left. Empty for a side that shows nothing.
    /// The old trail moves a sparkle by the cues of either side's animation and shows each side as it stood when that
    /// side was played, so the two sides are played together here, through the old trail itself.
    /// </summary>
    private static List<(ProjectileTrailFrame Frame, int Below)> SparklePlayed(ProjectileTrailAnimations pair, bool left)
    {
        var trails = new ProjectileTrails();
        var trail = new SamusProjectileTrail(SamusBeamCombination.Power, default(Position16<RoomPixels>));
        trail.Left.Animation = ProjectileTrailSparkleAnimations.Start(pair.Left);
        trail.Right.Animation = ProjectileTrailSparkleAnimations.Start(pair.Right);
        trails.Add(trail);
        SamusProjectileTrailSide side = left ? trail.Left : trail.Right;
        var played = new List<(ProjectileTrailFrame, int)>();
        while (true)
        {
            trails.Advance(timeIsFrozen: false);
            if (side.HasEnded)
                return played;
            if (side.ShownPose is not { } frame)
                throw new InvalidDataException("A sparkle of the old trails shows nothing on an update before its animation ends.");
            if (side.DisplayedOffset.X != 0)
                throw new InvalidDataException("A sparkle of the old trails moves across, which this does not expect.");
            played.Add((frame, side.DisplayedOffset.Y));
            if (played.Count > 4096)
                throw new InvalidDataException("A sparkle of the old trails does not end.");
        }
    }
}
