using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The handshake each sound library's queue runs with its library before it requests the next queued sound. A queue
/// that is ready requests the next sound; it repeats the request on every pass until the library reports it accepted;
/// it then holds the accepted request for <see cref="WithdrawalHoldPasses"/> passes and withdraws it on the last; and it
/// repeats the withdrawal until the library reports nothing, when it is ready again and requests the next sound on that
/// same pass. So the passes a library takes to accept each request and its withdrawal decide on which update every later
/// queued sound of that library starts. A music change's downtime withdraws every library's request without moving any
/// queue on (<see cref="DropAwaitedRequests"/>).
/// </summary>
/// <typeparam name="TLibrary">A sound library; the handshakes find a library by equality and never read its value.</typeparam>
/// <typeparam name="TSound">The sound a library is asked for; the handshakes only compare requests.</typeparam>
/// <remarks>
/// Created once per game session by its composition root (<c>FrontendSession</c>), which supplies the game's libraries,
/// and injected into the session's audio queues, so each handshake's phase, awaited request and hold live as long as the
/// session's queues; a soft reset returns every handshake to ready (<see cref="Reset"/>). Advanced once per library on every
/// sound-queue pass of an update's audio processing, after the music queue's, against what each library reports as
/// accepted to that update (<see cref="ISoundAcceptanceTiming{TLibrary, TSound}"/>). Evidence: the original's per-library
/// sound queues, whose four-state handshake and two-call withdrawal hold the SMV replays trace beside each library's
/// waiting requests on every update (the movie desync comparison prints both), and whose timing the Ridley full-movie and
/// 13% replays depend on through the door transition's wait for empty queues.
/// </remarks>
[CompatibilityService("each sound library's request is repeated until accepted, held and withdrawn, and the withdrawal repeated until accepted, before the library's next queued sound is requested")]
public sealed class SoundRequestHandshakes<TLibrary, TSound>
    where TLibrary : struct, Enum
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>Where one library's queue is in its handshake.</summary>
    public enum Phase
    {
        /// <summary>Ready to request the library's next queued sound; the power-on and reset phase.</summary>
        ReadyToRequest,
        /// <summary>Repeating the requested sound until the library reports it accepted.</summary>
        AwaitingAcceptance,
        /// <summary>Holding the accepted request before withdrawing it.</summary>
        HoldingBeforeWithdrawal,
        /// <summary>Repeating the withdrawal until the library reports nothing.</summary>
        AwaitingWithdrawalAcceptance,
    }

    /// <summary>What one sound-queue pass asks of the library's queue.</summary>
    public enum HandshakeStep
    {
        /// <summary>Nothing this pass.</summary>
        Wait,
        /// <summary>Request the library's next queued sound, if one waits (<see cref="Requested"/>).</summary>
        RequestNext,
        /// <summary>Repeat the request still awaiting acceptance (<see cref="Awaited"/>).</summary>
        RepeatRequest,
        /// <summary>Withdraw the library's request.</summary>
        Withdraw,
    }

    /// <summary>One library's handshake.</summary>
    /// <param name="Phase">Where the queue is.</param>
    /// <param name="Awaited">The sound the queue last requested and has not withdrawn, or none.</param>
    /// <param name="Hold">
    /// While holding, the sound-queue passes before the accepted request is withdrawn, counted once a pass and over on the
    /// pass that withdraws it; otherwise none.
    /// </param>
    internal readonly record struct Handshake(Phase Phase, TSound? Awaited, UpdateCountdown? Hold);

    /// <summary>Sound-queue passes an accepted request is held before it is withdrawn, the withdrawal falling on the last.</summary>
    public const int WithdrawalHoldPasses = 2;

    /// <summary>The libraries, distinct, in the order the handshakes were given them.</summary>
    private readonly ImmutableArray<TLibrary> libraries;

    /// <summary>Each library's handshake, kept by its library (#1755).</summary>
    private readonly Dictionary<TLibrary, Handshake> handshakes;

    /// <summary>Every library's queue ready, awaiting nothing.</summary>
    /// <param name="libraries">The libraries, distinct.</param>
    /// <exception cref="ArgumentException"><paramref name="libraries"/> is default or holds a library twice.</exception>
    public SoundRequestHandshakes(ImmutableArray<TLibrary> libraries)
    {
        if (libraries.IsDefault)
            throw new ArgumentException("The sound libraries are required.", nameof(libraries));
        if (libraries.Distinct().Count() != libraries.Length)
            throw new ArgumentException("Each sound library is listed once.", nameof(libraries));
        this.libraries = libraries;
        handshakes = libraries.ToDictionary(library => library, _ => default(Handshake));
    }

    /// <summary>The sound each library's queue last requested and has not withdrawn, as an acknowledgement of it, or none.</summary>
    public SoundAcknowledgements<TLibrary, TSound> Requests =>
        new(libraries, library => SoundAcknowledgement<TSound>.Of(HandshakeOf(library).Awaited));

    /// <summary>Returns every library's queue to ready, awaiting nothing, as at power-on.</summary>
    public void Reset()
    {
        foreach (TLibrary library in libraries)
            handshakes[library] = default;
    }

    /// <summary>The sound <paramref name="library"/>'s queue last requested and has not withdrawn, or none.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The handshakes were not given <paramref name="library"/>.</exception>
    public TSound? Awaited(TLibrary library) => HandshakeOf(library).Awaited;

    /// <summary>
    /// Runs one pass of <paramref name="library"/>'s handshake against what the library reports as accepted, and
    /// says what the pass asks of the library's queue.
    /// </summary>
    /// <param name="library">The library whose queue runs.</param>
    /// <param name="acceptance">What the library reports as accepted to this update.</param>
    /// <returns>What the pass asks of the library's queue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The handshakes were not given <paramref name="library"/>.</exception>
    /// <exception cref="InvalidDataException">The library's handshake holds a phase no pass can leave.</exception>
    public HandshakeStep Advance(TLibrary library, SoundAcknowledgement<TSound> acceptance)
    {
        Handshake handshake = HandshakeOf(library);
        switch (handshake.Phase)
        {
            case Phase.ReadyToRequest:
                return HandshakeStep.RequestNext;

            case Phase.AwaitingAcceptance:
                if (!acceptance.Acknowledges(handshake.Awaited))
                    return HandshakeStep.RepeatRequest;
                handshakes[library] = handshake with { Phase = Phase.HoldingBeforeWithdrawal, Hold = UpdateCountdown.Lasting(WithdrawalHoldPasses) };
                return HandshakeStep.Wait;

            case Phase.HoldingBeforeWithdrawal:
                UpdateCountdown hold = handshake.Hold
                    ?? throw new InvalidDataException("A sound handshake holds its accepted request with no hold.");
                if (!hold.Count())
                    return HandshakeStep.Wait;
                handshakes[library] = new Handshake(Phase.AwaitingWithdrawalAcceptance, Awaited: null, Hold: null);
                return HandshakeStep.Withdraw;

            case Phase.AwaitingWithdrawalAcceptance:
                if (!acceptance.IsNone)
                    return HandshakeStep.Withdraw;
                handshakes[library] = handshake with { Phase = Phase.ReadyToRequest };
                return HandshakeStep.RequestNext;

            default:
                throw new InvalidDataException($"Sound library {library} holds handshake phase {handshake.Phase}, which no pass can leave.");
        }
    }

    /// <summary>Records that <paramref name="library"/>'s ready queue requested <paramref name="sound"/>; it then awaits its acceptance.</summary>
    /// <param name="library">The library asked.</param>
    /// <param name="sound">The sound dequeued for it.</param>
    /// <exception cref="ArgumentOutOfRangeException">The handshakes were not given <paramref name="library"/>.</exception>
    /// <exception cref="InvalidOperationException">The library's queue is not ready to request.</exception>
    public void Requested(TLibrary library, TSound? sound)
    {
        Phase phase = HandshakeOf(library).Phase;
        if (phase != Phase.ReadyToRequest)
            throw new InvalidOperationException($"Sound library {library} requested a sound while {phase}.");
        handshakes[library] = new Handshake(Phase.AwaitingAcceptance, sound, Hold: null);
    }

    /// <summary>
    /// A music change's downtime withdraws every library's request: each queue forgets the request it awaits but keeps
    /// its phase and hold, so a queue awaiting acceptance then waits for the library to report nothing.
    /// </summary>
    public void DropAwaitedRequests()
    {
        foreach (TLibrary library in libraries)
            handshakes[library] = handshakes[library] with { Awaited = null };
    }

    /// <summary><paramref name="library"/>'s handshake.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The handshakes were not given <paramref name="library"/>.</exception>
    private Handshake HandshakeOf(TLibrary library) => handshakes.TryGetValue(library, out Handshake handshake)
        ? handshake
        : throw new ArgumentOutOfRangeException(nameof(library), library, "The sound handshakes were not given this library.");
}
