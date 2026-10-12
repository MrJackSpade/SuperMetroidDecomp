using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Executes the sixteen queued transfers issued by the reward landing actor.</summary>
internal sealed class EndingRewardGraphicsUpload
{
    /// <summary>Stores graphics.</summary>
    private byte[] graphics;
    /// <summary>Stores completed chunks.</summary>
    private int completedChunks;

    /// <summary>Initializes a new ending reward graphics upload instance.</summary>
    public EndingRewardGraphicsUpload(ISnesAddressSpace bus,
        EndingRewardIconArtwork? artwork = null)
    {
        Ensure.NotNull(bus);
        graphics = (artwork ?? throw new InvalidOperationException(
            "Ending reward requires installed icon artwork.")).Transfer.ToArray();
        if (graphics.Length < EndingRewardGraphicsUploadDefinitions.SourceBytes)
            throw new InvalidDataException("Post-credits icon graphics do not fill the native WRAM upload range.");
    }

    /// <summary>Upload.</summary>
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
        graphics = Ensure.NotNull(artwork).Transfer.ToArray();
        for (int index = 0; index < completedChunks; index++)
            UploadChunk(vram, index);
    }

    /// <summary>Uploads chunk.</summary>
    [PpuLayoutBoundary("codec: reward-icon chunk records' VMADD destinations")]
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
