namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Verification's view of where the cartridge keeps Zoa's launch speeds.</summary>
internal static class ZoaSpeedSources
{
    /// <summary>$A3:B415, ZoaXSpeedTable: whole/fraction word records, one per speed stage, then a trailing zero record.</summary>
    public const int SpeedTable = 0xa3b415;

    /// <summary>The bytes of one whole/fraction record; a stage's record starts at its stage times this.</summary>
    public const int RecordBytes = 4;
}
