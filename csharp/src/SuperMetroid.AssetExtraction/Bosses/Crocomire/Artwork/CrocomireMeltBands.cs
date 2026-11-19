using System.Collections.Frozen;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;

/// <summary>
/// The animations the bands of Crocomire's melt images are pictured under (#1392, #1406, #2011): each band is an animation
/// whose one frame shows its image before the dissolve erases any column. Core shows each step of the dissolve by hiding
/// columns of the image (#1652), so no other step is pictured.
/// </summary>
internal static class CrocomireMeltBands
{
    /// <summary>
    /// The installed name of each melt image's undissolved frame in a band's animation (#2511), written out so renaming an
    /// image's identity never changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<CrocomireMeltingIllustration, string> UndissolvedKeys =
        new Dictionary<CrocomireMeltingIllustration, string>
        {
            [CrocomireMeltingIllustration.Flesh] = "Flesh step 0",
            [CrocomireMeltingIllustration.BareSkull] = "BareSkull step 0",
        }.ToFrozenDictionary();

    /// <summary>The animation band <paramref name="band"/> of the melt images is.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="band"/> is not a band of either melt image.</exception>
    internal static ArtworkAnimation Animation(int band) =>
        (uint)band < CrocomireMeltingDefinitions.MeltImageBandCount ? new($"Crocomire melt band {band}")
            : throw new ArgumentOutOfRangeException(nameof(band), band, "Crocomire's melt images have no such band.");

    /// <summary>
    /// The frame of <paramref name="band"/>'s animation that shows melt image <paramref name="illustration"/> before its
    /// dissolve erases a column, named, as installed, for the dissolve's first step.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="band"/> is not a band of either melt image, or
    /// <paramref name="illustration"/> is no melt image.</exception>
    internal static ArtworkFrame Undissolved(CrocomireMeltingIllustration illustration, int band) =>
        new(Animation(band), Ensure.Listed(UndissolvedKeys, illustration));
}
