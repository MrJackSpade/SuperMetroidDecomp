namespace SuperMetroid.Core.Hardware;

/// <summary>Resolves immutable compiled artwork at NMI; references, not content blobs, belong in pending debugger state.</summary>
public interface IVramAssetProvider
{
    /// <summary>Resolves one installed artwork identity to its complete compiled transfer bytes when the pending VRAM queue is consumed.</summary>
    /// <param name="asset">Compiled asset identity carried by the queued record; <see cref="VramAssetId.None"/> instead selects the queue's memory-source path and is not an artwork request.</param>
    /// <returns>Read-only selected payload, with its byte extent matching the pending transfer; destination and transfer timing remain queue-owned.</returns>
    ReadOnlyMemory<byte> Resolve(VramAssetId asset);
}
