namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A room object that plays authored sequences of timed steps from its catalog (#1554): the door caps and the eye
/// doors. On each update the object first reacts to what it waits for (a shot, an opened door); then, unless it rests,
/// it counts down the updates its shown step stays and, when they run out, shows its sequence's next step, or, after
/// the last step, ends the sequence and decides what comes next. A sequence the object starts while it updates shows
/// its first step on that same update, as the original game's objects continued into their next list on the same
/// update. An object just placed decides on its first update, ending its empty first sequence there.
/// </summary>
/// <typeparam name="TStep">One step of the object's sequences.</typeparam>
internal abstract class SequencedRoomObject<TStep> : RoomObjectBehaviour
    where TStep : struct
{
    /// <summary>The sequence the object plays; one just placed plays none, so its first update ends it at once.</summary>
    private IReadOnlyList<TStep> _sequence = [];

    /// <summary>How many of the sequence's steps have shown.</summary>
    private int _stepsShown;

    /// <summary>The updates left before the sequence's next step, or before it ends after its last step.</summary>
    internal int UpdatesBeforeNextStep { get; private set; } = 1;

    /// <summary>Whether the object rests on its last step, counting nothing until what it waits for wakes it.</summary>
    internal bool Resting { get; private set; }

    /// <inheritdoc/>
    internal sealed override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (React(place, context))
            return;
        if (Resting)
            return;
        UpdatesBeforeNextStep--;
        if (UpdatesBeforeNextStep != 0)
            return;
        Step(place, context);
    }

    /// <summary>
    /// Reacts to what the object waits for, before its steps count down; true when the reaction started a new sequence,
    /// which then showed this update's step, or ended the object. Nothing by default.
    /// </summary>
    /// <param name="place">The object's place, which holds its block and a pending shot.</param>
    /// <param name="context">The update.</param>
    private protected virtual bool React(RoomObjectSlot place, RoomObjectStepContext context) => false;

    /// <summary>Decides what follows the sequence that just ended, on the update its last step's updates ran out.</summary>
    /// <param name="place">The object's place.</param>
    /// <param name="context">The update.</param>
    private protected abstract void SequenceEnded(RoomObjectSlot place, RoomObjectStepContext context);

    /// <summary>Shows <paramref name="step"/> at the object's place, with whatever comes with it; returns the updates it stays.</summary>
    /// <param name="step">The step.</param>
    /// <param name="place">The object's place.</param>
    /// <param name="context">The update.</param>
    private protected abstract int Show(TStep step, RoomObjectSlot place, RoomObjectStepContext context);

    /// <summary>Plays <paramref name="sequence"/> from its first step, which shows on this update.</summary>
    private protected void Play(IReadOnlyList<TStep> sequence, RoomObjectSlot place, RoomObjectStepContext context)
    {
        Start(sequence);
        Step(place, context);
    }

    /// <summary>Plays <paramref name="sequence"/> from its first step, which shows on the object's next update.</summary>
    private protected void PlayFromNextUpdate(IReadOnlyList<TStep> sequence)
    {
        Start(sequence);
        UpdatesBeforeNextStep = 1;
    }

    /// <summary>Ends the object's sequence on its next update without showing anything more.</summary>
    private protected void EndOnNextUpdate()
    {
        _sequence = [];
        _stepsShown = 0;
        Resting = false;
        UpdatesBeforeNextStep = 1;
    }

    /// <summary>Rests on the step last shown until what the object waits for wakes it.</summary>
    private protected void Rest() => Resting = true;

    /// <summary>Makes <paramref name="sequence"/> the one the object plays, from its first step.</summary>
    private void Start(IReadOnlyList<TStep> sequence)
    {
        if (Ensure.NotNull(sequence).Count == 0)
            throw new InvalidOperationException("A room object's sequence shows at least one step.");
        _sequence = sequence;
        _stepsShown = 0;
        Resting = false;
    }

    /// <summary>Shows the sequence's next step, or ends the sequence after its last step.</summary>
    private void Step(RoomObjectSlot place, RoomObjectStepContext context)
    {
        if (_stepsShown == _sequence.Count)
        {
            SequenceEnded(place, context);
            return;
        }
        TStep step = _sequence[_stepsShown];
        _stepsShown++;
        UpdatesBeforeNextStep = Ensure.BetweenInclusive(1, int.MaxValue, Show(step, place, context));
    }
}
