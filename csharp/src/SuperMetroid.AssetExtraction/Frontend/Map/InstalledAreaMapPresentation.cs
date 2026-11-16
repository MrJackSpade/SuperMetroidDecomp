using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map presentation (#1466, #1544, #1616, #1636, #1693): the catalog the game binds (<see cref="Game"/>), read
/// from the documents the game binds alone. The characters, the colours only the import pre-colours sheets from and the
/// documents only the import pictures are import data the installation never holds, which the import reads with
/// <see cref="ImportAreaMapPresentation"/>.
/// </summary>
public class InstalledAreaMapPresentation
{
    /// <summary>Takes the catalog the game binds.</summary>
    internal InstalledAreaMapPresentation(InstalledScreenContent game) => Game = Ensure.NotNull(game);

    /// <summary>The catalog the game binds: only what the game reads of the installed presentation (#1616).</summary>
    public InstalledScreenContent Game { get; }
}
