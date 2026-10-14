namespace SuperMetroid.Core.Game;

/// <summary>One seven-byte transfer record consumed by the Ceres escape graphics dispatcher.</summary>
internal readonly record struct CeresEscapeVramTransferDefinition(
    ushort Pointer, ushort ByteCount, int SourceAddress, ushort DestinationWord);
