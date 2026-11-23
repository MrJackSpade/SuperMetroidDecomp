using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// One enemy's data file for the Godot game, gathered a part at a time: what every enemy has, from the old game's
/// list of enemies, and then whatever its own kind adds.
/// </summary>
internal sealed partial class EngineEnemy
{
    private readonly EngineAssetTree _tree;
    private readonly string _file;
    private readonly JsonObject _timelines = [];
    private readonly JsonObject _colliders = [];

    /// <summary>
    /// The data file <paramref name="file"/> of the enemy the old game lists as <paramref name="id"/>, shown by the
    /// pictures <paramref name="pictures"/>.
    /// </summary>
    internal EngineEnemy(EngineAssetTree tree, string file, EnemyDefinitionId id, EngineSheetPictures pictures)
    {
        _tree = tree;
        _file = file;
        Pictures = pictures;
        Definition = RoomEnemyDefinitionCatalog.ById[id];
        Data["health"] = Definition.Health;
        Data["contact_damage"] = Definition.Damage;
        Data["hurt_flash_updates"] = Definition.HurtFlashFrames;
        Data["picture_offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
    }

    /// <summary>What the old game's list of enemies says of it.</summary>
    internal RoomEnemyDefinition Definition { get; }

    /// <summary>Its pictures.</summary>
    internal EngineSheetPictures Pictures { get; }

    /// <summary>The file's values so far.</summary>
    internal JsonObject Data { get; } = [];

    /// <summary>The picture of one of its frames; none for the frame that shows nothing.</summary>
    internal string Picture(EnemyFrameId frame) =>
        frame == EnemyFrameId.PointAtOrigin ? "" : Pictures.Of(EnemyFrameNameDefinitions.Names[frame]);

    /// <summary>Gives a value by name.</summary>
    internal EngineEnemy Value(string name, JsonNode? value)
    {
        Data[name] = value;
        return this;
    }

    /// <summary>
    /// Sizes the scene's colliders of these names by the half-width and half-height the old game gives the enemy,
    /// about its place.
    /// </summary>
    internal EngineEnemy BodyColliders(params ReadOnlySpan<string> names)
    {
        foreach (string name in names)
            Collider(name, -Definition.Radius.X, -Definition.Radius.Y, Definition.Radius.X * 2, Definition.Radius.Y * 2);
        return this;
    }

    /// <summary>Sizes one of the scene's colliders, from the enemy's place.</summary>
    internal EngineEnemy Collider(string name, int left, int top, int width, int height)
    {
        _colliders[name] = new JsonObject { ["left"] = left, ["top"] = top, ["width"] = width, ["height"] = height };
        return this;
    }

    /// <summary>Adds the timeline of one of the old game's animations, under the name the Godot game plays it by.</summary>
    internal EngineEnemy Timeline<TSegment, TEvent>(
        string name, AnimationSegments<TSegment, EnemyFrameId, TEvent> segments, Animation<TSegment> animation, Func<TEvent, string?> cue)
        where TSegment : struct
    {
        _timelines[name] = EngineTimelines.Of(segments, animation, Picture, cue);
        return this;
    }

    /// <summary>Adds the timeline of an animation whose events the Godot game hears none of.</summary>
    internal EngineEnemy Timeline<TSegment, TEvent>(
        string name, AnimationSegments<TSegment, EnemyFrameId, TEvent> segments, Animation<TSegment> animation)
        where TSegment : struct =>
        Timeline(name, segments, animation, static (TEvent _) => null);

    /// <summary>Adds a timeline already made.</summary>
    internal EngineEnemy Timeline(string name, JsonObject timeline)
    {
        _timelines[name] = timeline;
        return this;
    }

    /// <summary>Writes the file.</summary>
    internal void Write()
    {
        if (_colliders.Count > 0)
            Data["colliders"] = _colliders;
        if (_timelines.Count > 0)
            Data["timelines"] = _timelines;
        _tree.WriteData(_file, Data);
    }
}
