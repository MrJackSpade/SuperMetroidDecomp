namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Reference oracle of the work memory the game stages rotting corpses in (#1412, #1531): the room's enemy BG2 buffer and
/// the corpse-rotting heap, which only the character rot oracle and its checks read.
/// </summary>
internal static class CorpseWorkMemory
{
    /// <summary>$7E:2000: the work-memory enemy BG2 staging image, whose corpse rows a rot upload may read (#1412).</summary>
    internal const int EnemyBg2Staging = 0x7e2000;

    /// <summary>The bytes of the work-memory enemy BG2 staging image.</summary>
    internal const int EnemyBg2StagingBytes = 0x1000;

    /// <summary>$7E:9000: the work-memory corpse-rotting heap (#1412).</summary>
    internal const int CorpseRottingHeap = 0x7e9000;

    /// <summary>The bytes of the work-memory corpse-rotting heap.</summary>
    internal const int CorpseRottingHeapBytes = 0x800;

    /// <summary>Where in the corpse-rotting heap the dead Torizo's sand strips start.</summary>
    internal const int DeadTorizoSandOffset = 0x500;

    /// <summary>The bytes the dead Torizo's sand strips span.</summary>
    internal const int DeadTorizoSandBytes = 0x240;
}
