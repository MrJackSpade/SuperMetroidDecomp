using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Hardware BRR directory and block-header layout, including retired voice bookkeeping.</summary>
internal static partial class DspBrrLayout
{
    /// <summary>DIR entries contain two little-endian pointers: start, then loop.</summary>
    internal const int DirectoryEntryBytes = 4;
    /// <summary>Byte offset of the loop pointer within a DIR entry.</summary>
    internal const int LoopPointerOffset = 2;
    /// <summary>One header followed by eight bytes of packed samples.</summary>
    internal const int BlockBytes = 9;
    /// <summary>The low two header bits contain END and LOOP.</summary>
    private const int HeaderFlagsMask = 3;

    /// <summary>How the block whose header byte is <paramref name="header"/> ends its sample (#2563).</summary>
    internal static BrrBlockEnd EndOf(int header) => (BrrBlockEnd)(header & HeaderFlagsMask);
}
