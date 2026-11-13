namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The counter cadences and phases of both Ridley fights (#1606): Norfair Ridley's thinking, his breakup's flicker,
/// Ceres Ridley's hurt flash length and the getaway's capsule, wings and self-destruct alarm.
/// </summary>
internal static class RidleyCadenceDefinitions
{
    /// <summary>
    /// The updates on which Norfair Ridley runs his fight: every second update of his body's own update count, on its
    /// even place. His intangibility, combat preparation and current phase all wait out the odd updates.
    /// </summary>
    internal static readonly UpdateCadence NorfairRidleyActsEverySecondUpdate = new(2, 0);

    /// <summary>
    /// The phase of a Norfair Ridley breakup fragment's own update count on which it is hidden: set on odd updates, so it
    /// flickers. The fragment keeps only this phase of its count, which then alternates on its own.
    /// </summary>
    internal static readonly UpdateCounterField BreakupFragmentHidden = new(0, 1);

    /// <summary>
    /// The phase of Ceres Ridley's hurt flash timer that a new hit keeps: a hit during a flash on its odd counts restarts
    /// it one update longer, so his flash and the escape that follows keep their place in the alternation.
    /// </summary>
    internal static readonly UpdateCounterField CeresHurtFlashOddCount = new(0, 1);

    /// <summary>
    /// The updates on which the Baby's capsule in Ceres Ridley's getaway takes its next picture: every fourth update of
    /// the frame counter, on its first place.
    /// </summary>
    internal static readonly UpdateCadence GetawayCapsuleStepEveryFourthUpdate = new(4, 0);

    /// <summary>
    /// The updates on which Ceres Ridley's wings in the getaway take their next picture: every eighth update of the frame
    /// counter, on its first place. Each wing picture's floor update leaves the delays Samus's Speed Booster runs on.
    /// </summary>
    internal static readonly UpdateCadence GetawayWingStepEveryEighthUpdate = new(8, 0);

    /// <summary>
    /// The updates on which the self-destruct warning's EMERGENCY title takes its next alarm colour: every fourth update
    /// of the frame counter, on its first place.
    /// </summary>
    internal static readonly UpdateCadence SelfDestructAlarmStepEveryFourthUpdate = new(4, 0);
}
