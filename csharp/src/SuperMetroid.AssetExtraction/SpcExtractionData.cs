namespace SuperMetroid.AssetExtraction;

/// <summary>SPC table layouts and BRR bounds used only while extracting sample assets.</summary>
internal static class SpcExtractionData
{
    /// <summary>Maximum entries inspected in the music track-pointer table at SPC $581E.</summary>
    internal const int MaximumTrackPointerCount = 32;
    /// <summary>SPC $6C00 instrument-table extent, before the BRR source directory.</summary>
    internal const int InstrumentTableByteLength = 0x100;
    /// <summary>SPC $6D00: native BRR sample source directory, indexed by DSP SRCN.</summary>
    internal const int BrrDirectoryAddress = 0x6d00;
    /// <summary>Each BRR directory entry contains a two-byte start and two-byte loop pointer.</summary>
    internal const int BrrDirectoryEntrySize = 4;
    /// <summary>One BRR block contains its header and eight packed sample bytes.</summary>
    internal const int BrrBlockByteLength = 9;
    /// <summary>Maximum distinct complete BRR blocks within the SPC address space.</summary>
    internal const int MaximumBrrBlocks = ushort.MaxValue / BrrBlockByteLength;
}
