namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The control words of a game-over text stream; every other word is a tilemap cell.</summary>
internal enum GameOverTextControl
{
    /// <summary>The stream ends.</summary>
    End = 0xffff,

    /// <summary>The following cells start the next line.</summary>
    NextLine = 0xfffe,
}
