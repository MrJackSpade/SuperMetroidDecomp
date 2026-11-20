namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The segments of her damaged head's four-ring volley in one of her brain's catalogs (#2554), after its aim.</summary>
/// <typeparam name="TSegment">The catalog's segments.</typeparam>
/// <param name="Opening">The volley's opening, which fires its first ring.</param>
/// <param name="Shot">One more ring of the volley.</param>
/// <param name="End">The volley's end after its last ring.</param>
/// <param name="Neutral">Her idling after the volley.</param>
/// <param name="Hold">Her idle's hold, which on a chance holds again.</param>
internal readonly record struct MotherBrainVolleySegments<TSegment>(TSegment Opening, TSegment Shot, TSegment End, TSegment Neutral, TSegment Hold)
    where TSegment : struct, Enum;
