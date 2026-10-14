namespace SuperMetroid.Core.Assets;

/// <summary>Identifies one editable ending-explosion composition and its stock cartridge shape.</summary>
/// <param name="Name">Stable JSON key for the frame.</param>
/// <param name="Pointer">Bank-$8C pointer used by the cinematic instruction lists.</param>
/// <param name="StockPartCount">Number of OAM parts in the cartridge composition.</param>
public readonly record struct EndingExplosionSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);
