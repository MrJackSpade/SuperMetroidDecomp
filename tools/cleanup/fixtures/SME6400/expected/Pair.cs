using System.Globalization;

namespace Fixture;

/// <summary>A pair of values of one kind.</summary>
/// <typeparam name="T">The kind of value.</typeparam>
public sealed class Pair<T>
{
    /// <summary>The first value.</summary>
    public T? First { get; set; }
}
