namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous four-bit character page used by the Ceres escape DMA lists.</summary>
internal readonly record struct CeresEscapeTileSheetDefinition(
    int SourceAddress, int ByteCount, string FileName);
