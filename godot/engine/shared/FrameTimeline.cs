using Engine.Content;

namespace Engine.Shared;

/// <summary>
/// A timed sequence gameplay follows: ordered steps, each with a picture, a length in updates and any cues, and what
/// happens after the last step. Animations whose timing matters to play are these, stepped by their owner once per
/// update; Godot's own animation playback is never the clock.
/// </summary>
/// <remarks>
/// <code>
/// { "steps": [ { "picture": "a.png", "updates": 4 }, { "picture": "b.png", "updates": 4, "cues": ["fire"] } ],
///   "loops_to": 0 }
/// </code>
/// </remarks>
public sealed class FrameTimeline
{
    /// <summary>A timeline of <paramref name="steps"/> that loops back to step <paramref name="loopsTo"/>, or ends when that is null.</summary>
    /// <exception cref="ArgumentException">There are no steps, a step lasts no updates, or the loop names no step.</exception>
    public FrameTimeline(IReadOnlyList<TimelineStep> steps, int? loopsTo)
    {
        if (steps.Count == 0)
            throw new ArgumentException("A timeline has at least one step.", nameof(steps));
        if (steps.Any(step => step.Updates <= 0))
            throw new ArgumentException("Every step of a timeline lasts at least one update.", nameof(steps));
        if (loopsTo is < 0 || loopsTo >= steps.Count)
            throw new ArgumentException("A timeline loops back to one of its own steps.", nameof(loopsTo));
        Steps = steps;
        LoopsTo = loopsTo;
    }

    /// <summary>The steps, in order.</summary>
    public IReadOnlyList<TimelineStep> Steps { get; }

    /// <summary>The step played after the last one; null when the timeline ends there.</summary>
    public int? LoopsTo { get; }

    /// <summary>Reads a timeline from a data file.</summary>
    public static FrameTimeline Read(DataNode data)
    {
        var steps = data.List("steps").Select(step => new TimelineStep(
            step.Has("picture") ? step.Text("picture") : "",
            step.Int("updates"),
            step.Has("cues") ? step.List("cues").Select(cue => cue.AsText()).ToList() : [])).ToList();
        int? loopsTo = data.Has("loops_to") ? data.Int("loops_to") : null;
        try
        {
            return new FrameTimeline(steps, loopsTo);
        }
        catch (ArgumentException exception)
        {
            throw data.Problem($"is not a usable timeline: {exception.Message}");
        }
    }
}
