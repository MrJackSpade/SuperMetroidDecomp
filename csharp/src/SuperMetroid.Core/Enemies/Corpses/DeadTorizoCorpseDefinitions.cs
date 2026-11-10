namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>How the dead Torizo's corpse rots.</summary>
internal static class DeadTorizoCorpseDefinitions
{
    /// <summary>The corpse's height in pixel rows, each of which its rot schedule drops.</summary>
    internal const int RowCount = 96;

    /// <summary>The pixel rows of the corpse's sand heap.</summary>
    internal const int SandRowCount = 16;

    /// <summary>The lines of sand that fall, one row each, from the heap's last row up.</summary>
    internal const int SandLines = 15;

    /// <summary>The rot calls between one line of sand falling and the next.</summary>
    internal const int RotCallsPerSandLine = 15;
}
