using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The installed gameplay base palette (#1464, #1502, #1512): the starting colours and the common sprite colours a room
/// load restores, with the identity of the selected colours. Only the import pipeline reads it, to pre-colour the sheets
/// that show these colours; the runtime never holds it.
/// </summary>
public sealed class GameplayBasePalette
{
    /// <summary>The sixteen common sprite colours.</summary>
    private readonly RgbColor[] commonSprites;

    /// <summary>Retains the decoded starting colours, the common sprite colours and their identity.</summary>
    /// <param name="starting">The starting colours, by owner.</param>
    /// <param name="commonSprites">The sixteen common sprite colours.</param>
    /// <param name="contentIdentity">The identity of the selected colours, which the import pipeline derives from the document.</param>
    internal GameplayBasePalette(GameplayStartingColours starting, RgbColor[] commonSprites, string contentIdentity)
    {
        Starting = Ensure.NotNull(starting);
        this.commonSprites = Ensure.NotNull(commonSprites);
        ContentIdentity = Ensure.NotNull(contentIdentity);
    }

    /// <summary>The starting colours, by owner.</summary>
    public GameplayStartingColours Starting { get; }

    /// <summary>The sixteen common sprite colours a room load gives the common sprites.</summary>
    public ReadOnlySpan<RgbColor> CommonSprites => commonSprites;

    /// <summary>Identity of the decoded selected colors, independent of JSON encoding.</summary>
    public string ContentIdentity { get; }
}
