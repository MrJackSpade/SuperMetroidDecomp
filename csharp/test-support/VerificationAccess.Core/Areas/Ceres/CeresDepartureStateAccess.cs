using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresDepartureState"/> members production does not use.</summary>
internal static class CeresDepartureStateAccess
{
    /// <summary>Exposes Ceres departure brightness through its production state instance.</summary>
    extension(CeresDepartureState self)
    {
        /// <summary>Applies the current master-brightness nibble to a software-rendered frame.</summary>
        internal void ApplyBrightness(Span<Rgba32> pixels)
        {
            MasterBrightnessFilter.Apply(pixels, self.Brightness);
        }
    }
}
