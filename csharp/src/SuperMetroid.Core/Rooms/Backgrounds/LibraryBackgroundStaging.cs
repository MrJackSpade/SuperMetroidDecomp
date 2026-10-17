namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The owned buffers a library-background program's WRAM commands use: <paramref name="Tilemap"/>, which decompression
/// and fills write, and <paramref name="EnemySurfaces"/>, the enemy BG2 staging that boss-room programs re-upload from
/// <c>$7E:2000</c>.
/// </summary>
/// <param name="Tilemap">The <c>$7E:4000</c> tilemap staging.</param>
/// <param name="EnemySurfaces">The enemy staging surfaces at <c>$7E:2000</c> and <c>$7E:9000</c>.</param>
public sealed record LibraryBackgroundStaging(Bg2TilemapStaging Tilemap, IWorkRamStagingSource EnemySurfaces)
{
    /// <summary>Uploads a native WRAM-sourced transfer from whichever owned buffer holds its source.</summary>
    [PpuLayoutBoundary("codec: library-background WRAM transfer's native VRAM destination")]
    internal void LoadNativeTransfer(SnesVram vram, int sourceAddress, int byteCount, ushort encodedDestination)
    {
        Ensure.NotNull(vram);
        if (!Tilemap.TryResolve(sourceAddress, byteCount, out VramStagingBuffer buffer, out int offset) &&
            !EnemySurfaces.TryResolve(sourceAddress, byteCount, out buffer, out offset))
            throw new InvalidDataException(
                $"Library-background WRAM transfer ${sourceAddress:X6}+{byteCount} has no owned staging buffer.");
        vram.ExecuteQueuedAssetWrite(buffer.Bytes.Slice(offset, byteCount), encodedDestination);
    }
}
