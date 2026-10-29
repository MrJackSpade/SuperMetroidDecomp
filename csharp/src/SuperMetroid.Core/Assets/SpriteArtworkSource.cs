namespace SuperMetroid.Core.Assets;

/// <summary>
/// A sprite look gameplay shows (#1392, #1406, #1422, #1463): a named piece of installed artwork. Domains add their own
/// named looks. A look is an identity only; saved state carries it, and the import pipeline, which composes the
/// pre-coloured pictures, resolves its content and where it lands.
/// </summary>
public abstract record SpriteArtworkSource;
