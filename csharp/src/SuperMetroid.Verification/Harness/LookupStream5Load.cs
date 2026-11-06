namespace SuperMetroid.Verification.Harness;

/// <summary>The placements, read and identity of one lookup stream5 load.</summary>
/// <param name="Placements">The placements.</param>
/// <param name="Read">The read.</param>
/// <param name="Identity">The identity.</param>
internal readonly record struct LookupStream5Load<T>(object? Placements, Func<int, T> Read, string Identity);
