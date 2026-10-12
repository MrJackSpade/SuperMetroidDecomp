using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>One library-background command-2 VRAM DMA: its native source, VRAM word destination and byte count (#633).</summary>
internal readonly record struct LibraryBackgroundTransfer
{
    /// <summary>Source address.</summary>
    public required int SourceAddress { get; init; }

    /// <summary>Destination word.</summary>
    public required ushort DestinationWord { get; init; }

    /// <summary>Byte count.</summary>
    public required ushort ByteCount { get; init; }
}
