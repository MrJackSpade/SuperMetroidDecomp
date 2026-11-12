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
