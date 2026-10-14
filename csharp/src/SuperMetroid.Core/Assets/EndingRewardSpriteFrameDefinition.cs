namespace SuperMetroid.Core.Assets;

/// <summary>Identifies one published reward frame and its native OAM record.</summary>
/// <param name="Name">Stable JSON frame name.</param>
/// <param name="Pointer">Bank-$8C native OAM record pointer.</param>
/// <param name="StockPartCount">Number of OBJ parts in the stock composition.</param>
public readonly record struct EndingRewardSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);
