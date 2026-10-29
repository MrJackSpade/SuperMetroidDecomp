using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Decodes a cartridge-ordered 256-colour image into the owners of gameplay's colours (#1464): the one place the
/// installed starting image's colour order is read.
/// </summary>
public static class GameplayColourImage
{
    /// <summary>The starting colours, by owner, that <paramref name="image"/>, a whole 256-colour image, holds.</summary>
    /// <exception cref="ArgumentException"><paramref name="image"/> does not hold 256 colours.</exception>
    public static GameplayStartingColours Decode(ReadOnlySpan<RgbColor> image)
    {
        Ensure.LengthEqual(image, SnesCgram.ColorCount);
        return new GameplayStartingColours(
            image.Slice(GameplayColourPositionDefinitions.BackgroundSetsFirst, GameplayColourPositionDefinitions.SetsColours).ToArray(),
            image.Slice(GameplayColourPositionDefinitions.SpriteSetsFirst, GameplayColourPositionDefinitions.SetsColours).ToArray(),
            image.Slice(GameplayColourPositionDefinitions.SamusSuitFirst, GameplayColourPositionDefinitions.SetColours).ToArray(),
            image.Slice(GameplayColourPositionDefinitions.SamusBeamsFirst, GameplayColourPositionDefinitions.SetColours).ToArray(),
            image.Slice(GameplayColourPositionDefinitions.EnemyProjectilesFirst, GameplayColourPositionDefinitions.SetColours).ToArray());
    }
}
