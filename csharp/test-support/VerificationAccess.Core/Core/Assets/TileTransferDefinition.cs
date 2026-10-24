namespace SuperMetroid.Core.Assets;

/// <summary>One tile transfer: how many bytes it copies, which artwork source it reads and where it lands.</summary>
/// <param name="ByteCount">The bytes the transfer copies.</param>
/// <param name="SourceAddress">The artwork source the transfer reads, which selects the installed artwork.</param>
/// <param name="DestinationWord">Where the transferred tiles land.</param>
internal readonly record struct TileTransferDefinition(ushort ByteCount, int SourceAddress, ushort DestinationWord);
