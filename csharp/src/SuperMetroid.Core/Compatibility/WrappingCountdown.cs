namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A frame countdown that keeps counting past zero instead of stopping there, as every timer of the game does.
/// <para>
/// <b>Behaviour reproduced.</b> A countdown holds 0 through 65535 frames. Stepping it with <see cref="Decrement"/> takes
/// one frame away and wraps a countdown already at zero to 65535, so a timer that is stepped once more than its owner
/// expected does not stop: it waits 65535 further steps before it reaches zero again. Each owner decides how its timer
/// expires, and the choice is gameplay-observable:
/// <list type="bullet">
/// <item><b>Expires on reaching zero</b> (<see cref="CountdownStep.IsZero"/>): a timer started at N expires on its Nth
/// step. A timer started at zero wraps instead and waits 65535 steps.</item>
/// <item><b>Expires on running past zero</b> (<see cref="CountdownStep.IsNegative"/>): a timer started at N expires on
/// step N + 1; a timer started at zero expires on the first step.</item>
/// <item><b>Expires on either</b> (<see cref="CountdownStep.IsZeroOrNegative"/>).</item>
/// <item><b>Stops at zero</b> (<see cref="DecrementToZero"/>): the one form that never wraps.</item>
/// </list>
/// </para>
/// <para>
/// <b>Negative.</b> A countdown at 32768 or more is negative (<see cref="IsNegative"/>): it has run past zero, and an owner
/// that tests for expiry by sign treats it as expired. A timer set that high on purpose (the escape's 65535-frame
/// earthquake) therefore reads as already expired to such an owner while it keeps counting for tens of thousands of
/// frames to an owner that waits for zero.
/// </para>
/// <para>
/// <b>Sustained.</b> A producer that renews a running timer every update without restarting it (<see cref="Sustained"/>)
/// keeps it in the later half of a round of twice the sustained frames, so the timer never runs out while the producer
/// renews it and runs out within one round once it stops: rising lava's and the Tourian statues' earthquakes, and the
/// plant that holds Samus invincible.
/// </para>
/// <para>
/// <b>Frame point.</b> The countdown has none of its own: its owner advances it once per call of the owner's update (an
/// enemy's AI, a projectile's frame behaviour, a frontend phase's update) and tests the step there.
/// </para>
/// <para>
/// <b>Evidence:</b> the original game's timers are sixteen-bit words its routines decrement and test by sign or by zero,
/// with no clamp; the full-movie replays (the Ridley full movie and the 13% SMV) step every enemy, boss, Samus and frontend
/// countdown through this type and match the recording on every update. The wrap itself is observable where a timer is
/// stepped from zero: the Ceres hall's debris waits as long as the elevator shaft's last turn step reads as a countdown
/// (<see cref="RoomMainCarryover"/>).
/// </para>
/// <para>
/// Gameplay reads a countdown only through these named tests and the <see cref="RemainingFrames"/> it has left; checks and
/// codecs read its stored count through test-support.
/// </para>
/// </summary>
[CompatibilityService("a countdown that keeps the value it wrapped to, so a timer decremented past zero reads as a large or negative count")]
[EncodingOwner("wrapping sixteen-bit countdown")]
public readonly record struct WrappingCountdown
{
    /// <summary>The frames at which a countdown reads as negative: it has run past zero.</summary>
    private const int NegativeFrom = 1 << 15;

    /// <summary>
    /// The most frames a countdown can start at, 65535: one less than the frames a countdown started at zero waits
    /// after wrapping. A duration or delay that is only ever loaded into a countdown is bounded by it.
    /// </summary>
    public const int LongestStart = (1 << 16) - 1;

    /// <summary>The countdown's count, 0 through 65535.</summary>
    [IntegerQuantity("the countdown's remaining count")]
    private readonly ushort count;

    /// <summary>Stores <paramref name="frames"/> exactly; callers use <see cref="Start"/> or <see cref="FromRaw"/>.</summary>
    private WrappingCountdown(ushort frames) => count = frames;

    /// <summary>
    /// The frames left on the countdown, 0 through 65535, read where an owner uses its timer's length: as an index into a
    /// table of positions or offsets, as a duration handed to another owner, or as the count a step compares. A countdown
    /// run past zero reads as the large count it wrapped to.
    /// </summary>
    public int RemainingFrames => count;

    /// <summary>Whether the countdown has no frames left.</summary>
    public bool IsZero => count == 0;

    /// <summary>Whether the countdown has run past zero (32768 or more): expired, for an owner that tests by sign.</summary>
    public bool IsNegative => count >= NegativeFrom;

    /// <summary>Whether the countdown has no frames left or has run past zero.</summary>
    public bool IsZeroOrNegative => IsZero || IsNegative;

    /// <summary>Whether exactly <paramref name="frames"/> frames are left.</summary>
    /// <param name="frames">The frames compared, 0 through 65535.</param>
    public bool IsAt(int frames) => count == frames;

    /// <summary>Whether at least <paramref name="frames"/> frames are left; a countdown run past zero counts as a large count.</summary>
    /// <param name="frames">The frames compared, 0 through 65535.</param>
    public bool IsAtLeast(int frames) => count >= frames;

    /// <summary>
    /// Whether the countdown is in the second half of a cycle that alternates every <paramref name="phaseFrames"/>
    /// frames, the way a hit flash or a shake alternates between its two looks as the timer runs down.
    /// </summary>
    /// <param name="phaseFrames">The frames each look lasts: 1, 2, 4 or another positive length.</param>
    public bool IsInAlternatePhase(int phaseFrames) => count / Ensure.BetweenInclusive(1, ushort.MaxValue, phaseFrames) % 2 != 0;

    /// <summary>Whether the frames left are a whole number of <paramref name="periodFrames"/>: a periodic action's frame.</summary>
    /// <param name="periodFrames">The period, a positive number of frames.</param>
    public bool IsOnMultipleOf(int periodFrames) => count % Ensure.BetweenInclusive(1, ushort.MaxValue, periodFrames) == 0;

    /// <summary>
    /// The position in a cycle of <paramref name="phases"/> looks, each lasting <paramref name="phaseFrames"/> frames, that
    /// the frames left select: a shake offset or palette that steps as the timer runs down.
    /// </summary>
    /// <param name="phaseFrames">The frames each look lasts, positive.</param>
    /// <param name="phases">The looks in the cycle, positive.</param>
    public int CyclePhase(int phaseFrames, int phases) => count / Ensure.BetweenInclusive(1, ushort.MaxValue, phaseFrames) % Ensure.BetweenInclusive(1, ushort.MaxValue, phases);

    /// <summary>
    /// The countdown renewed by a producer that keeps it running for at least <paramref name="frames"/> without restarting
    /// it. The countdown runs in rounds of twice <paramref name="frames"/>: one in the first <paramref name="frames"/> of its
    /// round moves on by <paramref name="frames"/>, and one in the later half is left as it is. It never shortens a running
    /// countdown. Renewed every update, a countdown below twice <paramref name="frames"/> therefore cycles through the later
    /// half and never runs out; once the renewals stop it runs out within one round, from wherever in the round it was.
    /// </summary>
    /// <param name="frames">
    /// The frames sustained: a power of two from 1 through 32768, so that the rounds divide the countdown's 65536 counts
    /// and a renewal never carries past its round.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frames"/> is not such a power of two.</exception>
    public WrappingCountdown Sustained(int frames)
    {
        if (!int.IsPow2(Ensure.BetweenInclusive(1, NegativeFrom, frames)))
            throw new ArgumentOutOfRangeException(nameof(frames), frames, "Sustained frames divide the countdown's rounds only as a power of two.");
        return CyclePhase(frames, 2) == 0 ? AddFrames(frames) : this;
    }

    /// <summary>
    /// Starts a countdown at <paramref name="frames"/>, 0 through 65535.
    /// </summary>
    public static WrappingCountdown Start(int frames) =>
        new((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, frames));

    /// <summary>
    /// A countdown holding exactly <paramref name="frames"/>, 0 through 65535, including a count that already reads as
    /// run past zero; used where a timer is set from a decoded duration or carried from another owner.
    /// </summary>
    public static WrappingCountdown FromRaw(int frames) => new((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, frames));

    /// <summary>
    /// Takes one frame away, wrapping a countdown at zero to 65535. Store <see cref="CountdownStep.Remaining"/> and test
    /// the owner's expiry rule on the step: <see cref="CountdownStep.IsZero"/> for a timer that expires on reaching zero,
    /// <see cref="CountdownStep.IsNegative"/> for one that expires on running past it, or
    /// <see cref="CountdownStep.IsZeroOrNegative"/> for either.
    /// </summary>
    public CountdownStep Decrement() => WrappingCounter.Decrement(count);

    /// <summary>Takes one frame away from a countdown that has frames left; a countdown at zero stays at zero.</summary>
    public WrappingCountdown DecrementToZero() => count == 0 ? this : new((ushort)(count - 1));

    /// <summary>
    /// Adds <paramref name="frames"/> to the countdown (a negative count takes frames away): a sum past 65535 restarts
    /// from zero and a difference below zero becomes a large count, so a timer extended or shortened across the boundary
    /// runs for the wrapped count, not the intended one.
    /// </summary>
    /// <param name="frames">The frames to add, -65535 through 65535.</param>
    public WrappingCountdown AddFrames(int frames) =>
        new(unchecked((ushort)(count + Ensure.BetweenInclusive(-ushort.MaxValue, ushort.MaxValue, frames))));

    /// <inheritdoc />
    public override string ToString() => $"{count} frames";
}
