namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>One bank-$8C visual spritemap and its expected retail OAM entry count.</summary>
/// <param name="Name">Stable asset name identifying the visual composition.</param>
/// <param name="Pointer">Native 16-bit spritemap pointer in bank $8C, used as the runtime frame identity.</param>
/// <param name="StockPartCount">Retail composition's OAM part count for source comparison, not a required count for independently edited artwork.</param>
public readonly record struct CeresFlightSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);
