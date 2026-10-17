namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One fourteen-colour write from `$AD:E90C-$E998` to sprite palette seven.</summary>
public readonly record struct BabyMetroidPaletteTransferRequest(
    int PaletteIndex,
    uint SourceAddress,
    int DestinationColorIndex,
    int ColorCount);
