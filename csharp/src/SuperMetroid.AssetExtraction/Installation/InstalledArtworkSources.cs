using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// The installed artwork only the import reads, which the installed content's identity covers (#1616): the game draws the
/// sheets the import composes from it and never binds the artwork itself.
/// </summary>
public sealed record InstalledArtworkSources
{
    /// <summary>The opening cinematic's and the Ceres scenes' maps and sprite compositions.</summary>
    public required IntroCinematicSourceArtwork Intro { get; init; }

    /// <summary>The ending's object maps and sprite compositions.</summary>
    public required EndingObjectSourceArtwork EndingObjects { get; init; }
}
