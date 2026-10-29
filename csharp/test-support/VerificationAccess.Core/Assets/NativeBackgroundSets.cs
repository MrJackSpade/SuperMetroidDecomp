using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification access: gameplay's background sets at the native boundary (#1464), for checks that read or write a
/// background colour by its position in the cartridge's colour memory. Where each set lies is the import pipeline's
/// <see cref="BackgroundColourSetPositionDefinitions"/>.
/// </summary>
internal static class NativeBackgroundSets
{
    /// <summary>The background set the cartridge holds as its set <paramref name="set"/>, zero through seven.</summary>
    internal static BackgroundColourSet AtNativeSet(int set) =>
        BackgroundColourSetPositionDefinitions.InCartridgeOrder[Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set)];

    /// <summary>The one colour of <paramref name="sets"/> that position <paramref name="position"/> of the cartridge's background colours decodes to.</summary>
    internal static ColourRun At(BackgroundColourSets sets, int position)
    {
        Ensure.NotNull(sets);
        Ensure.BetweenInclusive(0, BackgroundColourSetImage.ColourCount - 1, position);
        return sets.Set(AtNativeSet(position / ColourSets.SetColours)).Slice(position % ColourSets.SetColours, 1);
    }
}
