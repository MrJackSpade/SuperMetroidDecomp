namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="FloorZoomStep"/> that only verification reads.</summary>
internal static class FloorZoomStepConstants
{
    /// <summary>A step of <paramref name="twoHundredFiftySixths"/> 256ths of a floor pixel per screen pixel, at least one.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is zero or negative.</exception>
    public static FloorZoomStep FromTwoHundredFiftySixths(int twoHundredFiftySixths) => global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Spatial.FloorZoomStep>([typeof(int)], new object?[] { (int)(twoHundredFiftySixths) });
}
