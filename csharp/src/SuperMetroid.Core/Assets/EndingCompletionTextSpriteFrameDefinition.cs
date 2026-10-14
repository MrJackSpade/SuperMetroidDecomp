namespace SuperMetroid.Core.Assets;

/// <summary>One named native completion-text composition identity and its original OAM part geometry; the part count describes stock artwork rather than imposing a timing or edit restriction.</summary>
/// <param name="Name">Stable JSON key identifying a phrase prefix, decimal digit, or colon composition.</param>
/// <param name="Pointer">Bank-relative $8C spritemap pointer selected by the fixed ending instruction lists.</param>
/// <param name="StockPartCount">Original ordered OAM part count: two per revealed letter, or two for a digit or colon.</param>
public readonly record struct EndingCompletionTextSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);
