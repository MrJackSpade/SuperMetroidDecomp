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

    /// <summary>The timeline that begins, from its first step, where the step it is on ends; null for this one's own next step.</summary>
    private FrameTimeline? _followedBy;

    /// <summary>Whether another timeline begins where the step it is on ends (<see cref="FollowWith"/>).</summary>
    public bool IsFollowed => _followedBy is not null;

    /// <summary>A player at the start of <paramref name="timeline"/>.</summary>
    public TimelinePlayer(FrameTimeline timeline) => Play(timeline);

    /// <summary>
    /// A player picking <paramref name="timeline"/> up at the start of one of its steps, with the first
    /// <paramref name="cuesPassed"/> of that step's cues already behind it.
    /// </summary>
    public static TimelinePlayer Resuming(FrameTimeline timeline, int step, int cuesPassed)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentOutOfRangeException.ThrowIfNegative(cuesPassed);
        if (step < 0 || step >= timeline.Steps.Count)
            throw new ArgumentOutOfRangeException(nameof(step), step, "A timeline is picked up at one of its own steps.");
        var player = new TimelinePlayer(timeline);
        player.Begin(step);
        player.Shown = player.Current;
        player._pendingCues = [.. timeline.Steps[step].Cues.Skip(cuesPassed)];
        return player;
    }

    /// <summary>The timeline being played.</summary>
    public FrameTimeline Timeline { get; private set; } = null!;

    /// <summary>Which step it is on.</summary>
    public int StepIndex { get; private set; }

    /// <summary>
    /// The step it is on: the one the next update falls in. As the last update of a step is played this is already the
    /// step that follows.
    /// </summary>
    public TimelineStep Current => Timeline.Steps[StepIndex];

    /// <summary>
    /// The step the update last played belongs to, and before any has been played the step it starts on. This is what
    /// is shown: a step's picture comes on the update its cues do, and stays through the step's last update.
    /// </summary>
    public TimelineStep Shown { get; private set; } = null!;

    /// <summary>Whether a timeline that does not loop has played through its last step.</summary>
    public bool IsOver { get; private set; }

    /// <summary>Whether the step it is on has only just begun: no update has fallen in it yet, and its cues are still to come.</summary>
    public bool IsAtStepStart { get; private set; }

    /// <summary>Starts <paramref name="timeline"/> from its first step.</summary>
    public void Play(FrameTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        Timeline = timeline;
        _followedBy = null;
        Begin(0);
        Shown = Current;
        IsOver = false;
    }

    /// <summary>
    /// Has <paramref name="timeline"/> follow the step it is on: the step keeps the updates it has left, and where
    /// its own next step would have begun the timeline begins from its first, so the picture showing stays out its
    /// time and none of the cues left of the timeline it leaves come. On a step no update has fallen in yet, or with
    /// its timeline played out, the timeline begins at once, and that step's cues never come; what is shown stays
    /// until the next update is played.
    /// </summary>
    public void FollowWith(FrameTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        if (!IsOver && !IsAtStepStart)
        {
            _followedBy = timeline;
            return;
        }

        Timeline = timeline;
        _followedBy = null;
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
        Shown = Current;
        IsAtStepStart = false;
        _updatesLeft--;
        if (_updatesLeft == 0)
        {
            if (_followedBy is { } next)
            {
                (Timeline, _followedBy) = (next, null);
                Begin(0);
            }
            else if (StepIndex + 1 < Timeline.Steps.Count)
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
        IsAtStepStart = true;
    }
}
