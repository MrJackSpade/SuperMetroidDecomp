namespace Fixture;

/// <summary>A pair of numbers.</summary>
public sealed class Pair
{
}

#error SME6400: 'Pair<T>' and 'Pair' both belong in Pair.cs; one of them needs another name
/// <summary>A pair of values.</summary>
/// <typeparam name="T">The kind of value.</typeparam>
public sealed class Pair<T>
{
}
