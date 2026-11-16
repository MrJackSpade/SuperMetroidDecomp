namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="SixteenSixteen"/> that only verification reads.</summary>
internal static class SixteenSixteenConstants
{
    /// <summary>The whole part of a raw 16.16 value.</summary>
    public static int WholeOf(int raw) => raw >> SixteenSixteen.FractionBits;
}
