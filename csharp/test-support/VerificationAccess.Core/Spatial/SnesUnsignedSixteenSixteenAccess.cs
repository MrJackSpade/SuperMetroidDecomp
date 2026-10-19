namespace SuperMetroid.Core.Spatial;

/// <summary>Fixture edits of an unsigned 16.16 speed that production code never makes.</summary>
internal static class SnesUnsignedSixteenSixteenAccess
{
    /// <summary>This speed with its fraction word replaced.</summary>
    internal static SnesUnsignedSixteenSixteen WithFraction(this SnesUnsignedSixteenSixteen speed, int fraction) =>
        new(speed.Whole, fraction);
}
