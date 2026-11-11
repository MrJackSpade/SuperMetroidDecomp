
namespace SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

/// <summary>Verification access to <see cref="MapPaletteCycle"/> members production does not use.</summary>
internal static class MapPaletteCycleAccess
{
    /// <summary>Exposes <see cref="MapPaletteCycle"/> state that only verification reads.</summary>
    extension(MapPaletteCycle self)
    {
        /// <summary>Returns the installed hold for one color frame in map-palette animation updates.</summary>
        /// <returns>A hold of one through 254 ticks.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside the selected cycle.</exception>
        public int Duration(int frame)
        {
            self.CheckFrame(frame);
            return self.durations[frame];
        }
    }
}
