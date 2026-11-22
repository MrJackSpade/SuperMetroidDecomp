namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where Ceres Ridley's getaway flight along its curves (<see cref="CeresRidleyGetawayCurve.Steps"/>) stands (#2553): not
/// yet begun, so the room shows no getaway; flying, with the step its next update flies; or flown, every step run, so the
/// update after the last ends the getaway.
/// </summary>
public abstract partial record CeresGetawayFlight
{
    /// <summary>Only the cases below exist.</summary>
    private CeresGetawayFlight()
    {
    }

    /// <summary>Ridley has not handed the room to the getaway: it neither flies nor shows its floor.</summary>
    public static CeresGetawayFlight NotBegun { get; } = new Waiting();

    /// <summary>The flight as the getaway begins: its first step is the one its first update flies.</summary>
    internal static CeresGetawayFlight Begun => new Flying(ListStep<CeresRidleyGetawayStep>.FirstOf(CeresRidleyGetawayCurve.Steps));

    /// <summary>Ridley has not handed the room to the getaway.</summary>
    public sealed partial record Waiting : CeresGetawayFlight;

    /// <summary>The getaway flies its curves.</summary>
    /// <param name="Step">The step the next getaway update flies, its place the update of the getaway.</param>
    public sealed partial record Flying(ListStep<CeresRidleyGetawayStep> Step) : CeresGetawayFlight
    {
        /// <summary>The flight once <see cref="Step"/> is flown: the step after it, or flown once it was the last.</summary>
        internal CeresGetawayFlight Following => Step.Following is { } next ? new Flying(next) : new Flown();
    }

    /// <summary>Every step of the curves is flown: the update after the last ends the getaway.</summary>
    public sealed partial record Flown : CeresGetawayFlight;
}
