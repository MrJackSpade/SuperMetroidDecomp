using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The control words of a game-over text stream; every other word is a tilemap cell.</summary>
internal enum GameOverTextControl : ushort
{
    /// <summary>The stream ends.</summary>
    End = GameOverRomData.TextEnd,

    /// <summary>The following cells start the next line.</summary>
    NextLine = GameOverRomData.TextNextLine,
}
