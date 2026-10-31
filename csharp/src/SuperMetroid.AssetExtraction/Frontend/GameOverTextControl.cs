using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The control words of a game-over text stream; every other word is a tilemap cell. <see cref="GameOverSourceDefinitions"/>
/// owns the word that names each.
/// </summary>
internal enum GameOverTextControl
{
    /// <summary>The stream ends.</summary>
    End,

    /// <summary>The following cells start the next line.</summary>
    NextLine,
}
