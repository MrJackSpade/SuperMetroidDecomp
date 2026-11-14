using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Areas for checks that sweep the cartridge's area numbers: each number the cartridge gives an area decodes through the
/// import's codec, and every other number stands for an area the game does not have.
/// </summary>
internal static class RetailAreaNumbers
{
    /// <summary>A value no area holds, standing for a forged or out-of-range area.</summary>
    internal static AreaId UnknownArea => (AreaId)(-1);

    /// <summary>The area the cartridge numbers <paramref name="number"/>, or <see cref="UnknownArea"/> for a number it gives no area.</summary>
    internal static AreaId RetailArea(int number) =>
        (uint)number < (uint)AreaNumberCodec.Count ? AreaNumberCodec.At(number) : UnknownArea;
}
