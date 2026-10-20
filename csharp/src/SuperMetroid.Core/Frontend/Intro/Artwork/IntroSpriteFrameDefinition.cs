namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Native spritemap identity, editable asset name, and validated OAM part count of an opening sprite frame.</summary>
/// <param name="Pointer">The native 16-bit spritemap pointer, used as the runtime frame identity.</param>
/// <param name="Name">The stable asset name of the visual composition.</param>
/// <param name="StockPartCount">Retail composition's OAM part count for source comparison.</param>
internal readonly record struct IntroSpriteFrameDefinition(ushort Pointer, string Name, int StockPartCount);
