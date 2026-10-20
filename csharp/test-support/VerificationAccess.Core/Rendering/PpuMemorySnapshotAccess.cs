using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="PpuMemorySnapshot"/>: OAM fixtures and the parts a frame shows.</summary>
internal static class PpuMemorySnapshotAccess
{
    /// <summary>Builds snapshots from native OAM fixture tables.</summary>
    extension(PpuMemorySnapshot)
    {
        /// <summary>
        /// A snapshot whose sprites are an OAM fixture's records, converted as a frontend capture converts them
        /// (<see cref="FrontendOamDraws"/>), so the records reach both backends only as a frame description.
        /// </summary>
        [PpuLayoutBoundary("exact hardware test: builds frames from native OAM fixture tables")]
        internal static PpuMemorySnapshot FromOam(ReadOnlySpan<byte> vram, ReadOnlySpan<Bgr555> cgram,
            ReadOnlySpan<byte> oamUpload, int spriteCount, int objectSelection)
        {
            var oam = new OamBuffer();
            oam.LoadUploadPayload(oamUpload, spriteCount);
            var image = new SnesVram();
            image.LoadBytes(0, vram);
            return new(vram, cgram, FrontendOamDrawsConstants.Draws(oam, image, objectSelection));
        }
    }

    /// <summary>Reads what a captured frame description shows.</summary>
    extension(PpuMemorySnapshot memory)
    {
        /// <summary>Every sprite part the frame shows, frontmost first.</summary>
        internal PlacedSpritePart[] SpriteParts() => PlacedSpritePart.Of(memory.SpriteDraws);
    }
}
