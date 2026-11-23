using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What an enemy launches: its shots, and the timelines that show them.</summary>
internal sealed partial class EngineEnemy
{
    private readonly JsonObject _shots = [];

    /// <summary>The folder its pictures and the timelines of what it launches are written under: its file's name without the ending.</summary>
    internal string Folder => _file[.._file.LastIndexOf('.')];

    /// <summary>
    /// Adds one of its shots, by the name the Godot enemy fires it by: the hit and the size the old game gives the
    /// projectile of <paramref name="kind"/>, and whether it lives on after hurting Samus and stops her shots.
    /// It is not taken away for leaving the view, since the old game's is not. <paramref name="more"/> adds whatever
    /// else the shot and its projectile are given.
    /// </summary>
    internal EngineEnemy Shot(string name, RoomEnemyProjectileKind kind, Action<EngineEnemy, JsonObject, JsonObject>? more = null)
    {
        EnemyProjectileDefinition old = EnemyProjectileDefinitionCatalog.Definitions[kind];
        var projectile = new JsonObject
        {
            // Something of no size still lies somewhere: one pixel.
            ["width"] = Math.Max(1, old.Radius.X * 2),
            ["height"] = Math.Max(1, old.Radius.Y * 2),
            ["meeting_target"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact) ? "passes_through_and_rehits" : "ends",
            ["met_by_shot"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots) ? "stops_shot" : "ignored",
            ["view_margin"] = -1,
        };
        if (old.Properties.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus))
            projectile["delivers_hit"] = false;
        var shot = new JsonObject { ["damage"] = old.Damage, ["projectile"] = projectile };
        more?.Invoke(this, shot, projectile);
        _shots[name] = shot;
        return this;
    }

    /// <summary>
    /// Writes the timeline of something it launches, under its own folder, with pictures cut from the sheets of what
    /// it spawns, and returns the timeline's data file.
    /// </summary>
    internal string SpawnedTimeline<TSegment, TEvent, TBeat, TShown>(
        string name, AnimationSegmentPlay<TSegment, EnemyProjectileFrameId, TEvent, TBeat, TShown> segments, Animation<TSegment> animation,
        Func<TEvent, string?> cue)
        where TSegment : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<EnemyProjectileFrameId, TEvent>
        where TShown : class
    {
        (List<IAnimationBeat<EnemyProjectileFrameId, TEvent>> lead, List<IAnimationBeat<EnemyProjectileFrameId, TEvent>>? cycle) =
            EngineTimelines.Played(segments, animation);
        return SpawnedTimeline(name, lead, cycle, cue);
    }

    /// <summary>Writes the timeline of something it launches from the beats it plays once and then, where it does, over and over.</summary>
    internal string SpawnedTimeline<TEvent>(
        string name, IReadOnlyList<IAnimationBeat<EnemyProjectileFrameId, TEvent>> lead, IReadOnlyList<IAnimationBeat<EnemyProjectileFrameId, TEvent>>? cycle,
        Func<TEvent, string?> cue)
        where TEvent : notnull
    {
        string file = $"{Folder}/{name}.json";
        IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue)
            .Select(frame => EnemySheetDefinitions.ProjectileFrameName(frame!.Value));
        EngineSheetPictures pictures = _sheets.CutSpawned(_tree, $"{Folder}/{name}", EnemyDefinitionIdKeys.Installed.Of(_id), frames);
        JsonObject timeline = EngineTimelines.Of(lead, cycle, frame => pictures.Of(EnemySheetDefinitions.ProjectileFrameName(frame)), cue);
        timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        _tree.WriteData(file, timeline);
        return file;
    }

    /// <summary>
    /// The offset of a projectile's pictures that leaves them where its timeline puts them: without one a projectile
    /// centres its pictures on itself.
    /// </summary>
    internal static JsonObject NoOffset() => new() { ["x"] = 0, ["y"] = 0 };

    /// <summary>The cue of something launched that only ever takes itself away, which a timeline does by ending.</summary>
    internal static string? EndsOnly(EnemyProjectileCommonCue cue) => cue is EnemyProjectileCommonCue.Signal { Kind: EnemyProjectileCommonCueKind.Delete }
        ? null
        : throw new InvalidDataException($"Something launched raises {cue}, which this does not know.");

    /// <summary>
    /// Writes the data file of a projectile that is a scene of its own in the Godot project, with the values its type
    /// reads at their defaults and then those given.
    /// </summary>
    internal EngineEnemy ProjectileData(string file, string type, params ReadOnlySpan<(string Name, JsonNode? Value)> values)
    {
        var data = new JsonObject();
        EngineDefaults.Fill(data, type);
        foreach ((string name, JsonNode? value) in values)
        {
            if (!data.ContainsKey(name))
                throw new InvalidDataException($"The data file {file} is given the value {name}, which its type does not read by that name.");
            data[name] = value;
        }

        _tree.WriteData(file, data);
        return this;
    }
}
