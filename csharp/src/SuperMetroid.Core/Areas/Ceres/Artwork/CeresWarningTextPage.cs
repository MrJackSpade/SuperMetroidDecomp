using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// One page of the Ceres self-destruct's warning text as a look the escape shows over the sprites' artwork (#1406, #1463),
/// in the order the timer-sprite list shows them. Which installed characters it holds and where it lands is the import
/// pipeline's knowledge.
/// </summary>
/// <param name="Page">The page's place among the warning text's pages, from zero.</param>
internal sealed record CeresWarningTextPage(int Page) : SpriteArtworkSource
{
    /// <summary>Names the look for diagnostics and for the pictures of the backgrounds that read it.</summary>
    public override string ToString() => $"Ceres escape warning page {Page}";
}
