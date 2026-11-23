using Engine.Shared;

namespace Engine.Actors.Components;

/// <summary>
/// Steps a <see cref="FrameTimeline"/> once per update, when its owner says. It reports the step it is on and the cues
/// reached; the owner acts on them. This is how gameplay-timed animation works.
/// </summary>
public sealed class TimelinePlayer
{
    /// <summary>The updates left in the current step.</summary>
    private int _updatesLeft;

    /// <summary>The cues of the step that has just begun and has not yet been through an update.</summary>
    private IReadOnlyList<string> _pendingCues = [];

    /// <summary>A player at the start of <paramref name="timeline"/>.</summary>
    public TimelinePlayer(FrameTimeline timeline) => Play(timeline);

    /// <summary>The timeline being played.</summary>
    public FrameTimeline Timeline { get; private set; } = null!;

    /// <summary>Which step it is on.</summary>
    public int StepIndex { get; private set; }

    /// <summary>The step it is on.</summary>
    public TimelineStep Current => Timeline.Steps[StepIndex];

    /// <summary>Whether a timeline that does not loop has played through its last step.</summary>
    public bool IsOver { get; private set; }

    /// <summary>Starts <paramref name="timeline"/> from its first step.</summary>
    public void Play(FrameTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        Timeline = timeline;
        Begin(0);
        IsOver = false;
    }

    /// <summary>
    /// Plays one update and returns the cues reached in it: those of the step the update falls in, the first time an
    /// update falls in it.
    /// </summary>
    public IReadOnlyList<string> Step()
    {
        if (IsOver)
            return [];
        IReadOnlyList<string> cues = _pendingCues;
        _pendingCues = [];
        _updatesLeft--;
        if (_updatesLeft == 0)
        {
            if (StepIndex + 1 < Timeline.Steps.Count)
                Begin(StepIndex + 1);
            else if (Timeline.LoopsTo is int loop)
                Begin(loop);
            else
                IsOver = true;
        }

        return cues;
    }

    /// <summary>Begins a step.</summary>
    private void Begin(int index)
    {
        StepIndex = index;
        _updatesLeft = Timeline.Steps[index].Updates;
        _pendingCues = Timeline.Steps[index].Cues;
    }
}
