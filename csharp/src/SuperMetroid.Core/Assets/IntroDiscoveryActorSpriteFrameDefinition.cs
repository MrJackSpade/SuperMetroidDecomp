namespace SuperMetroid.Core.Assets;

/// <summary>Native spritemap identity, editable asset name, and validated OAM part count for an opening actor.</summary>
internal readonly record struct IntroDiscoveryActorSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
