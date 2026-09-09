using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Executes the sixteen queued transfers issued by the reward landing actor.</summary>
internal sealed class EndingRewardGraphicsUpload
{
    private readonly ISnesAddressSpace bus;
    private readonly byte[] graphics;

    public EndingRewardGraphicsUpload(ISnesAddressSpace bus)
    {
        this.bus = bus;
        graphics = RomDataReader.Decompress(bus, EndingCreditsRomData.Assets.PostCreditsMode7Characters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        if (graphics.Length < EndingRewardGraphicsUploadDefinitions.SourceBytes)
            throw new InvalidDataException("Post-credits icon graphics do not fill the native WRAM upload range.");
    }

    public void Upload(SnesVram vram, int index)
    {
        if ((uint)index >= EndingRewardJumpDefinitions.UploadCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int source = RomDataReader.ReadWordFixedBank(bus,
            EndingRewardGraphicsUploadDefinitions.SourceTable + index * sizeof(ushort));
        int destination = RomDataReader.ReadWordFixedBank(bus,
            EndingRewardGraphicsUploadDefinitions.DestinationTable + index * sizeof(ushort));
        // The queued transfer writes both VRAM ports with increment-after-high.
        // These bytes already contain the interleaved Mode-7 map/character data.
        vram.LoadBytes(destination * sizeof(ushort), graphics.AsSpan(
            source - EndingRewardGraphicsUploadDefinitions.SourceBase,
            EndingRewardGraphicsUploadDefinitions.ChunkBytes));
    }
}
