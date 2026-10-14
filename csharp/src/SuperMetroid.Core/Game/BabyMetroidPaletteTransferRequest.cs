namespace SuperMetroid.Core.Game;

/// <summary>One fourteen-colour write from `$AD:E90C-$E998` to sprite palette seven.</summary>
public readonly record struct BabyMetroidPaletteTransferRequest(
    ushort PaletteIndex,
    uint SourceAddress,
    ushort DestinationColorIndex,
    ushort ColorCount);
