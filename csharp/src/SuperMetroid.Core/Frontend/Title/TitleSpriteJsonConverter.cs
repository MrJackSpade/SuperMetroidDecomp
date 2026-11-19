namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Reads and writes a <see cref="TitleSprite"/> in an installed document by its installed key (#2520).</summary>
internal sealed class TitleSpriteJsonConverter() : InstalledKeyJsonConverter<TitleSprite>(TitleSpriteKeys.Installed);
