using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The bodies of the big bosses. The old game drew each on a layer of the room's background, so the installation
/// keeps it on a sheet of its own, with a list that says for each of the boss's frames which picture of the sheet it
/// is and where that picture's corner lies from the boss's place.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>A boss's body cut for the Godot game.</summary>
    /// <param name="Boss">The boss, as the installation names its body.</param>
    /// <param name="Pictures">The pictures of its sheet.</param>
    /// <param name="Frames">Each of its frames: the picture it shows, and where the picture's corner lies from the boss's place.</param>
    private sealed partial record BossBody(string Boss, EngineSheetPictures Pictures, IReadOnlyDictionary<string, (string Picture, int X, int Y)> Frames)
    {
        /// <summary>One of its frames.</summary>
        /// <exception cref="InvalidDataException">Its body has no such frame.</exception>
        private (string Picture, int X, int Y) Of(EnemyFrameId frame) => Frames.TryGetValue(EnemySheetDefinitions.FrameName(frame), out (string, int, int) found)
            ? found
            : throw new InvalidDataException($"The body of {Boss} has no frame {EnemySheetDefinitions.FrameName(frame)}.");

        /// <summary>The picture of one of its frames.</summary>
        internal string Picture(EnemyFrameId frame) => Pictures.Of(Of(frame).Picture);

        /// <summary>Where the corner of one of its frames' pictures lies from the boss's place.</summary>
        internal (int X, int Y) Anchor(EnemyFrameId frame)
        {
            (string _, int x, int y) = Of(frame);
            // The sheet's pictures share one corner, which a picture's own place on the canvas is measured from. The
            // body is of the room's background, which lies a row above its place against the boss's sprites.
            return (x + Pictures.OffsetX, y + Pictures.OffsetY - EngineSheets.LayersRowAbove);
        }
    }

    /// <summary>The sheet of a boss's body, as the installation names it.</summary>
    private const string BossBodySheetPrefix = "BossBody.";

    /// <summary>Cuts the body of <paramref name="boss"/> into <paramref name="folder"/>.</summary>
    /// <exception cref="InvalidDataException">The installation keeps no body of that boss.</exception>
    private BossBody CutBossBody(string boss, string folder)
    {
        const string family = GameInstallationLayout.BossBodySheetDirectoryName;
        string list = Path.Combine(_installation.ContentDirectory, family, BossBodySheetFiles.ManifestFileName);
        JsonObject listed = JsonNode.Parse(File.ReadAllText(list))?["sheets"]?[boss]?["frames"]?.AsObject()
            ?? throw new InvalidDataException($"The list of the bosses' bodies {list} has no body of {boss}.");
        var frames = new Dictionary<string, (string Picture, int X, int Y)>(StringComparer.Ordinal);
        foreach ((string frame, JsonNode? placed) in listed)
            frames[frame] = (placed!["picture"]!.GetValue<string>(), placed["anchor"]!["x"]!.GetValue<int>(), placed["anchor"]!["y"]!.GetValue<int>());
        EngineSheetPictures pictures = _sheets.Cut(_tree, family, folder, _sheets.SheetNamed(family, BossBodySheetPrefix + boss));
        return new BossBody(boss, pictures, frames);
    }

    /// <summary>
    /// The timeline of one of a boss's animations whose frames are of its body. Its pictures lie where the body's list
    /// puts each, measured from <paramref name="origin"/>: the corner the boss's data gives every picture of it.
    /// </summary>
    private static JsonObject BossBodyTimeline<TSegment, TEvent>(
        BossBody body, AnimationSegments<TSegment, EnemyFrameId, TEvent> segments, Animation<TSegment> animation, (int X, int Y) origin, Func<TEvent, string?> cue)
        where TSegment : struct
        where TEvent : notnull
    {
        var anchors = new Dictionary<string, (int X, int Y)>(StringComparer.Ordinal);
        string Picture(EnemyFrameId frame)
        {
            if (frame == EnemyFrameId.PointAtOrigin)
                return "";
            string picture = body.Picture(frame);
            (int X, int Y) anchor = body.Anchor(frame);
            if (picture.Length > 0)
            {
                if (anchors.TryGetValue(picture, out (int X, int Y) known) && known != anchor)
                    throw new InvalidDataException($"The body of {body.Boss} shows the picture {picture} in two places.");
                anchors[picture] = anchor;
            }

            return picture;
        }

        JsonObject timeline = EngineTimelines.Of(segments, animation, Picture, cue);
        // The whole timeline lies where its first picture does; a step whose picture lies elsewhere says how far off.
        (int X, int Y)? first = null;
        foreach (JsonNode? step in timeline["steps"]!.AsArray())
        {
            string picture = step!["picture"]!.GetValue<string>();
            if (picture.Length == 0)
                continue;
            (int X, int Y) anchor = anchors[picture];
            first ??= anchor;
            if (anchor != first)
                step["offset"] = new JsonObject { ["x"] = anchor.X - first.Value.X, ["y"] = anchor.Y - first.Value.Y };
        }

        if (first is { } placed && placed != origin)
            timeline["offset"] = new JsonObject { ["x"] = placed.X - origin.X, ["y"] = placed.Y - origin.Y };
        return timeline;
    }

    /// <summary>
    /// Starts the data file of a boss whose body is on a sheet of its own. Every picture of it is measured from the
    /// corner of <paramref name="main"/>, the frame its own visual shows.
    /// </summary>
    private EngineEnemy BossWithBody(string file, string type, EnemyDefinitionId id, BossBody body, EnemyFrameId main)
    {
        var boss = new EngineEnemy(_tree, _sheets, file, type, id, body.Pictures, EngineEnemy.NoPlaces);
        (int x, int y) = body.Anchor(main);
        boss.Data["picture_offset"] = new JsonObject { ["x"] = x, ["y"] = y };
        return boss;
    }
}
