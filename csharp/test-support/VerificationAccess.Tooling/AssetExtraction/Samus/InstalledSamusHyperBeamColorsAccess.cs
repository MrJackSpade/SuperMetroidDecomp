using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to <see cref="InstalledSamusHyperBeamColors"/> members production does not use.</summary>
internal static class InstalledSamusHyperBeamColorsAccess
{
    /// <summary>Exposes <see cref="InstalledSamusHyperBeamColors"/> state that only verification reads.</summary>
    extension(InstalledSamusHyperBeamColors self)
    {
        /// <summary>Returns one installed colour; the frame clock remains gameplay's.</summary>
        /// <param name="frame">The cycle frame, 0 through 9.</param>
        /// <param name="color">The colour within the frame, 0 through 15.</param>
        /// <returns>The installed colour.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The frame or colour is outside the cycle.</exception>
        public RgbColor Resolve(int frame, int color)
        {
            if ((uint)frame >= self.frames.Length) throw new ArgumentOutOfRangeException(nameof(frame));
            if ((uint)color >= self.frames[frame].Length) throw new ArgumentOutOfRangeException(nameof(color));
            return self.frames[frame][color];
        }
    }
}
