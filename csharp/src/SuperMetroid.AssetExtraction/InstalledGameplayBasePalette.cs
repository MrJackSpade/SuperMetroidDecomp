using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The installed gameplay base palette as the import pipeline reads it (#1502): beside the starting and common sprite
/// colours the game loads, the identity of the selected colours.
/// </summary>
public sealed class InstalledGameplayBasePalette : GameplayBasePalette
{
    /// <summary>Retains the decoded starting colours, the common sprite colours and their identity.</summary>
    /// <param name="starting">The starting colours, by owner.</param>
    /// <param name="commonSprites">The sixteen common sprite colours.</param>
    /// <param name="contentIdentity">The identity of the selected colours, which the import pipeline derives from the document.</param>
    internal InstalledGameplayBasePalette(GameplayStartingColours starting, RgbColor[] commonSprites, string contentIdentity)
        : base(starting, commonSprites) => ContentIdentity = Ensure.NotNull(contentIdentity);

    /// <summary>Identity of the decoded selected colors, independent of JSON encoding.</summary>
    public string ContentIdentity { get; }
}
