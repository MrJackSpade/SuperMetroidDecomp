namespace SuperMetroid.Core.Assets;

/// <summary>Pairs an intro explosion's bank-$8C OAM pointer and artwork key with its original part count.</summary>
internal readonly record struct IntroMotherBrainExplosionSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
