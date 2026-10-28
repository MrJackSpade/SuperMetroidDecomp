namespace SuperMetroid.Core.Assets;

/// <summary>
/// What a run of sprite characters shows (#1392, #1406, #1422): a named piece of installed artwork. Domains add their
/// own named sources. A source is an identity only; saved state carries it, and the import pipeline, which composes the
/// pre-coloured pictures, resolves its content.
/// </summary>
public abstract record SpriteArtworkSource;
