namespace SuperMetroid.Core.Hardware;

/// <summary>Native layout of the Mode 7 transfer queue that <c>QueueMode7Transfers</c> ($80:8B4F) fills.</summary>
internal static class Mode7TransferQueueLayout
{
    /// <summary>WRAM <c>$7E:02D0</c>, <c>Mode7Transfer</c>: the first queued descriptor's control byte.</summary>
    internal const int QueueStart = 0x7e02d0;

    /// <summary>The queue's 96 bytes, <c>$02D0-$032F</c>.</summary>
    internal const int QueueByteCount = 0x60;

    /// <summary>A queued VRAM descriptor: control, long source, size, destination and increment mode.</summary>
    internal const int VramDescriptorByteCount = 9;

    /// <summary>Control byte <c>$80</c>: write the descriptor's bytes to the VRAM tilemap (low) port.</summary>
    internal const byte VramTilemapControl = 0x80;

    /// <summary>Bank <c>$A6</c>, which holds the Ceres Mode 7 tilemap sources.</summary>
    internal const byte CeresSourceBank = 0xa6;
}
