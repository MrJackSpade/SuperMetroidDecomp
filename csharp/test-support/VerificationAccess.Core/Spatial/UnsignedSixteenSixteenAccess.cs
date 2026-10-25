namespace SuperMetroid.Core.Spatial;

/// <summary>Fixture edits of an unsigned 16.16 speed that production code never makes.</summary>
internal static class UnsignedSixteenSixteenAccess
{
    /// <summary>This speed with its fraction word replaced.</summary>
    internal static UnsignedSixteenSixteen WithFraction(this UnsignedSixteenSixteen speed, int fraction) =>
        new(speed.Whole, fraction);
}
