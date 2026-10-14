namespace SuperMetroid.Core.Game;

/// <summary>One contiguous run of native collision records starting at <paramref name="Start"/>.</summary>
/// <param name="Start">Address of the run's first record.</param>
/// <param name="Records">Each record's elements in native order.</param>
internal readonly record struct CollisionRecordRun<TElement>(ushort Start, TElement[][] Records);
