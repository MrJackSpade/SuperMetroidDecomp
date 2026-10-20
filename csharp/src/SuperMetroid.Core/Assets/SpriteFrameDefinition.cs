namespace SuperMetroid.Core.Assets;

/// <summary>One visual spritemap of a cinematic or ending sequence and its expected retail OAM entry count.</summary>
/// <param name="Name">Stable asset name identifying the visual composition.</param>
/// <param name="Pointer">Native 16-bit spritemap pointer, used as the runtime frame identity.</param>
/// <param name="StockPartCount">Retail composition's OAM part count for source comparison, not a required count for independently edited artwork.</param>
public readonly record struct SpriteFrameDefinition(string Name, ushort Pointer, int StockPartCount);
