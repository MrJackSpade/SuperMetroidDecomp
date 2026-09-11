namespace SuperMetroid.Core.Audio;

/// <summary>Hardware BRR directory and block-header layout, including retired voice bookkeeping.</summary>
internal static class DspBrrLayout
{
    /// <summary>DIR entries contain two little-endian pointers: start, then loop.</summary>
    internal const int DirectoryEntryBytes = 4;
    /// <summary>Byte offset of the loop pointer within a DIR entry.</summary>
    internal const int LoopPointerOffset = 2;
    /// <summary>One header followed by eight bytes of packed samples.</summary>
    internal const int BlockBytes = 9;
    /// <summary>Header bit zero marks the end of a BRR sequence.</summary>
    internal const byte EndFlag = 1;
    /// <summary>The low two header bits contain END and LOOP.</summary>
    internal const byte HeaderFlagsMask = 3;
}
