namespace SuperMetroid.Verification.Harness;

/// <summary>The read and identity of one lookup stream5 load.</summary>
/// <param name="Read">The read.</param>
/// <param name="Identity">The identity.</param>
internal readonly record struct LookupStream5Load<T>(Func<int, T> Read, string Identity);
