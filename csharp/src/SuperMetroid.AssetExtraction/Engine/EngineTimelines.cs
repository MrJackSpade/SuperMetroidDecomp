using System.Text.Json.Nodes;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Turns the old game's animations into the Godot game's timelines. An old animation is beats: a frame shown for so
/// many updates, a frame held for good, a pause, a while with nothing shown, and events raised between frames. A timeline is steps, each a
/// picture shown for so many updates with the cues reached as it begins. An event becomes a cue of the step that
/// follows it, which the Godot game reaches on that step's first update, as the old game raised it on the update the
/// frame before it ran out.
/// </summary>
internal static partial class EngineTimelines
{
    /// <summary>What stands between the cues of an event that becomes more than one.</summary>
    internal const char And = '|';

    /// <summary>The timeline of one of the old game's animations.</summary>
    /// <typeparam name="TSegment">The animation's segments.</typeparam>
    /// <typeparam name="TFrame">Its frames.</typeparam>
    /// <typeparam name="TEvent">Its events.</typeparam>
    /// <param name="segments">The beats of every segment.</param>
    /// <param name="animation">The animation.</param>
    /// <param name="picture">The picture of a frame, as its path in the asset tree.</param>
    /// <param name="cue">
    /// The cue an event becomes, or null for an event the Godot game does not hear. An event that is two things to the
    /// Godot game gives both, with <see cref="And"/> between them.
    /// </param>
    internal static JsonObject Of<TSegment, TFrame, TEvent, TBeat, TShown>(
        AnimationSegmentPlay<TSegment, TFrame, TEvent, TBeat, TShown> segments, Animation<TSegment> animation, Func<TFrame, string> picture,
        Func<TEvent, string?> cue)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<TFrame, TEvent>
        where TShown : class
    {
        (List<IAnimationBeat<TFrame, TEvent>> lead, List<IAnimationBeat<TFrame, TEvent>>? cycle) = Played(segments, animation);
        return Of<TFrame, TEvent>(lead, cycle, picture, cue);
    }

    /// <summary>
    /// The beats one of the old game's animations plays: those it plays once, and then those it plays over and over,
    /// where it does.
    /// </summary>
    internal static (List<IAnimationBeat<TFrame, TEvent>> Lead, List<IAnimationBeat<TFrame, TEvent>>? Cycle) Played<TSegment, TFrame, TEvent, TBeat, TShown>(
        AnimationSegmentPlay<TSegment, TFrame, TEvent, TBeat, TShown> segments, Animation<TSegment> animation)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<TFrame, TEvent>
        where TShown : class
    {
        int loopsFrom = animation.End is AnimationEnd.Loop loop ? loop.Phase : animation.Phases.Count;
        var lead = new List<IAnimationBeat<TFrame, TEvent>>();
        var cycle = new List<IAnimationBeat<TFrame, TEvent>>();
        for (int phase = 0; phase < animation.Phases.Count; phase++)
        {
            AnimationPhase<TSegment> played = animation.Phases[phase];
            IEnumerable<IAnimationBeat<TFrame, TEvent>> beats = segments.Beats(played.Segment).Skip(played.FirstBeat).Take(played.BeatCount);
            for (int play = 0; play < played.Plays; play++)
                (phase < loopsFrom ? lead : cycle).AddRange(beats);
        }

        return (lead, animation.End is AnimationEnd.Loop ? cycle : null);
    }

    /// <summary>The timeline of one whole segment, played once or over and over.</summary>
    internal static JsonObject Of<TSegment, TFrame, TEvent, TBeat, TShown>(
        AnimationSegmentPlay<TSegment, TFrame, TEvent, TBeat, TShown> segments, TSegment segment, bool loops, Func<TFrame, string> picture,
        Func<TEvent, string?> cue)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
        where TBeat : class, IAnimationBeat<TFrame, TEvent>
        where TShown : class =>
        loops ? Of<TFrame, TEvent>([], segments.Beats(segment), picture, cue) : Of<TFrame, TEvent>(segments.Beats(segment), null, picture, cue);

