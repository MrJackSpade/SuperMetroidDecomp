namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One Samus spritemap composition, named once when the import decodes the body's tables (#2556) by the first pose half
/// frame, explosion frame or atmosphere frame that shows it; the import composes whole body pictures from it (#1585).
/// </summary>
/// <param name="Name">The composition's name.</param>
public readonly partial record struct SamusSpritemapId(string Name);
