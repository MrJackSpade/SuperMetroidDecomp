using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What a thing launches whose pictures are kept apart from its own, and shots written as files of their own.</summary>
internal sealed partial class EngineEnemy
{
    /// <summary>
    /// Writes the timeline of something it launches, as <c>SpawnedTimeline</c> does, with pictures cut from the
    /// sheets of what another of the old game's things spawns: the old game drew some of what a boss throws from a
    /// sheet named for the thing thrown from. One that <paramref name="rests"/> holds its last picture instead of
    /// ending.
    /// </summary>
    internal string SpawnedTimelineFrom<TSegment, TEvent, TBeat, TShown>(
        string spawner, string name, AnimationSegmentPlay<TSegment, EnemyProjectileFrameId, TEvent, TBeat, TShown> segments, Animation<TSegment> animation,
        Func<TEvent, string?> cue, bool rests = false)
        where TSegment : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<EnemyProjectileFrameId, TEvent>
        where TShown : class
    {
        (List<IAnimationBeat<EnemyProjectileFrameId, TEvent>> lead, List<IAnimationBeat<EnemyProjectileFrameId, TEvent>>? cycle) =
            EngineTimelines.Played(segments, animation);
        string file = $"{Folder}/{name}.json";
        IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue)
            .Select(frame => EnemySheetDefinitions.ProjectileFrameName(frame!.Value));
        EngineSheetPictures pictures = _sheets.CutSpawned(_tree, $"{Folder}/{name}", spawner, frames);
        JsonObject timeline = EngineTimelines.Of(lead, cycle, frame => pictures.Of(EnemySheetDefinitions.ProjectileFrameName(frame)), cue);
        timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        if (rests)
            Resting(timeline);
        _tree.WriteData(file, pictures.Lay(timeline, file));
        return file;
    }

    /// <summary>The name the installation keeps what the old enemy spawns under.</summary>
    internal string Spawner => SuperMetroid.Core.Enemies.EnemyDefinitionIdKeys.Installed.Of(_id);

    /// <summary>
    /// Makes a timeline that ends one that rests on its last picture instead. The old game's thrown things hold their
    /// last picture for as long as they fly; the Godot game's are over when their timeline is.
    /// </summary>
    /// <exception cref="InvalidDataException">The timeline already goes round.</exception>
    internal static JsonObject Resting(JsonObject timeline)
    {
        if (timeline.ContainsKey("loops_to"))
            throw new InvalidDataException("A timeline that goes round is asked to rest on its last picture.");
        timeline["loops_to"] = timeline["steps"]!.AsArray().Count - 1;
        return timeline;
    }

    /// <summary>
    /// Writes one of its shots to a file of its own, under its folder, and takes it out of its own list: a shot that
    /// something it launches fires in its turn, and names by its file.
    /// </summary>
    /// <exception cref="InvalidDataException">It has no shot of that name.</exception>
    internal string ShotFile(string name)
    {
        JsonNode shot = _shots[name] ?? throw new InvalidDataException($"The file {_file} has no shot named {name} to write to a file of its own.");
        _shots.Remove(name);
        string file = $"{Folder}/{name}.json";
        _tree.WriteData(file, shot.AsObject());
        return file;
    }

    /// <summary>The timeline one of its shots is shown by, to change it once the shot has been added.</summary>
    internal JsonObject ShotProjectile(string name) =>
        _shots[name]?["projectile"]?.AsObject() ?? throw new InvalidDataException($"The file {_file} has no shot named {name}.");
}
