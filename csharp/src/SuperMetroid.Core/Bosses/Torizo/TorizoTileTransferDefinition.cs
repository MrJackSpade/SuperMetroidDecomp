namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One Torizo instruction-time tile transfer: how many bytes it copies, which artwork it reads and where among the OBJ characters it lands.</summary>
/// <param name="ByteCount">The bytes the transfer copies.</param>
/// <param name="SourceAddress">The artwork source the transfer reads, which selects the installed artwork.</param>
/// <param name="FirstCharacter">The OBJ character the transferred characters begin at.</param>
internal readonly record struct TorizoTileTransferDefinition(ushort ByteCount, int SourceAddress, int FirstCharacter);
