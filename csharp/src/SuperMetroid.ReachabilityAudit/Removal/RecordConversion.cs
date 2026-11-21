namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// A positional record whose every generated property is dead while some of its parameters are still read by its base
/// call: it becomes a record without a parameter list whose constructor takes the parameters still read and passes
/// them to its base (#2599).
/// </summary>
/// <param name="Record">The record's name.</param>
/// <param name="Parameters">The names of all its positional parameters, in order, which find it in its file.</param>
/// <param name="Kept">The names of the parameters its base call reads, which the constructor keeps.</param>
internal sealed record RecordConversion(string Record, IReadOnlyList<string> Parameters, IReadOnlySet<string> Kept);
