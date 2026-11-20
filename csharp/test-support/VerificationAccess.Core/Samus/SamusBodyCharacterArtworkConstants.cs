using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Helpers of <see cref="SamusBodyCharacterArtwork"/> that only verification reads.</summary>
internal static class SamusBodyCharacterArtworkConstants
{
    /// <summary>SHA-256 of <paramref name="artwork"/>'s half pictures' names, transfer sizes and characters, in name order (#1636: import data).</summary>
    public static string CharactersContentIdentityOf(SamusBodyCharacterArtwork artwork)
    {
        Ensure.NotNull(artwork);
        return SelectedPresentationHash.Create("samus-body-characters-v2", content =>
        {
            foreach ((SamusBodyPicture picture, SamusBodyTileTransfer definition) in artwork.pictures.OrderBy(pair => pair.Key.Name, StringComparer.Ordinal))
            {
                content.AppendName("picture", picture.Name);
                content.Append("first transfer size", definition.FirstSize);
                content.Append("second transfer size", definition.SecondSize);
                content.Append("characters", definition.Planar.Span);
            }
        });
    }
}
