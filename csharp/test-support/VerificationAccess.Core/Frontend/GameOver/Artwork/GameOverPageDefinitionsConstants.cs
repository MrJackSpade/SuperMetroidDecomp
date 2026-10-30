namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>Cartridge values and helpers of <see cref="GameOverPageDefinitions"/> that only verification reads.</summary>
internal static class GameOverPageDefinitionsConstants
{
    /// <summary>$81:9304 Tilemap_GameOver_findTheMetroidLarva, the native one-row objective.</summary>
    internal const string ObjectiveText = "FIND THE METROID LARVA!";
    /// <summary>$81:9334 Tilemap_GameOver_tryAgain includes a space before its question mark.</summary>
    internal const string PromptText = "TRY AGAIN ?";
    /// <summary>$81:937C-939C small-font continuation below the YES answer.</summary>
    internal const string ReturnToGameText = " (RETURN TO GAME)";
    /// <summary>$81:93CA-93E4 small-font continuation below the N O answer.</summary>
    internal const string ReturnToTitleText = " (GO TO TITLE)";
    /// <summary>$81:9304-93E4 small uppercase menu atlas: A starts at $6A and letters are consecutive.</summary>
    internal const int SmallLetterA = 0x6a;
    /// <summary>$81:9330, Tilemap_GameOver_findTheMetroidLarva punctuation.</summary>
    internal const int ExclamationTile = 0x84;
    /// <summary>$81:9348, Tilemap_GameOver_tryAgain punctuation.</summary>
    internal const int QuestionTile = 0x85;
    /// <summary>$81:937E/$939C, answer explanation opening/closing glyphs occupy adjacent atlas cells.</summary>
    internal const int OpeningParenthesisTile = 0x8a;
    /// <summary>$81:92DC/92F0 and $934C/9376/$93A0/93C4 select large-font halves.
    /// A/E/M/N/S use the atlas row stride; G/R/V/Y reuse distinct halves; O uses the round zero glyph.</summary>
    internal static readonly IReadOnlyDictionary<char, GameOverLargeGlyph> LargeLetters = new Dictionary<char, GameOverLargeGlyph>
    {
        [' '] = new(GameOverPageDefinitions.BlankTile, GameOverPageDefinitions.BlankTile),
        ['A'] = new(0x0a, 0x0a + 16),
        ['E'] = new(0x0e, 0x0e + 16),
        ['M'] = new(0x26, 0x26 + 16),
        ['N'] = new(0x27, 0x27 + 16),
        ['S'] = new(0x2b, 0x2b + 16),
        ['O'] = new(0, 16),
        ['G'] = new(0x0c, 0x30),
        ['R'] = new(0x0d, 0x3a),
        ['V'] = new(0x2d, 0x3e),
        ['Y'] = new(0x41, 0x17),
    };
}
