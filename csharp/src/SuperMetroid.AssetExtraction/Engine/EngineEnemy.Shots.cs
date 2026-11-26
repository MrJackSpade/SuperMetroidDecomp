using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What an enemy launches: its shots, and the timelines that show them.</summary>
internal sealed partial class EngineEnemy
{
    private readonly JsonObject _shots = [];

    /// <summary>The folder its pictures and the timelines of what it launches are written under: its file's name without the ending.</summary>
    internal string Folder => _file[.._file.LastIndexOf('.')];

    /// <summary>
    /// Where something an enemy launches stands against Samus, as the Godot game names it: the old game draws each
    /// kind behind her and behind every enemy, or in front of her and of every enemy.
    /// </summary>
    internal static string StandingOf(EnemyProjectileDefinition old) => old.Depth switch
    {
        EnemyProjectileDepth.BehindSamus => "hindmost",
        EnemyProjectileDepth.InFrontOfSamus => "foremost",
        _ => throw new InvalidDataException($"The old game draws something it launches at {old.Depth}, which has no name here."),
    };

    /// <summary>
    /// Adds one of its shots, by the name the Godot enemy fires it by: the hit and the size the old game gives the
    /// projectile of <paramref name="kind"/>, whether it lives on after hurting Samus, and whether her shots meet it.
    /// One her shots meet is destroyed by them, as the old game's is unless its own code makes it a dud.
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
            ["met_by_shot"] = old.Properties.HasFlag(EnemyProjectilePropertyFlags.BlocksShots) ? "destroyed" : "ignored",
            ["view_margin"] = -1,
            ["standing"] = StandingOf(old),
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
        Func<TEvent, string?> cue, string? shade = null)
        where TSegment : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<EnemyProjectileFrameId, TEvent>
        where TShown : class
    {
        (List<IAnimationBeat<EnemyProjectileFrameId, TEvent>> lead, List<IAnimationBeat<EnemyProjectileFrameId, TEvent>>? cycle) =
            EngineTimelines.Played(segments, animation);
        return SpawnedTimeline(name, lead, cycle, cue, shade);
    }

    /// <summary>Writes the timeline of something it launches from the beats it plays once and then, where it does, over and over.</summary>
    internal string SpawnedTimeline<TEvent>(
        string name, IReadOnlyList<IAnimationBeat<EnemyProjectileFrameId, TEvent>> lead, IReadOnlyList<IAnimationBeat<EnemyProjectileFrameId, TEvent>>? cycle,
        Func<TEvent, string?> cue, string? shade = null)
        where TEvent : notnull =>
        SpawnedTimelineAt($"{Folder}/{name}.json", lead, cycle, EnemySheetDefinitions.ProjectileFrameName, cue, shade);

    /// <summary>
    /// Writes a timeline of pictures from the sheets of what it spawns to a data file of any name, cutting the
    /// pictures into a folder named as the file is, and returns the file. With a <paramref name="shade"/> the
    /// pictures are cut as that shade of their sheet shows them: what the old game draws in a look of its own, as the
    /// projectiles' shared colours, and not in its sheet's own.
    /// </summary>
    internal string SpawnedTimelineAt<TFrame, TEvent>(
        string file, IReadOnlyList<IAnimationBeat<TFrame, TEvent>> lead, IReadOnlyList<IAnimationBeat<TFrame, TEvent>>? cycle, Func<TFrame, string> frameName,
        Func<TEvent, string?> cue, string? shade = null)
        where TFrame : struct
        where TEvent : notnull
    {
        IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue)
            .Select(frame => frameName(frame!.Value));
        string folder = file[..file.LastIndexOf('.')], spawner = EnemyDefinitionIdKeys.Installed.Of(_id);
        EngineSheetPictures pictures = shade is null
            ? _sheets.CutSpawned(_tree, folder, spawner, frames)
            : _sheets.CutSpawnedShade(_tree, folder, spawner, frames, shade);
        JsonObject timeline = EngineTimelines.Of(lead, cycle, frame => pictures.Of(frameName(frame)), cue);
        timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        _tree.WriteData(file, timeline);
        return file;
    }

    /// <summary>
    /// Writes the timeline of one of the old game's lesser effects as this enemy's rooms draw it, under its own
    /// folder, and returns the timeline's data file.
    /// </summary>
    internal string SpawnedEffect(string name, RoomEffectActorKind kind)
    {
        (List<IAnimationBeat<RoomEffectActorPicture, NoAnimationCue>> lead, List<IAnimationBeat<RoomEffectActorPicture, NoAnimationCue>>? cycle) =
            EngineTimelines.Played(RoomEffectActorAnimations.Segments, RoomEffectActorAnimations.StartingWith(kind));
        return SpawnedTimelineAt($"{Folder}/{name}.json", lead, cycle, EffectFrame, static (NoAnimationCue _) => null);
    }

    /// <summary>The name of the picture of a lesser effect's frame on the installed sheets.</summary>
    internal static string EffectFrame(RoomEffectActorPicture picture) => picture.EnemyFrame is { } frame
        ? EnemySheetDefinitions.EffectActorFrameName(frame)
        : throw new InvalidDataException("A lesser effect shows a picture that is not on the enemies' sheets.");

    /// <summary>
    /// Adds a timeline of its own that is one of the old game's lesser effects: something drawn with the enemy as a
    /// part of it. Its pictures are on the sheets of what the enemy spawns and are cut to a size of their own, so the
    /// timeline says where they lie beyond where the enemy's own pictures do.
    /// </summary>
    internal EngineEnemy SpawnedLook(string name, RoomEffectActorKind kind)
    {
        (List<IAnimationBeat<RoomEffectActorPicture, NoAnimationCue>> lead, List<IAnimationBeat<RoomEffectActorPicture, NoAnimationCue>>? cycle) =
            EngineTimelines.Played(RoomEffectActorAnimations.Segments, RoomEffectActorAnimations.StartingWith(kind));
        return SpawnedLook(name, lead, cycle, EffectFrame, static (NoAnimationCue _) => null);
    }

    /// <summary>
    /// Adds a timeline of its own that the old game drew as something the enemy launched and kept by it: a part of
    /// it, shown by pictures on the sheets of what it spawns.
    /// </summary>
    internal EngineEnemy SpawnedLook<TSegment, TEvent, TBeat, TShown>(
        string name, AnimationSegmentPlay<TSegment, EnemyProjectileFrameId, TEvent, TBeat, TShown> segments, Animation<TSegment> animation,
        Func<TEvent, string?> cue)
        where TSegment : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<EnemyProjectileFrameId, TEvent>
        where TShown : class
    {
        (List<IAnimationBeat<EnemyProjectileFrameId, TEvent>> lead, List<IAnimationBeat<EnemyProjectileFrameId, TEvent>>? cycle) =
            EngineTimelines.Played(segments, animation);
        return SpawnedLook(name, lead, cycle, EnemySheetDefinitions.ProjectileFrameName, cue);
    }

    /// <summary>
    /// Adds a timeline of its own whose pictures are on the sheets of what it spawns. They are cut to a size of their
    /// own, so the timeline says where they lie beyond where the enemy's own pictures do.
    /// </summary>
    internal EngineEnemy SpawnedLook<TFrame, TEvent>(
        string name, IReadOnlyList<IAnimationBeat<TFrame, TEvent>> lead, IReadOnlyList<IAnimationBeat<TFrame, TEvent>>? cycle, Func<TFrame, string> frameName,
        Func<TEvent, string?> cue)
        where TFrame : struct
        where TEvent : notnull
    {
        IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue).Select(frame => frameName(frame!.Value));
        EngineSheetPictures pictures = _sheets.CutSpawned(_tree, $"{Folder}/{name}", EnemyDefinitionIdKeys.Installed.Of(_id), frames);
        JsonObject timeline = EngineTimelines.Of(lead, cycle, frame => pictures.Of(frameName(frame)), cue);
        timeline["offset"] = new JsonObject { ["x"] = pictures.OffsetX - Pictures.OffsetX, ["y"] = pictures.OffsetY - Pictures.OffsetY };
        return Timeline(name, timeline);
    }

    /// <summary>
    /// The offset of a projectile's pictures that leaves them where its timeline puts them: without one a projectile
    /// centres its pictures on itself.
    /// </summary>
    internal static JsonObject NoOffset() => new() { ["x"] = 0, ["y"] = 0 };

    /// <summary>The height of the view, in pixels.</summary>
    private const int ViewHeight = 224;

    /// <summary>
    /// Makes a projectile one the old game takes away once it is out of the enemies' view of the room, which is the
    /// screen and as far again below it as that view is taller than the screen.
    /// </summary>
    internal static void GoneOutOfView(JsonObject projectile)
    {
        projectile["view_margin"] = 0;
        projectile["view_margin_below"] = EnemyView.Extent - ViewHeight;
    }

    /// <summary>Makes a projectile one the old game takes away only once it has fallen its limit below the top of the view.</summary>
    internal static void GoneFarBelowView(JsonObject projectile)
    {
        projectile["view_margin"] = -1;
        projectile["view_margin_below"] = EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth - ViewHeight;
    }

    /// <summary>
    /// Writes the timeline of something it launches that is one picture and never changes, under its own folder, and
    /// returns the timeline's data file.
    /// </summary>
    internal string SpawnedPicture(string name, EnemyProjectileFrameId frame) =>
        SpawnedTimeline<NoAnimationCue>(name, [new AnimationBeat<EnemyProjectileFrameId, NoAnimationCue>.HeldFrame(frame)], null, static _ => null);

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
        if (data.ContainsKey("speed_curve"))
            data["speed_curve"] = EngineAssetWriter.SpeedCurveFile;
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
