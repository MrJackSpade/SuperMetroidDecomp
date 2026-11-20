namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="FloorZoom"/> that only verification reads.</summary>
internal static class FloorZoomConstants
{
    /// <summary>A zoom of <paramref name="twoHundredFiftySixths"/> 256ths of a floor pixel per screen pixel, zero or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom is negative.</exception>
    public static FloorZoom FromTwoHundredFiftySixths(int twoHundredFiftySixths) => global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Spatial.FloorZoom>([typeof(int)], new object?[] { twoHundredFiftySixths });
}
