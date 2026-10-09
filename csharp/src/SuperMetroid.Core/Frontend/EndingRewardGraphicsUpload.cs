using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Executes the sixteen queued transfers issued by the reward landing actor.</summary>
internal sealed class EndingRewardGraphicsUpload
{
    /// <summary>Snapshot of the installed reward-icon transfer bytes used by each VRAM chunk.</summary>
    private byte[] graphics;

    /// <summary>One past the greatest upload index completed, used to restore queued chunks after artwork rebinding.</summary>
    private int completedChunks;

    /// <summary>Creates an upload owner from the installed reward-icon data and verifies it covers the native transfer range.</summary>
    /// <param name="bus">Non-null address-space context for the active game that owns this ending upload.</param>
    /// <param name="artwork">Installed icon artwork whose transfer bytes are copied for upload.</param>
    public EndingRewardGraphicsUpload(ISnesAddressSpace bus,
        EndingRewardIconArtwork? artwork = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        graphics = (artwork ?? throw new InvalidOperationException(
            "Ending reward requires installed icon artwork.")).Transfer.ToArray();
        if (graphics.Length < EndingRewardGraphicsUploadDefinitions.SourceBytes)
            throw new InvalidDataException("Post-credits icon graphics do not fill the native WRAM upload range.");
    }

    /// <summary>Uploads one actor-selected chunk to VRAM and records progress for later artwork rebinding.</summary>
    /// <param name="vram">Video memory receiving the chunk.</param>
    /// <param name="index">Zero-based reward graphics chunk selected by the native actor.</param>
    public void Upload(SnesVram vram, int index)
    {
        if ((uint)index >= EndingRewardJumpDefinitions.UploadCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        UploadChunk(vram, index);
        completedChunks = Math.Max(completedChunks, index + 1);
    }

    /// <summary>Restores already-queued chunks without advancing the native actor.</summary>
    public void BindArtwork(EndingRewardIconArtwork artwork, SnesVram vram)
    {
        graphics = artwork?.Transfer.ToArray() ?? throw new ArgumentNullException(nameof(artwork));
        for (int index = 0; index < completedChunks; index++)
            UploadChunk(vram, index);
    }

    /// <summary>Copies the indexed interleaved Mode 7 map/character byte range to its native VRAM destination.</summary>
    /// <param name="vram">Video memory receiving the chunk.</param>
    /// <param name="index">Zero-based chunk whose source and destination are defined by the native upload layout.</param>
    private void UploadChunk(SnesVram vram, int index)
    {
        int source = EndingRewardGraphicsUploadDefinitions.SourceWord(index);
        int destination = EndingRewardGraphicsUploadDefinitions.DestinationWord(index);
        // The queued transfer writes both VRAM ports with increment-after-high.
        // These bytes already contain the interleaved Mode-7 map/character data.
        vram.LoadBytes(destination * sizeof(ushort), graphics.AsSpan(
            source - EndingRewardGraphicsUploadDefinitions.SourceBase,
            EndingRewardGraphicsUploadDefinitions.ChunkBytes));
    }
}
