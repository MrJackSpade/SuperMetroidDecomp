namespace SuperMetroid.Core.Game;

/// <summary>Cartridge transfer lists used during the Mother Brain encounter.</summary>
public static class MotherBrainTileTransferData
{
    /// <summary>$A9:8FE5, four size/source/destination records loading Baby graphics +$400 onward.</summary>
    public const int BabyTileList = 0xa98fe5;

    /// <summary>Four nonzero records precede the Baby transfer list terminator.</summary>
    public const int BabyTileCount = 4;

    /// <summary>Each native record contains a word size, long source, and word VRAM destination.</summary>
    public const int RecordSize = 7;
}
