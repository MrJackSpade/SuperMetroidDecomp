namespace SuperMetroid.Core.Assets;

/// <summary>Native pointer, installed key, and stock OAM part count for an intro scientist pose.</summary>
internal readonly record struct IntroScientistSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
