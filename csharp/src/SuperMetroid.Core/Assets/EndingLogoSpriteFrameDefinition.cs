namespace SuperMetroid.Core.Assets;

/// <summary>Identifies one editable final-logo composition and its stock cartridge shape.</summary>
/// <param name="Name">Stable JSON key for the frame.</param>
/// <param name="Pointer">Bank-$8C pointer used by the cinematic script.</param>
/// <param name="StockPartCount">Number of OAM parts in the cartridge composition.</param>
public readonly record struct EndingLogoSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);
