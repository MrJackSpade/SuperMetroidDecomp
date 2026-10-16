using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Executes the sixteen queued transfers issued by the reward landing actor.</summary>
internal sealed class EndingRewardGraphicsUpload
{
    /// <summary>Installed interleaved Mode-7 map and character bytes for the reward icon.</summary>
    private byte[] graphics;
    /// <summary>Number of queued chunks to replay when artwork is rebound after a restore.</summary>
    private int completedChunks;

    /// <summary>Captures the installed reward-icon transfer and verifies it fills the native upload range.</summary>
    public EndingRewardGraphicsUpload(EndingRewardIconArtwork? artwork = null)
    {
        graphics = (artwork ?? throw new InvalidOperationException(
            "Ending reward requires installed icon artwork.")).Transfer.ToArray();
        if (graphics.Length < EndingRewardGraphicsUploadDefinitions.SourceBytes)
            throw new InvalidDataException("Post-credits icon graphics do not fill the native WRAM upload range.");
    }

    /// <summary>Transfers one indexed reward-icon chunk to VRAM and records upload progress.</summary>
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

    /// <summary>Copies the selected native chunk from the interleaved source bytes to its VMADD destination.</summary>
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
