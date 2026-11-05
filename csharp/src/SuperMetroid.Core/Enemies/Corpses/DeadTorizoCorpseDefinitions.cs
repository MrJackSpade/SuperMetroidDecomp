namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>How the dead Torizo's corpse rots.</summary>
internal static class DeadTorizoCorpseDefinitions
{
    /// <summary>The corpse's height in pixel rows, each of which its rot schedule drops.</summary>
    internal const int RowCount = 96;

    /// <summary>The first body row of the corpse's odd half, which the game shows apart from the even half above it.</summary>
    internal const int OddHalfFirstRow = 48;

    /// <summary>The pixel rows of the sand heap the corpse's hook shows: two strips, the odd half's above the even half's.</summary>
    internal const int SandRowCount = 16;

    /// <summary>The rows of the sand's first strip, which the odd half shows.</summary>
    internal const int SandStripRows = 8;

    /// <summary>The lines of sand that fall, one row each, from the heap's last row up.</summary>
    internal const int SandLines = 15;

    /// <summary>The rot calls between one line of sand falling and the next.</summary>
    internal const int RotCallsPerSandLine = 15;
}
