namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When a Yapping Maw may grab Samus (#2454). The Maw's cooldown starts at <see cref="YappingMawAttackDefinitions.GrabCooldown"/> when its tongue finishes
/// retracting, when it lets Samus go and on every update it waits with Samus closer than <see cref="YappingMawAttackDefinitions.GrabSafetyDistance"/>, and at none when it
/// is placed and when it grabs her. Counted down past none, it lets the Maw grab for the next 32768 updates; then it
/// refuses for 32768 updates, lets the Maw grab for the 32768 after those, and so on in rounds of 65536 updates, until
/// the cooldown starts again.
/// </summary>
/// <remarks>
/// <para>
/// <b>When it matters:</b> the Maw updates only while it is on screen, and nothing starts its cooldown while Samus stays
/// further from it than its reach. A Samus who keeps a Maw on screen for 32768 of its updates without coming within its
/// reach or within its safety distance of it finds its next bite closing on her without holding her; that bite's retraction starts
/// the cooldown again, so the bite after it grabs as usual.
/// </para>
/// <para>
/// <b>Frame point:</b> the Maw counts the cooldown once at the start of each of its updates, before its action runs
/// (<see cref="Counted"/>), and its touch reaction asks whether it may grab (<see cref="AllowsGrab"/>).
/// </para>
/// <para>
/// <b>Evidence:</b> the Maw's main update (<c>$A8:A214</c>-<c>$A8:A219</c>) decrements its cooldown word every update
/// with no clamp; its waiting function stores 48 when Samus is closer than 32 pixels (<c>$A8:A284</c>), and the attack's
/// end (<c>$A8:A5DB</c>) and the release from the cooldown (<c>$A8:A6B3</c>) store 48; its touch routine grabs only
/// while the word is negative (<c>BPL</c> at <c>$A8:A7A0</c> returns) and stores zero as it grabs (<c>$A8:A7AB</c>).
/// The Yapping Maw grab cooldown check counts the production cooldown through two full rounds.
/// </para>
/// </remarks>
[CompatibilityService("a Yapping Maw may grab Samus only in alternate runs of 32768 updates after its cooldown runs out, the first starting as the cooldown runs out")]
public readonly record struct YappingMawGrabCooldown
{
    /// <summary>The countdown whose sign decides whether the Maw may grab.</summary>
    private readonly WrappingCountdown countdown;

    /// <summary>Holds <paramref name="countdown"/>.</summary>
    private YappingMawGrabCooldown(WrappingCountdown countdown) => this.countdown = countdown;

    /// <summary>
    /// A cooldown with no updates left, as a newly placed Maw has and as a grab leaves it: the Maw may grab from its next
    /// update on, the one that counts it past none.
    /// </summary>
    public static YappingMawGrabCooldown Spent => default;

    /// <summary>
    /// A cooldown started at <paramref name="updates"/>: the Maw refuses to grab for those updates and may grab from the
    /// update after them.
    /// </summary>
    /// <param name="updates">The updates the Maw holds off, 0 through 32767.</param>
    public static YappingMawGrabCooldown Started(int updates) =>
        new(WrappingCountdown.Start(Ensure.BetweenInclusive(0, short.MaxValue, updates)));

    /// <summary>Whether the Maw may grab Samus: its cooldown has run out and not yet come round to a wait again.</summary>
    public bool AllowsGrab => countdown.IsNegative;

    /// <summary>The cooldown one of the Maw's updates later.</summary>
    public YappingMawGrabCooldown Counted() => new(countdown.Decrement().Remaining);

    /// <inheritdoc />
    public override string ToString() => AllowsGrab ? $"allows grab ({countdown})" : $"refuses grab ({countdown})";
}
