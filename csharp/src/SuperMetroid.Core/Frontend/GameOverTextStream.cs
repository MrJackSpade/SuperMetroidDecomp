namespace SuperMetroid.Core.Frontend;

/// <summary>One bank-$81 text command stream and its BG1 byte destination.</summary>
/// <param name="DestinationByteOffset">BG1-page-relative byte offset of the initial tile; row and column are encoded as 64 times Y plus twice X.</param>
/// <param name="SourcePointer">Sixteen-bit bank-$81 source pointer to tile words and the $FFFE/$FFFF text commands.</param>
/// <param name="Description">Human-readable text-element identity for extraction diagnostics, not the encoded tile-word payload.</param>
public readonly record struct GameOverTextStream(
    int DestinationByteOffset,
    ushort SourcePointer,
    string Description);
