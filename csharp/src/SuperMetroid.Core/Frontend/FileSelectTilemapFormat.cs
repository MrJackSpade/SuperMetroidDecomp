namespace SuperMetroid.Core.Frontend;

/// <summary>Control words in the bank-$81 file-select tilemap stream format.</summary>
internal static class FileSelectTilemapFormat
{
    /// <summary>Bank containing the file-select text streams.</summary>
    public const int Bank = FileSelectMapRomData.TilemapTextBank;
    /// <summary>Terminates one tilemap text stream.</summary>
    public const ushort End = 0xffff;
    /// <summary>Moves the output cursor to the same column on the following row.</summary>
    public const ushort NextRow = 0xfffe;
}
