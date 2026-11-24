using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Input;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What the cinematics, the menus and the rest of what is shown outside play share: sprites' timelines, places, colours and recordings.</summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>A place on the screen or in a room, as a data file writes one.</summary>
    private static JsonObject At(int x, int y) => new() { ["x"] = x, ["y"] = y };

    /// <summary>A colour as a data file writes one.</summary>
    private static string Colour(RgbColor colour) => $"#{colour.Red:x2}{colour.Green:x2}{colour.Blue:x2}";

    /// <summary>The name the Godot game gives something the old game names in capitalised words.</summary>
    private static string Named<TName>(TName name)
        where TName : struct, Enum => EngineSheets.FileName(name.ToString());

    /// <summary>
    /// The timeline of one of the old game's cinematic sprites: its pictures are cut from its sheet, in the shade it
    /// is drawn in, into <paramref name="folder"/>, and the timeline says where they lie from the sprite's place.
    /// </summary>
    /// <param name="segments">The beats of every segment of the sprite's family of animations.</param>
    /// <param name="animation">The animation.</param>
    /// <param name="sheet">The sheet its pictures are on.</param>
    /// <param name="family">The family each pose's picture is named in.</param>
    /// <param name="folder">The folder its pictures are written under.</param>
    /// <param name="shade">The shade it is drawn in, or none for the sheet's own colours.</param>
    /// <param name="cue">The cue an event becomes, or null for one the Godot game does not hear.</param>
    private JsonObject SpriteTimeline<TSegment, TEvent, TBeat, TShown>(
        AnimationSegmentPlay<TSegment, CinematicSpritePose, TEvent, TBeat, TShown> segments, Animation<TSegment> animation, SpriteSheetId sheet,
        Func<CinematicSpritePose, CinematicSpriteFamily> family, string folder, PictureShade? shade, Func<TEvent, string?> cue)
        where TSegment : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<CinematicSpritePose, TEvent>
        where TShown : class
    {
        (List<IAnimationBeat<CinematicSpritePose, TEvent>> lead, List<IAnimationBeat<CinematicSpritePose, TEvent>>? cycle) = EngineTimelines.Played(segments, animation);
        string FrameOf(CinematicSpritePose pose) => CinematicSpriteSheet.FrameName(family(pose), pose);
        IEnumerable<string> frames = lead.Concat(cycle ?? []).Select(beat => beat.ShownFrame).Where(frame => frame.HasValue).Select(frame => FrameOf(frame!.Value));
        EngineSheetPictures pictures = shade is { } drawn
            ? _sheets.CutSprites(_tree, sheet, folder, frames, drawn.Key)
            : _sheets.CutSprites(_tree, sheet, folder, frames);
        JsonObject timeline = EngineTimelines.Of(lead, cycle, pose => pictures.Of(FrameOf(pose)), cue);
        timeline["offset"] = At(pictures.OffsetX, pictures.OffsetY);
        return timeline;
    }

    /// <summary>As <see cref="SpriteTimeline{TSegment, TEvent, TBeat, TShown}(AnimationSegmentPlay{TSegment, CinematicSpritePose, TEvent, TBeat, TShown}, Animation{TSegment}, SpriteSheetId, Func{CinematicSpritePose, CinematicSpriteFamily}, string, PictureShade?, Func{TEvent, string?})"/>, for a sprite all of whose poses are of one family.</summary>
    private JsonObject SpriteTimeline<TSegment, TEvent, TBeat, TShown>(
        AnimationSegmentPlay<TSegment, CinematicSpritePose, TEvent, TBeat, TShown> segments, Animation<TSegment> animation, SpriteSheetId sheet,
        CinematicSpriteFamily family, string folder, PictureShade? shade, Func<TEvent, string?> cue)
        where TSegment : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<CinematicSpritePose, TEvent>
        where TShown : class =>
        SpriteTimeline(segments, animation, sheet, _ => family, folder, shade, cue);

    /// <summary>The cue of an animation that raises none.</summary>
    private static string? NoCue(NoAnimationCue cue) => null;

    /// <summary>The Godot game's names of the actions one of the old game's inputs holds, in the old game's order of them.</summary>
    /// <exception cref="InvalidDataException">The input holds something the Godot game has no action for.</exception>
    private static JsonArray Actions(GameplayAction held)
    {
        (GameplayAction Old, string Name)[] names =
        [
            (GameplayAction.Up, "up"), (GameplayAction.Down, "down"), (GameplayAction.Left, "left"), (GameplayAction.Right, "right"),
            (GameplayAction.Start, "pause"), (GameplayAction.Shoot, "attack"), (GameplayAction.Jump, "jump"), (GameplayAction.Dash, "run"),
            (GameplayAction.ItemSelect, "item_select"), (GameplayAction.ItemCancel, "item_cancel"), (GameplayAction.AimUp, "angle_up"),
            (GameplayAction.AimDown, "angle_down"),
        ];
        var actions = new JsonArray();
        GameplayAction left = held;
        foreach ((GameplayAction old, string name) in names)
        {
            if (!held.HasFlag(old))
                continue;
            actions.Add(name);
            left &= ~old;
        }

        return left == GameplayAction.None ? actions : throw new InvalidDataException($"A recording holds {left}, which the Godot game has no action for.");
    }

    /// <summary>
    /// One of the old game's recordings as the Godot game's data writes one, played in a room from an entry point.
    /// The Godot game takes an action as pressed when it is first held; where the old recording says otherwise, the
    /// stretch says what it presses.
    /// </summary>
    private static JsonObject Recording(DemoRecording recording, string room, string entryPoint)
    {
        var inputs = new JsonArray();
        GameplayAction before = GameplayAction.None;
        foreach (DemoInput input in recording.Inputs)
        {
            // The old game goes on reporting an input's presses for as long as the input lasts.
            bool pressesThroughout = input.NewlyPressed != GameplayAction.None && input.Duration > 1;
            for (int update = 0; update < (pressesThroughout ? input.Duration : 1); update++)
            {
                var stretch = new JsonObject { ["held"] = Actions(input.Held), ["updates"] = pressesThroughout ? 1 : input.Duration };
                if (input.NewlyPressed != (input.Held & ~before))
                    stretch["pressed"] = Actions(input.NewlyPressed);
                inputs.Add(stretch);
                before = input.Held;
            }
        }

        var data = new JsonObject { ["room"] = room, ["entry_point"] = entryPoint, ["inputs"] = inputs };
        if (recording.End is DemoRecordingEnd.HoldLastInput)
            data["ends"] = "holding";
        return data;
    }
}
