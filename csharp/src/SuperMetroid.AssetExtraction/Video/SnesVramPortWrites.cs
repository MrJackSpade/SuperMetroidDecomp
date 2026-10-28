
namespace SuperMetroid.AssetExtraction.Video;

/// <summary>VRAM writes addressed by the cartridge's encoded destination words, which only extraction and the checks decode.</summary>
internal static class SnesVramPortWrites
{
    /// <summary>Runs the mode-1 VRAM port sequence a queued transfer to <paramref name="encodedDestination"/> runs.</summary>
    internal static void ExecuteQueuedAssetWrite(this SnesVram vram, ReadOnlySpan<byte> bytes, ushort encodedDestination) =>
        vram.ExecuteQueuedAssetWrite(bytes, VramDmaDestination.FromNative(encodedDestination));

    /// <summary>Runs the mode-1 VRAM port sequence a queued transfer to <paramref name="destination"/> runs.</summary>
    internal static void ExecuteQueuedAssetWrite(this SnesVram vram, ReadOnlySpan<byte> bytes, VramDmaDestination destination) =>
        vram.WriteThroughPort(bytes, destination.Word, destination.WordIncrement);
}
