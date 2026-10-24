namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The cartridge's list of the Baby Metroid pages Mother Brain's fight shows ($A9:8FE5), which only verification reads.</summary>
internal static class MotherBrainBabyPageListConstants
{
    /// <summary>$A9:8FE5, four size/source/destination records loading Baby graphics +$400 onward.</summary>
    public const int BabyTileList = 0xa98fe5;
    /// <summary>Each native record contains a word size, long source, and word VRAM destination.</summary>
    public const int RecordSize = 7;
}
