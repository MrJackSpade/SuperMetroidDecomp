using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Decodes a cartridge-ordered 256-colour image into the owners of gameplay's colours (#1464, #1512): the one place the
/// colour order of the import data's starting image (<see cref="GameplayBasePaletteFiles"/>) is read. Only the import reads
/// it, to pre-colour the sheets that show those colours; the installation holds none of it (#1637).
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
