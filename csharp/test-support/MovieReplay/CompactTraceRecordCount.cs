namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How many records a compact replay trace holds and which of them precedes the first port update.</summary>
/// <param name="Records">The number of checkpoint records in the trace.</param>
/// <param name="Initial">The record preceding the first port update.</param>
internal readonly record struct CompactTraceRecordCount(int Records, int Initial);
