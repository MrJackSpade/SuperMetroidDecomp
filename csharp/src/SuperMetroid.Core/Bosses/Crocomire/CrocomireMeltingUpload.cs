namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One eight-byte scratch-to-VRAM transfer authored in the same table.</summary>
internal readonly record struct CrocomireMeltingUpload(
    ushort ByteCount, ushort DestinationWord, byte SourceBank, ushort SourceWord);
