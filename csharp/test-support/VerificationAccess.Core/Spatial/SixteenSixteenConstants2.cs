namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="SixteenSixteen"/> that only verification reads.</summary>
internal static class SixteenSixteenConstants2
{
    /// <summary>The raw 16.16 value with whole part <paramref name="whole"/> and no fraction.</summary>
    public static int FromWhole(int whole) => whole << SixteenSixteen.FractionBits;
}
