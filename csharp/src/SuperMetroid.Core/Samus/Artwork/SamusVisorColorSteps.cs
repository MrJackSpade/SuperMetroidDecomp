using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Artwork.SamusVisorColorDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The visor colour of each step of the visor cycle.</summary>
internal static class SamusVisorColorSteps
{
    /// <summary>Interpolates the widening visor into white, then darkens the selected full-beam ink by five RGB5 levels per phase.</summary>
    internal static RgbColor Color(int index)
    {
        if ((uint)index >= SamusVisorColorFormat.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return WideningStart.Zip(FullBeamStart, (_, start, fullBeam) =>
        {
            if (index >= WideningPhaseCount)
                return fullBeam - (index - WideningPhaseCount) * DarkeningStep;
            int intervals = WideningPhaseCount - 1;
            return (start * (intervals - index) + 31 * index + intervals / 2) / intervals;
        });
    }
}
