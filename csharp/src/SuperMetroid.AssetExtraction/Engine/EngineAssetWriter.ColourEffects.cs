using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The colour effects the rooms run on their own pictures: rock that glows, lights that flicker, lightning. Each is
/// written as the steps of an engine colour effect, once for every set of graphics a room that runs it is drawn with,
/// since a step names the colours it changes as that set's pictures have them.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The path of one colour effect's steps for one set of graphics, as the room scenes name it with the set left out.</summary>
    private static string ColourStepsPath(string graphicsFolder, ColourEffectId effect) =>
        $"colours/{graphicsFolder}/{EngineSheets.FileName(effect.ToString())}.json";

    /// <summary>
    /// Writes the steps of every colour effect a room's effect starts on the room's own pictures, for each set of
    /// graphics that shows it. A step's colours are those the effect's frame shows changed on the set's blocks and on
    /// the pictures behind the rooms that run it.
    /// </summary>
    /// <exception cref="InvalidDataException">An effect cannot be shown by swapping colours on one of its sheets, or two of its sheets disagree about a colour.</exception>
    private void WriteRoomColourEffects()
    {
        // The sheets an effect shows on in one set of graphics: the set's blocks, and each picture behind a room that runs it.
        var shownOn = new SortedDictionary<(string Set, ColourEffectId Effect), SortedSet<string>>();
        foreach ((RoomDefinition _, RoomStateSettings state) in EngineLayouts.States())
        {
            foreach (ColourEffectId effect in (state.Fx?.Effects ?? []).SelectMany(record => record.ColourEffects))
            {
                if (!ColourEffectRoomColourDefinitions.RoomColours.Contains(ColourEffectAnimations.Definition(effect).Colours))
                    continue;
                if (!shownOn.TryGetValue((GraphicsFolder(state), effect), out SortedSet<string>? sheets))
                    shownOn[(GraphicsFolder(state), effect)] = sheets = new SortedSet<string>(StringComparer.Ordinal) { state.GraphicsSet.ToString() };
                if (BackdropSheet(state) is { } backdrop)
                    sheets.Add(backdrop);
            }
        }

        foreach (((string set, ColourEffectId effect), SortedSet<string> sheets) in shownOn)
            _tree.WriteData(ColourStepsPath(set, effect), ColourSteps(effect, sheets));
    }

    /// <summary>
    /// The steps of one colour effect as its animation plays them, on the sheets given: each frame a step, lasting as
    /// long as the frame shows, with the sound the animation asks for as the frame comes; a part of the animation
    /// played several times over is written out as often.
    /// </summary>
    private JsonObject ColourSteps(ColourEffectId effect, IEnumerable<string> sheets)
    {
        ColourEffectDefinition definition = ColourEffectAnimations.Definition(effect);
        Animation<ColourEffectSegment> animation = definition.Frames.Played;
        var steps = new JsonArray();
        var firstStepOfPhase = new List<int>();
        string? sound = null;
        foreach (AnimationPhase<ColourEffectSegment> phase in animation.Phases)
        {
            firstStepOfPhase.Add(steps.Count);
            IReadOnlyList<AnimationBeat<int, ColourEffectCue>> beats = ColourEffectAnimations.Segments.Beats(phase.Segment);
            for (int play = 0; play < phase.Plays; play++)
            {
                for (int beat = phase.FirstBeat; beat < phase.FirstBeat + phase.BeatCount; beat++)
                {
                    switch (beats[beat])
                    {
                        case AnimationBeat<int, ColourEffectCue>.Frame shown:
                            var step = new JsonObject
                            {
                                ["updates"] = shown.Duration,
                                ["colours"] = ColoursShown(definition.Colours, new ColourEffectFrame(effect, shown.Shown), sheets),
                            };
                            if (sound is not null)
                                step["sound"] = sound;
                            sound = null;
                            steps.Add(step);
                            break;
                        case AnimationBeat<int, ColourEffectCue>.Event { Raised: ColourEffectCue.QueueSound queued }:
                            sound = EngineNames.Sound(queued.Sound, ColourEffectSoundPriorities.CueSound);
                            break;
                        case AnimationBeat<int, ColourEffectCue>.Event { Raised: ColourEffectCue.PublishHeatPhase }:
                            // The heat's phase is told to Samus's own shimmer, which is no part of the room's colours.
                            break;
                        default:
                            throw new InvalidDataException($"The colour effect {effect} plays a beat this does not know: {beats[beat]}.");
                    }
                }
            }
        }

        if (sound is not null)
            throw new InvalidDataException($"The colour effect {effect} asks for a sound after its last frame.");
        if (steps.Count == 0)
            throw new InvalidDataException($"The colour effect {effect} shows no frame.");
        var data = new JsonObject { ["steps"] = steps };
        if (animation.End is AnimationEnd.Loop loop)
            data["loops_to"] = firstStepOfPhase[loop.Phase];
        return data;
    }

    /// <summary>
    /// The colours one frame of a colour effect shows changed on the sheets given, each as the sheets' pictures have it
    /// and as the frame shows it, written as six hexadecimal digits.
    /// </summary>
    /// <exception cref="InvalidDataException">Two sheets show one colour as two under the frame.</exception>
    private JsonObject ColoursShown(ColourEffectColours colours, ColourEffectFrame frame, IEnumerable<string> sheets)
    {
        string shade = ColourEffectShades.Of(colours, frame).Key;
        var shown = new SortedDictionary<int, int>();
        foreach (string sheet in sheets)
        {
            foreach ((int from, int to) in _sheets.ShadeSwaps(RoomBlockFamily, sheet, shade))
            {
                if (shown.TryGetValue(from, out int other) && other != to)
                    throw new InvalidDataException($"The shade {shade} shows the colour #{from:x6} as #{other:x6} on one sheet and #{to:x6} on {sheet}.");
                shown[from] = to;
            }
        }

        var written = new JsonObject();
        foreach ((int from, int to) in shown)
            written[from.ToString("x6", System.Globalization.CultureInfo.InvariantCulture)] = to.ToString("x6", System.Globalization.CultureInfo.InvariantCulture);
        return written;
    }
}
