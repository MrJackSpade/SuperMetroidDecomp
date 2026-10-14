namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous, tile-aligned visual source for Torizo $814B uploads.</summary>
internal readonly record struct TorizoInstructionTileSheetDefinition(
    int SourceAddress, int ByteCount, string FileName);
