using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A door cap that plays authored sequences of its images (#1554): the blue, grey and coloured caps. On each update the
/// cap first reacts to a shot it is waiting for; then, unless it rests, it counts down the updates its image stays and,
/// when they run out, draws its sequence's next step, or, after the last step, ends the sequence and decides what comes
/// next. A sequence the cap starts while it updates draws its first step on that same update, as the original game's
/// caps continued into their next list on the same update. A cap just placed takes its first look on its first update.
/// </summary>
/// <param name="orientation">The cap's facing.</param>
/// <param name="paint">The paint its closed images show.</param>
internal abstract class DoorCapObject(ColoredDoorOrientation orientation, DoorCapPaint paint) : RoomObjectBehaviour
{
    /// <summary>The cap's facing.</summary>
    internal ColoredDoorOrientation Orientation { get; } = orientation;

    /// <summary>The paint the cap's closed images show.</summary>
    internal DoorCapPaint Paint { get; } = paint;

    /// <summary>The sequence the cap plays; a cap just placed plays none, so its first update ends it at once.</summary>
    private IReadOnlyList<DoorCapStep> _sequence = [];

    /// <summary>How many of the sequence's steps have drawn.</summary>
    private int _stepsShown;

    /// <summary>The updates left before the sequence's next step, or before it ends after its last step.</summary>
    internal int UpdatesBeforeNextStep { get; private set; } = 1;

    /// <summary>Whether the cap rests on its last image, counting nothing until a shot or a closing wakes it.</summary>
    internal bool Resting { get; private set; }

    /// <inheritdoc/>
    internal sealed override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (ReactToShot(place, context))
            return;
        if (Resting)
            return;
        UpdatesBeforeNextStep--;
        if (UpdatesBeforeNextStep != 0)
            return;
        Step(place, context);
    }

    /// <summary>
    /// Reacts to the shot pending at the cap's block, before its images count down; true when the reaction started a new
    /// sequence, which then drew this update's step. Nothing by default.
    /// </summary>
    /// <param name="place">The cap's place, which holds its block and the pending shot.</param>
    /// <param name="context">The update.</param>
    private protected virtual bool ReactToShot(RoomObjectSlot place, RoomObjectStepContext context) => false;

    /// <summary>Decides what follows the sequence that just ended, on the update its last step's updates ran out.</summary>
    /// <param name="place">The cap's place.</param>
    /// <param name="context">The update.</param>
    private protected abstract void SequenceEnded(RoomObjectSlot place, RoomObjectStepContext context);

    /// <summary>Plays <paramref name="sequence"/> from its first step, which draws on this update.</summary>
    private protected void Play(IReadOnlyList<DoorCapStep> sequence, RoomObjectSlot place, RoomObjectStepContext context)
    {
        Start(sequence);
        Step(place, context);
    }

    /// <summary>Plays <paramref name="sequence"/> from its first step, which draws on the cap's next update.</summary>
    private protected void PlayFromNextUpdate(IReadOnlyList<DoorCapStep> sequence)
    {
        Start(sequence);
        UpdatesBeforeNextStep = 1;
    }

    /// <summary>Rests on the image last drawn until a shot or a closing wakes the cap.</summary>
    private protected void Rest() => Resting = true;

    /// <summary>Makes <paramref name="sequence"/> the one the cap plays, from its first step.</summary>
    private void Start(IReadOnlyList<DoorCapStep> sequence)
    {
        if (Ensure.NotNull(sequence).Count == 0)
            throw new InvalidOperationException("A door cap's sequence draws at least one image.");
        _sequence = sequence;
        _stepsShown = 0;
        Resting = false;
    }

    /// <summary>Draws the sequence's next step, with its sound, or ends the sequence after its last step.</summary>
    private void Step(RoomObjectSlot place, RoomObjectStepContext context)
    {
        if (_stepsShown == _sequence.Count)
        {
            SequenceEnded(place, context);
            return;
        }
        DoorCapStep step = _sequence[_stepsShown];
        _stepsShown++;
        if (step.SoundBefore is { } sound)
            context.QueueSound(sound, DoorCapSequenceDefinitions.SoundPriority);
        context.Draw(place, DoorCaps.Image(Paint, Orientation, step.Image));
        UpdatesBeforeNextStep = Ensure.BetweenInclusive(1, int.MaxValue, step.Updates);
    }
}
