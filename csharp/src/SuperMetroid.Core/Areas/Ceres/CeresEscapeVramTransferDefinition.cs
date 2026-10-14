namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One seven-byte transfer record consumed by the Ceres escape graphics dispatcher.</summary>
internal readonly record struct CeresEscapeVramTransferDefinition(
    ushort Pointer, ushort ByteCount, int SourceAddress, ushort DestinationWord);
