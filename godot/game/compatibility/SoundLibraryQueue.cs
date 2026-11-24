using Engine.Core;

namespace Game.Compatibility;

/// <summary>
/// One of the cartridge's three libraries of sounds, as the game deals with it: the sounds asked for that wait their
/// turn, oldest first, and the exchange by which one at a time is handed to the sound program. A sound is let in
/// while fewer wait than its priority bears. On each turn a library that is ready asks the program for its oldest
/// waiting sound; it asks again every turn until the program reports that sound back; it then holds for two turns
/// and withdraws what it asked on the second; and it withdraws again every turn until the program reports nothing,
/// when it is ready and asks for the next on that same turn.
/// </summary>
internal sealed class SoundLibraryQueue
{
    /// <summary>The most sounds that can wait.</summary>
    public const int Most = 15;

    /// <summary>The turns a sound the program reported back is held before it is withdrawn, the withdrawal on the last.</summary>
    private const int HoldTurns = 2;

    private enum Phase
    {
        Ready,
        AwaitingSound,
        Holding,
        AwaitingWithdrawal,
    }

    private readonly List<string> _waiting = [];
    private Phase _phase = Phase.Ready;
    private string? _awaited;
    private int _hold;

    /// <summary>How many sounds wait.</summary>
    public int Waiting => _waiting.Count;

    /// <summary>
    /// Takes a sound that is asked for. It is let in while fewer wait than its priority bears; one let in joins those
    /// waiting unless the game is turning sounds away, and counts as let in all the same.
    /// </summary>
    public SoundAdmission Admit(string sound, SoundPriority priority, bool turnedAway)
    {
        var admission = new SoundAdmission(_waiting.Count < Bears(priority), _waiting.Count);
        if (admission.Admitted && !turnedAway)
            _waiting.Add(sound);
        return admission;
    }

    /// <summary>
    /// Plays one turn against what the program reports back for this library. True when the turn asks something of
    /// the program: <paramref name="asked"/>, or the withdrawal of what was asked when it is nothing.
    /// </summary>
    public bool TakeTurn(string? reported, out string? asked)
    {
        asked = null;
        switch (_phase)
        {
            case Phase.Ready:
                return AskNext(out asked);

            case Phase.AwaitingSound:
                if (reported != _awaited)
                {
                    asked = _awaited;
                    return true;
                }

                _phase = Phase.Holding;
                _hold = HoldTurns;
                return false;

            case Phase.Holding:
                if (--_hold > 0)
                    return false;
                _phase = Phase.AwaitingWithdrawal;
                _awaited = null;
                return true;

            case Phase.AwaitingWithdrawal:
                if (reported is not null)
                    return true;
                _phase = Phase.Ready;
                return AskNext(out asked);

            default:
                throw new InvalidOperationException($"Undefined phase {_phase} of a library's exchange with the sound program.");
        }
    }

    /// <summary>
    /// Forgets the sound the exchange awaits, where it stands, as a change of music does: a library awaiting its
    /// sound then waits for the program to report nothing.
    /// </summary>
    public void DropAwaited() => _awaited = null;

    private bool AskNext(out string? asked)
    {
        asked = null;
        if (_waiting.Count == 0)
            return false;
        asked = _waiting[0];
        _waiting.RemoveAt(0);
        _awaited = asked;
        _phase = Phase.AwaitingSound;
        return true;
    }

    /// <summary>How many sounds may wait before one of <paramref name="priority"/> is turned away.</summary>
    private static int Bears(SoundPriority priority) => priority switch
    {
        SoundPriority.WhenIdle => 1,
        SoundPriority.Low => 3,
        SoundPriority.Normal => 6,
        SoundPriority.High => 9,
        SoundPriority.Essential => Most,
        _ => throw new InvalidOperationException($"Undefined priority {priority} of a sound."),
    };
}
