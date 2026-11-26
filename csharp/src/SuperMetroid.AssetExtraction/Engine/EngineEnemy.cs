using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EffectActors;
using SoundRequestPriority = SuperMetroid.Core.Compatibility.SoundRequestPriority;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// One enemy's data file for the Godot game, gathered a part at a time: what every enemy has, from the old game's
/// list of enemies, and then whatever its own kind adds.
/// </summary>
internal sealed partial class EngineEnemy
{
    private readonly EngineAssetTree _tree;
    private readonly EngineSheets _sheets;
    private readonly EnemyDefinitionId _id;
    private readonly string _file;
    private readonly JsonObject _timelines = [];
    private readonly JsonObject _colliders = [];

    /// <summary>
    /// The data file <paramref name="file"/> of the enemy the old game lists as <paramref name="id"/>, which the Godot
    /// type <paramref name="type"/> reads, shown by the pictures <paramref name="pictures"/>.
    /// </summary>
    internal EngineEnemy(EngineAssetTree tree, EngineSheets sheets, string file, string type, EnemyDefinitionId id, EngineSheetPictures pictures)
    {
        _tree = tree;
        _sheets = sheets;
        _id = id;
        _file = file;
        Pictures = pictures;
        // An enemy a fight brings into a room is listed apart from those rooms place.
        Definition = RoomEnemyDefinitionCatalog.ById.TryGetValue(id, out RoomEnemyDefinition placed)
            ? placed
            : RoomEnemyAuxiliaryDefinitionCatalog.Definitions.TryGetValue(id, out RoomEnemyDefinition brought)
                ? brought
                : throw new InvalidDataException($"The old game lists no enemy {id}.");
        EngineDefaults.Fill(Data, type);
        // Every enemy that moves by a curve of speeds moves by the one curve the old game has.
        if (Data.ContainsKey("speed_curve"))
            Data["speed_curve"] = EngineAssetWriter.SpeedCurveFile;
        Data["health"] = Definition.Health;
        Data["contact_damage"] = Definition.Damage;
        // A weapon's hit makes the old enemy flash that much longer than its own flash, and nearly every hit is a weapon's.
        Data["hurt_flash_updates"] = Definition.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension;
        Data["picture_offset"] = new JsonObject { ["x"] = pictures.OffsetX, ["y"] = pictures.OffsetY };
        // Where it stands against Samus as it enters; one that stands behind her, as nearly all do, says nothing.
        if (Definition.Stacking != EnemyStacking.BehindSamus)
        {
            Data["standing"] = Definition.Stacking switch
            {
                EnemyStacking.Frontmost => "frontmost",
                EnemyStacking.InFrontOfSamus => "in_front",
                EnemyStacking.JustBehindSamus => "just_behind",
                EnemyStacking.FarBehindSamus => "far_behind",
                EnemyStacking.Rearmost => "rearmost",
                _ => throw new InvalidDataException($"The old game's enemy {id} stands at {Definition.Stacking}, which has no name here."),
            };
        }

        // One whose every picture the cartridge draws in front of all the room's tiles is drawn there.
        if (pictures.Depth == SuperMetroid.Core.Assets.ActorDepth.InFront)
            Data["in_front_of_tiles"] = true;
        Data["reactions"] = EngineReactions.Of(Definition);
        if (EngineAssetWriter.Drops(Definition.Drops) is { } drops)
            Data["drops"] = drops;
        Data["death"] = EngineAssetWriter.Death(Definition.DeathExplosion);
        // A shot's hurt cry and the sound of being frozen each join only a queue holding at most two.
        if (Definition.HurtSoundEffect is { } cry && EngineNames.Sound(cry) is not null)
            Data["hurt_sound"] = EngineNames.Sound(cry, SoundRequestPriority.Low);
        Data["status_sounds"] = new JsonObject { ["frozen"] = EngineNames.Sound(RoomSound.EnemyFrozen, SoundRequestPriority.Low) };
        // A shot or a bomb that does it no harm leaves the dud's flash where it is, with the dud's sound, and is spent.
        Data["dud"] = new JsonObject
        {
            ["effect"] = EngineAssetWriter.EffectFile(RoomEffectActorKind.DudShot),
            ["sound"] = EngineNames.Sound(SamusSound.DudShot, SoundRequestPriority.Low),
        };
    }

    /// <summary>What the old game's list of enemies says of it.</summary>
    internal RoomEnemyDefinition Definition { get; }

    /// <summary>Its pictures.</summary>
    internal EngineSheetPictures Pictures { get; }

    /// <summary>The file's values so far.</summary>
    internal JsonObject Data { get; } = [];

    /// <summary>
    /// The picture of one of its frames, found by the name the installed sheets keep the frame under; none for the
    /// frame that shows nothing.
    /// </summary>
    internal string Picture(EnemyFrameId frame) =>
        frame == EnemyFrameId.PointAtOrigin ? "" : Pictures.Of(EnemySheetDefinitions.FrameName(frame));

    /// <summary>Gives a value by name.</summary>
    internal EngineEnemy Value(string name, JsonNode? value)
    {
        if (!Data.ContainsKey(name))
            throw new InvalidDataException($"The data file {_file} is given the value {name}, which its type does not read by that name.");
        Data[name] = value;
        return this;
    }

    /// <summary>Gives a group of its data that is not one of its type's named values.</summary>
    internal EngineEnemy Group(string name, JsonNode value)
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
        where TEvent : notnull
    {
        _timelines[name] = EngineTimelines.Of(segments, animation, Picture, cue);
        return this;
    }

