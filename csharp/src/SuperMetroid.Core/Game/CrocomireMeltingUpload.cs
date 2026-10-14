namespace SuperMetroid.Core.Game;

/// <summary>One eight-byte scratch-to-VRAM transfer authored in the same table.</summary>
internal readonly record struct CrocomireMeltingUpload(
    ushort ByteCount, ushort DestinationWord, byte SourceBank, ushort SourceWord);
