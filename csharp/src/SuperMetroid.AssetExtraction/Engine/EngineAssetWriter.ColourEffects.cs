using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
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
    /// the pictures behind the rooms that run it, and on what is placed in a room and drawn with the set's blocks
    /// (<see cref="EngineRoomObjects.FoldersShownRoomColours"/>).
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
    /// from: the folder of its set's blocks, the folders of what is placed in a room and drawn with those blocks (a
    /// door's cap, an item, its shell, a station: <see cref="EngineRoomObjects.FoldersShownRoomColours"/>), which are
    /// of the blocks' sheet and change as the blocks do, and the picture behind it, whether a picture of its own or
    /// one put together from the set's blocks.
    /// </summary>
    private static void AddRoomPlaces(IDictionary<string, string> places, RoomStateSettings state)
    {
        string set = GraphicsFolder(state), blocks = state.GraphicsSet.ToString();
        places[$"tiles/{set}"] = blocks;
        foreach (string folder in EngineRoomObjects.FoldersShownRoomColours)
            places[$"{folder}/{set}"] = blocks;
        if (BackdropSheet(state) is { } backdrop)
            places[BackdropPath(state, state.Background!)] = backdrop;
        else if (state.Background is null)
            places[EngineLayouts.LevelBackgroundPath(state, set)] = blocks;
    }

    /// <summary>The places the states of the rooms that place <paramref name="enemy"/> show pictures of their own from, each with its sheet.</summary>
    /// <exception cref="InvalidDataException">No room places the enemy.</exception>
    private static SortedDictionary<string, string> RoomPlacesOf(EnemyDefinitionId enemy)
    {
        var places = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach ((RoomDefinition _, RoomStateSettings state) in EngineLayouts.States())
        {
            if (state.EnemyPopulation.Placements.Span.Any(placed => placed.DefinitionId == enemy))
                AddRoomPlaces(places, state);
        }

        return places.Count > 0 ? places : throw new InvalidDataException($"No room places {enemy}.");
    }

    /// <summary>
    /// The steps of one colour effect as its animation plays them, on the sheets given: each frame a step, lasting as
    /// long as the frame shows, with the sound the animation asks for as the frame comes; a part of the animation
    /// played several times over is written out as often.
    /// </summary>
    private JsonObject ColourSteps(ColourEffectId effect, IReadOnlyDictionary<string, string> places) =>
        ColourSteps(effect, shade => ShadeChange(shade, places));

    /// <summary>
    /// The steps of one colour effect as its animation plays them, the colours of each frame being what
    /// <paramref name="coloursOf"/> gives for the frame's shade.
    /// </summary>
    private static JsonObject ColourSteps(ColourEffectId effect, Func<string, JsonObject> coloursOf)
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
                                ["colours"] = coloursOf(ColourEffectShades.Of(definition.Colours, new ColourEffectFrame(effect, shown.Shown)).Key),
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
    /// The frames of a colour effect that plays once through and asks for nothing as it plays, in order: the shade
    /// each shows and the updates it shows for, a part played several times over given as often.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect comes round again, or plays a beat that is not a frame.</exception>
    private static IEnumerable<(SuperMetroid.Core.Assets.PictureShade Shade, int Updates)> ColourFramesPlayedOnce(ColourEffectId effect)
    {
        ColourEffectDefinition definition = ColourEffectAnimations.Definition(effect);
        Animation<ColourEffectSegment> animation = definition.Frames.Played;
        if (animation.End is AnimationEnd.Loop)
            throw new InvalidDataException($"The colour effect {effect} comes round again, where one that plays once through was asked for.");
        foreach (AnimationPhase<ColourEffectSegment> phase in animation.Phases)
        {
            IReadOnlyList<AnimationBeat<int, ColourEffectCue>> beats = ColourEffectAnimations.Segments.Beats(phase.Segment);
            for (int play = 0; play < phase.Plays; play++)
            {
                for (int beat = phase.FirstBeat; beat < phase.FirstBeat + phase.BeatCount; beat++)
                {
                    if (beats[beat] is not AnimationBeat<int, ColourEffectCue>.Frame shown)
                        throw new InvalidDataException($"The colour effect {effect} plays a beat that is not a frame: {beats[beat]}.");
                    yield return (ColourEffectShades.Of(definition.Colours, new ColourEffectFrame(effect, shown.Shown)), shown.Duration);
                }
            }
        }
    }

    /// <summary>
    /// A change of colours taken part of the way, as the old game's fades between two colourings go: each colour the
    /// change shows otherwise is shown so far toward what the change shows for it, each of its three parts by the old
    /// game's own steps of brightness. A colour that has not yet moved is left out, and so is a place none has moved in.
    /// </summary>
    /// <param name="change">The whole change, as <see cref="ShadeChange(string, IReadOnlyDictionary{string, string})"/> writes one.</param>
    /// <param name="progress">How far along the way.</param>
    private static JsonObject ChangePartWay(JsonObject change, SuperMetroid.Core.Assets.FadeLevel progress) => ChangeBetween([], change, progress);

    /// <summary>
    /// A fade between two changes of colours taken part of the way, as the old game's fades between two colourings
    /// go: each colour as the pictures have it is shown so far from what <paramref name="from"/> shows it as toward
    /// what <paramref name="to"/> shows it as, each of its three parts by the old game's own steps of brightness; a
    /// colour a change leaves alone is shown by it as the pictures have it. A colour shown as the pictures have it is
    /// left out, and so is a place none is shown otherwise in.
    /// </summary>
    /// <param name="from">The change the fade starts from, as <see cref="ShadeChange(string, IReadOnlyDictionary{string, string})"/> writes one; empty for the pictures' own colours.</param>
    /// <param name="to">The change the fade ends on, written the same way.</param>
    /// <param name="progress">How far along the way.</param>
    private static JsonObject ChangeBetween(JsonObject from, JsonObject to, SuperMetroid.Core.Assets.FadeLevel progress)
    {
        static SuperMetroid.Core.Assets.RgbColor Read(string written)
        {
            int colour = int.Parse(written.AsSpan(0, 6), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
            return new SuperMetroid.Core.Assets.RgbColor((colour >> 16) & 0xFF, (colour >> 8) & 0xFF, colour & 0xFF);
        }

        static IEnumerable<string> Keys(JsonObject? group) => group?.Select(entry => entry.Key) ?? [];

        var partWay = new JsonObject();
        foreach (string place in Keys(from).Union(Keys(to)).OrderBy(place => place, StringComparer.Ordinal))
        {
            JsonObject? starts = from[place]?.AsObject(), ends = to[place]?.AsObject();
            var moved = new JsonObject();
            foreach (string colour in Keys(starts).Union(Keys(ends)).OrderBy(colour => colour, StringComparer.Ordinal))
            {
                string own = colour[..6];
                SuperMetroid.Core.Compatibility.FadeLevelColour start = SuperMetroid.Core.Compatibility.FadeLevelColour.Of(Read(starts?[colour]?.GetValue<string>() ?? own));
                SuperMetroid.Core.Compatibility.FadeLevelColour end = SuperMetroid.Core.Compatibility.FadeLevelColour.Of(Read(ends?[colour]?.GetValue<string>() ?? own));
                SuperMetroid.Core.Assets.RgbColor shown = start.Map((channel, level) => level + ((end[channel] - level) * progress.Step / progress.Steps)).ToColour();
                string written = ((shown.Red << 16) | (shown.Green << 8) | shown.Blue).ToString("x6", System.Globalization.CultureInfo.InvariantCulture);
                if (written != own)
                    moved[colour] = written;
            }

            if (moved.Count > 0)
                partWay[place] = moved;
        }

        return partWay;
    }

    /// <summary>The most colours a thing drawn in the engine shows changed at once (its <c>Recolouring.MostSwaps</c>).</summary>
    private const int MostSwapsShown = 64;

    /// <summary>The colours a shade shows changed on a room's own pictures, place by place, each place's pictures cut from one sheet of the rooms' blocks.</summary>
    private JsonObject ShadeChange(string shade, IReadOnlyDictionary<string, string> places) =>
        ShadeChange(shade, RoomBlockFamily, places.ToDictionary(place => place.Key, place => (IReadOnlyList<string>)[place.Value], StringComparer.Ordinal));

    /// <summary>
    /// The colours a shade shows changed, place by place, as the engine reads a change of colours: for each place,
    /// every colour the shade changes as the place's pictures have it, with the colour the shade shows for it,
    /// written as six hexadecimal digits. A colour changed only in one region of the pictures is written with the
    /// region's number after a stroke, as the pictures' maps of regions number it. A place the shade changes nothing
    /// in is left out.
    /// </summary>
    /// <param name="shade">The shade, or several with a plus sign between them, a later one laid over an earlier.</param>
    /// <param name="family">The family of the sheets.</param>
    /// <param name="places">Each place with the sheets its pictures were cut from.</param>
    /// <exception cref="InvalidDataException">
    /// The shade changes more colours in one place than a thing can show changed at once, or two sheets of one place
    /// show one colour differently.
    /// </exception>
    private JsonObject ShadeChange(string shade, string family, IReadOnlyDictionary<string, IReadOnlyList<string>> places)
    {
        static string Written(int colour) => colour.ToString("x6", System.Globalization.CultureInfo.InvariantCulture);
        var written = new JsonObject();
        foreach ((string place, IReadOnlyList<string> sheets) in places.OrderBy(place => place.Key, StringComparer.Ordinal))
        {
            var changed = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (string sheet in sheets)
            {
                foreach (ShadeSwap swap in _sheets.ShadeSwaps(family, sheet, shade))
                {
                    string from = swap.Region == ShadeSwap.Everywhere ? Written(swap.From) : $"{Written(swap.From)}/{swap.Region}";
                    if (changed.TryGetValue(from, out string? shown) && shown != Written(swap.To))
                        throw new InvalidDataException($"The shade {shade} shows the colour {from} of {place} as {shown} on one sheet and as {Written(swap.To)} on {sheet}.");
                    changed[from] = Written(swap.To);
                }
            }

            if (changed.Count > MostSwapsShown)
                throw new InvalidDataException($"The shade {shade} changes {changed.Count} colours of {place}; a thing shows at most {MostSwapsShown} changed at once.");
            if (changed.Count == 0)
                continue;
            var colours = new JsonObject();
            foreach ((string from, string to) in changed)
                colours[from] = to;
            written[place] = colours;
        }

        return written;
    }
}
