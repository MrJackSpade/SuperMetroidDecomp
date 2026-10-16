namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>The cartridge's BG tilemap staging buffer in bank-$7E WRAM.</summary>
internal static class LibraryBackgroundStagingDefinitions
{
    /// <summary><c>$7E:4000</c>, the first byte of the buffer.</summary>
    internal const int NativeAddress = 0x7e4000;

    /// <summary>The buffer spans <c>$7E:4000-$7E:4FFF</c>.</summary>
    internal const int ByteCount = 0x1000;
}
