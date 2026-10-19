namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One eight-byte scratch-to-VRAM transfer authored in the same table.</summary>
/// <param name="ByteCount">Bytes the upload sends.</param>
/// <param name="DestinationWord">VRAM word the upload starts at.</param>
/// <param name="SourceBank">Bank of the scratch chunk the upload reads.</param>
/// <param name="SourceWord">Scratch address the upload reads from.</param>
internal readonly record struct CrocomireMeltingUpload(
    int ByteCount, int DestinationWord, int SourceBank, int SourceWord);
