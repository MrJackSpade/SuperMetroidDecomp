using System.Globalization;

namespace Fixture;

/// <summary>A shape with a number of sides.</summary>
public sealed class Shape
{
    /// <summary>The number of sides.</summary>
    public int Sides { get; init; }

    /// <summary>The number of sides as invariant text.</summary>
    public string Text => Sides.ToString(CultureInfo.InvariantCulture);
}

/// <summary>A pair of values of one kind.</summary>
/// <typeparam name="T">The kind of value.</typeparam>
public sealed class Pair<T>
{
    /// <summary>The first value.</summary>
    public T? First { get; set; }
}

/// <summary>The corners a shape can have.</summary>
public enum Corner
{
    /// <summary>A square corner.</summary>
    Square,
}
