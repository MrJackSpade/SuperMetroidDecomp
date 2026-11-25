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
        // Where an effect shows in one set of graphics, each place with the sheet its pictures were cut from: the
        // folder of the set's blocks, and each picture behind a room that runs it, whether a picture of its own or
        // one put together from the set's blocks.
        var shownOn = new SortedDictionary<(string Set, ColourEffectId Effect), SortedDictionary<string, string>>();
        foreach ((RoomDefinition _, RoomStateSettings state) in EngineLayouts.States())
        {
            foreach (ColourEffectId effect in (state.Fx?.Effects ?? []).SelectMany(record => record.ColourEffects))
            {
                if (!ColourEffectRoomColourDefinitions.RoomColours.Contains(ColourEffectAnimations.Definition(effect).Colours))
                    continue;
                string set = GraphicsFolder(state);
                if (!shownOn.TryGetValue((set, effect), out SortedDictionary<string, string>? places))
                    shownOn[(set, effect)] = places = new SortedDictionary<string, string>(StringComparer.Ordinal);
                AddRoomPlaces(places, state);
            }
        }

        foreach (((string set, ColourEffectId effect), SortedDictionary<string, string> places) in shownOn)
            _tree.WriteData(ColourStepsPath(set, effect), ColourSteps(effect, places));
    }

    /// <summary>
    /// Adds the places one state of a room shows pictures of its own from, each with the sheet its pictures were cut
    /// from: the folder of its set's blocks, and the picture behind it, whether a picture of its own or one put
    /// together from the set's blocks.
    /// </summary>
    private static void AddRoomPlaces(IDictionary<string, string> places, RoomStateSettings state)
    {
        string set = GraphicsFolder(state), blocks = state.GraphicsSet.ToString();
        places[$"tiles/{set}"] = blocks;
        if (BackdropSheet(state) is { } backdrop)
            places[BackdropPath(state, state.Background!)] = backdrop;
        else if (state.Background is null)
            places[EngineLayouts.LevelBackgroundPath(state, set)] = blocks;
    }

    /// <summary>
    /// The steps of one colour effect as its animation plays them, on the sheets given: each frame a step, lasting as
    /// long as the frame shows, with the sound the animation asks for as the frame comes; a part of the animation
    /// played several times over is written out as often.
    /// </summary>
    private JsonObject ColourSteps(ColourEffectId effect, IReadOnlyDictionary<string, string> places)
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
                                ["colours"] = ColoursShown(definition.Colours, new ColourEffectFrame(effect, shown.Shown), places),
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

    /// <summary>The most colours a thing drawn in the engine shows changed at once (its <c>Recolouring.MostSwaps</c>).</summary>
    private const int MostSwapsShown = 64;

    /// <summary>The colours one frame of a colour effect shows changed, place by place.</summary>
    private JsonObject ColoursShown(ColourEffectColours colours, ColourEffectFrame frame, IReadOnlyDictionary<string, string> places) =>
        ShadeChange(ColourEffectShades.Of(colours, frame).Key, places);

    /// <summary>
    /// The colours a shade shows changed, place by place, as the engine reads a change of colours: for each place,
    /// every colour the shade changes as the place's pictures have it, with the colour the shade shows for it,
    /// written as six hexadecimal digits. A colour changed only in one region of the pictures is written with the
    /// region's number after a stroke, as the pictures' maps of regions number it. A place the shade changes nothing
    /// in is left out.
    /// </summary>
    /// <param name="shade">The shade, or several with a plus sign between them, a later one laid over an earlier.</param>
    /// <param name="places">Each place with the sheet its pictures were cut from.</param>
    /// <exception cref="InvalidDataException">The shade changes more colours in one place than a thing can show changed at once.</exception>
    private JsonObject ShadeChange(string shade, IReadOnlyDictionary<string, string> places)
    {
        static string Written(int colour) => colour.ToString("x6", System.Globalization.CultureInfo.InvariantCulture);
        var written = new JsonObject();
        foreach ((string place, string sheet) in places)
        {
            IReadOnlyList<ShadeSwap> swaps = _sheets.ShadeSwaps(RoomBlockFamily, sheet, shade);
            if (swaps.Count > MostSwapsShown)
                throw new InvalidDataException($"The shade {shade} changes {swaps.Count} colours of {place}; a thing shows at most {MostSwapsShown} changed at once.");
            if (swaps.Count == 0)
                continue;
            var changed = new JsonObject();
            foreach (ShadeSwap swap in swaps)
                changed[swap.Region == ShadeSwap.Everywhere ? Written(swap.From) : $"{Written(swap.From)}/{swap.Region}"] = Written(swap.To);
            written[place] = changed;
        }

        return written;
    }
}
