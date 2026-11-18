using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The cartridge pose record's no-input fallback byte a typed fallback stands for, for checks that compare the typed
/// fallbacks with the records (#2050). The records spell keeping the current pose as the last byte value, a number past
/// every record that names no pose.
/// </summary>
internal static class SamusNoInputFallbackNumbers
{
    /// <summary>The record byte that keeps the current pose.</summary>
    private const byte KeepCurrentPoseByte = byte.MaxValue;

    /// <summary>The record byte <paramref name="fallback"/> stands for.</summary>
    internal static byte Number(SamusNoInputFallback fallback) =>
        fallback.TryGetPose(out SamusPoseId pose) ? SamusPoseNumbers.Number(pose) : KeepCurrentPoseByte;
}
