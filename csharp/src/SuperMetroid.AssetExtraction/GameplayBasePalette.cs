using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The gameplay base palette (#1464, #1502, #1512): the starting colours and the common sprite colours a room load
/// restores. Only the import pipeline reads it, from its import data (#1637), to pre-colour the sheets that show these
/// colours; the runtime never holds it (#1544) and the installation never holds it.
/// </summary>
public sealed class GameplayBasePalette
{
    /// <summary>The sixteen common sprite colours.</summary>
    private readonly RgbColor[] commonSprites;

    /// <summary>Retains the decoded starting colours and the common sprite colours.</summary>
    /// <param name="starting">The starting colours, by owner.</param>
    /// <param name="commonSprites">The sixteen common sprite colours.</param>
    internal GameplayBasePalette(GameplayStartingColours starting, RgbColor[] commonSprites)
    {
        Starting = Ensure.NotNull(starting);
        this.commonSprites = Ensure.NotNull(commonSprites);
    }

    /// <summary>The starting colours, by owner.</summary>
    public GameplayStartingColours Starting { get; }

    /// <summary>The sixteen common sprite colours a room load gives the common sprites.</summary>
    public ReadOnlySpan<RgbColor> CommonSprites => commonSprites;
}
