using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The cartridge pose record's no-input fallback byte a typed fallback stands for, for checks that compare the typed
/// fallbacks with the records (#2050). The records spell keeping the current pose as the forward still pose's number.
/// </summary>
internal static class SamusNoInputFallbackNumbers
{
    /// <summary>The record byte <paramref name="fallback"/> stands for.</summary>
    internal static byte Number(SamusNoInputFallback fallback) =>
        SamusPoseNumbers.Number(fallback.TryGetPose(out SamusPoseId pose) ? pose : SamusPoseId.ForwardStillCrouchHeightPose);
}