    /// <summary>Adds the timeline of an animation whose events the Godot game hears none of.</summary>
    internal EngineEnemy Timeline<TSegment, TEvent>(
        string name, AnimationSegments<TSegment, EnemyFrameId, TEvent> segments, Animation<TSegment> animation)
        where TSegment : struct
        where TEvent : notnull =>
        Timeline(name, segments, animation, static (TEvent _) => null);

    /// <summary>
    /// Adds a timeline for each of the old game's animations that begins at a named segment and whose events the Godot
    /// game hears none of: the name the Godot game plays it by, and the segment.
    /// </summary>
    internal EngineEnemy Looks<TSegment, TEvent>(
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments, Func<TSegment, Animation<TSegment>> startingWith,
        params ReadOnlySpan<(string Name, TSegment Segment)> named)
        where TSegment : struct
        where TEvent : notnull
    {
        foreach ((string name, TSegment segment) in named)
            Timeline(name, segments, startingWith(segment));
        return this;
    }

    /// <summary>
    /// Adds a timeline for each of the old game's animations that begins at a named segment, with what each of its
    /// events becomes.
    /// </summary>
    internal EngineEnemy Looks<TSegment, TEvent>(
        AnimationSegments<TSegment, EnemyFrameId, TEvent> segments, Func<TSegment, Animation<TSegment>> startingWith, Func<TEvent, string?> cue,
        params ReadOnlySpan<(string Name, TSegment Segment)> named)
        where TSegment : struct
        where TEvent : notnull
    {
        foreach ((string name, TSegment segment) in named)
            Timeline(name, segments, startingWith(segment), cue);
        return this;
    }

    /// <summary>Adds a timeline already made.</summary>
    internal EngineEnemy Timeline(string name, JsonObject timeline)
    {
        _timelines[name] = timeline;
        return this;
    }

    private string? _pictureSets;

    /// <summary>
    /// Makes every picture its timelines name one of several sets of its pictures, which the Godot enemy picks between
    /// by what it puts for <paramref name="placeholder"/>: the pictures' folder is named with the placeholder after
    /// it, and each set is the folder of that name with something put for it.
    /// </summary>
    internal EngineEnemy PictureSets(string placeholder)
    {
        _pictureSets = placeholder;
        return this;
    }

    /// <summary>Cuts another set of its pictures, in one of its sheet's shades, into the folder named as its own with <paramref name="after"/> added.</summary>
    internal EngineEnemy ShadedSet(string after, string shade)
    {
        const string family = Installation.GameInstallationLayout.EnemySheetDirectoryName;
        string sheet = _sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(_id));
        _sheets.CutShade(_tree, family, Folder + after, sheet, shade);
        FlashShaded(Folder + after, family, sheet, shade);
        return this;
    }

    /// <summary>
    /// Adds, to the colours it is shown in while its hurt flash shows ("hurt_colours"), those for a set of its
    /// pictures cut in one of a sheet's shades and kept in <paramref name="place"/>: each colour as the shade shows
    /// it, as the flash shows the colour it came from. The flash's own colours are named for the pictures as the
    /// sheet has them, which a set cut in a shade does not show. One with no flash colours, or whose sheet has no
    /// flash shade, is left as it is.
    /// </summary>
    /// <exception cref="InvalidDataException">One colour of the set would flash as two.</exception>
    private void FlashShaded(string place, string family, string sheet, string shade)
    {
        string flash = SuperMetroid.Core.Enemies.Drawing.EnemyShades.HurtFlash.Key;
        if (Data["hurt_colours"] is not JsonObject colours || !_sheets.HasShade(family, sheet, flash))
            return;
        static string Written(int colour) => colour.ToString("x6", System.Globalization.CultureInfo.InvariantCulture);
        var shaded = new Dictionary<(int Colour, int Region), int>();
        foreach (ShadeSwap swap in _sheets.ShadeSwaps(family, sheet, shade))
            shaded[(swap.From, swap.Region)] = swap.To;
        if (colours[place] is not JsonObject into)
            colours[place] = into = [];
        foreach (ShadeSwap swap in _sheets.ShadeSwaps(family, sheet, flash))
        {
            int from = shaded.TryGetValue((swap.From, swap.Region), out int inRegion) ? inRegion
                : shaded.TryGetValue((swap.From, ShadeSwap.Everywhere), out int everywhere) ? everywhere : swap.From;
            if (from == swap.To)
                continue;
            string key = swap.Region == ShadeSwap.Everywhere ? Written(from) : $"{Written(from)}/{swap.Region}";
            if (into[key] is JsonNode had && had.GetValue<string>() != Written(swap.To))
                throw new InvalidDataException($"The colour {key} of the pictures in {place} would flash as {had.GetValue<string>()} and as {Written(swap.To)}.");
            into[key] = Written(swap.To);
        }
    }

    /// <summary>Writes the file.</summary>
    internal void Write()
    {
        if (_pictureSets is { } placeholder)
        {
            foreach ((string _, JsonNode? timeline) in _timelines)
            {
                foreach (JsonNode? step in timeline!["steps"]!.AsArray())
                {
                    string picture = step!["picture"]!.GetValue<string>();
                    if (picture.StartsWith(Folder + "/", StringComparison.Ordinal))
                        step["picture"] = Folder + placeholder + picture[Folder.Length..];
                }
            }
        }

        if (_colliders.Count > 0)
            Data["colliders"] = _colliders;
        if (_timelines.Count > 0)
            Data["timelines"] = _timelines;
        if (_shots.Count > 0)
            Data["shots"] = _shots;
        _tree.WriteData(_file, Data);
    }
}