    /// <summary>
    /// The timeline of beats played once and then, where there is a cycle, of the cycle's beats played over and over.
    /// </summary>
    internal static JsonObject Of<TFrame, TEvent>(
        IEnumerable<IAnimationBeat<TFrame, TEvent>> lead, IEnumerable<IAnimationBeat<TFrame, TEvent>>? cycle, Func<TFrame, string> picture,
        Func<TEvent, string?> cue)
        where TFrame : struct
        where TEvent : notnull
    {
        var steps = new JsonArray();
        var pending = new List<string>();
        string shown = "";
        int? restsOn = null;

        void Add(string path, int updates)
        {
            if (updates <= 0)
                throw new InvalidDataException("An animation shows a frame for no updates, which a timeline cannot.");
            var step = new JsonObject { ["picture"] = path, ["updates"] = updates };
            if (pending.Count > 0)
            {
                var cues = new JsonArray();
                foreach (string raised in pending)
                    cues.Add(raised);
                step["cues"] = cues;
                pending.Clear();
            }

            steps.Add(step);
            shown = path;
        }

        // Plays beats into steps; false once a frame held for good has ended them.
        bool Play(IEnumerable<IAnimationBeat<TFrame, TEvent>> beats)
        {
            foreach (IAnimationBeat<TFrame, TEvent> beat in beats)
            {
                switch (beat)
                {
                    case AnimationHiddenBeat<TFrame, TEvent> hidden:
                        Add("", hidden.Duration);
                        break;
                    case AnimationBeat<TFrame, TEvent>.Frame frame:
                        Add(picture(frame.Shown), frame.Duration);
                        break;
                    case AnimationBeat<TFrame, TEvent>.Pause pause:
                        Add(shown, pause.Duration);
                        break;
                    case AnimationBeat<TFrame, TEvent>.Event raised:
                        if (cue(raised.Raised) is { } heard)
                            pending.AddRange(heard.Split(And));
                        break;
                    case AnimationBeat<TFrame, TEvent>.Marker marked:
                        if (cue(marked.Raised) is { } reached)
                            pending.AddRange(reached.Split(And));
                        break;
                    case AnimationBeat<TFrame, TEvent>.HeldFrame held:
                        // The animation rests here: a step that comes round to itself. What was raised before the
                        // held frame is reached once, as the frame first shows, on a step of its own before the
                        // rest; on the resting step it would be reached again every time that came round.
                        if (pending.Count > 0)
                            Add(picture(held.Shown), 1);
                        Add(picture(held.Shown), 1);
                        restsOn = steps.Count - 1;
                        return false;
                    default:
                        throw new InvalidDataException($"An animation has a beat this does not know: {beat.GetType().Name}.");
                }
            }

            return true;
        }

        var timeline = new JsonObject();
        if (Play(lead) && cycle is not null)
        {
            // Cues left over from the lead are reached once, as the cycle first begins; cues left over at the cycle's
            // end are reached each time it comes round. Where there are either, the cycle is written twice: once to
            // be entered, once to come round to.
            IReadOnlyList<IAnimationBeat<TFrame, TEvent>> round = [.. cycle];
            int first = steps.Count;
            bool carriesIn = pending.Count > 0;
            if (Play(round))
            {
                if (pending.Count > 0 || carriesIn)
                {
                    first = steps.Count;
                    if (!Play(round))
                        throw new InvalidDataException("An animation's cycle rests on a held frame the second time round but not the first.");
                    pending.Clear();
                }

                if (steps.Count == first)
                    throw new InvalidDataException("An animation's cycle shows no frame.");
                restsOn = first;
            }
        }
        else if (restsOn is null && pending.Count > 0)
        {
            // An event after the last frame is reached as that frame runs out: one more update of the same picture.
            Add(shown, 1);
        }

        if (steps.Count == 0)
            throw new InvalidDataException("An animation shows no frame.");
        timeline["steps"] = steps;
        if (restsOn is { } loopsTo)
            timeline["loops_to"] = loopsTo;
        return timeline;
    }
}
