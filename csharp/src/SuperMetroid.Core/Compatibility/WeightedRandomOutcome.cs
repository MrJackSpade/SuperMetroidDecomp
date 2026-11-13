namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One outcome of a <see cref="WeightedRandomChoice{TOutcome}"/> and its share (#1619): how many consecutive values of the
/// choice's field choose it, following the values the outcomes before it take.
/// </summary>
/// <remarks>
/// Listed in a family's catalog, in the order the original's thresholds rise. Evidence: the distance between each of the
/// original's ascending thresholds and the one before it, which the shares reproduce exactly.
/// </remarks>
/// <typeparam name="TOutcome">What the choice chooses.</typeparam>
/// <param name="Values">How many of the field's values choose the outcome, at least one.</param>
/// <param name="Outcome">The outcome those values choose.</param>
[CompatibilityService("how many of a weighted random choice's field values choose each outcome, as the distance between the original's ascending thresholds")]
public readonly record struct WeightedRandomOutcome<TOutcome>(int Values, TOutcome Outcome);
