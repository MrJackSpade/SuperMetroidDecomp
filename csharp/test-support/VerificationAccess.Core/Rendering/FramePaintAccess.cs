using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="FramePaint"/>: OAM fixtures and the parts a frame shows.</summary>
internal static class FramePaintAccess
{
    /// <summary>Builds snapshots from native OAM fixture tables.</summary>
    extension(FramePaint)
    {
        /// <summary>
        /// A snapshot whose sprites are an OAM fixture's records, converted as a frontend capture converts them
        /// (the extraction pipeline's ObjCharacterImage), so the records reach both backends only as a frame description.
        /// </summary>
        internal static FramePaint FromOam(ReadOnlySpan<byte> vram, ReadOnlySpan<RgbColor> cgram,
            ReadOnlySpan<byte> oamUpload, int spriteCount, int objectSelection)
        {
            var oam = new OamBuffer();
            oam.LoadUploadPayload(oamUpload, spriteCount);
            var image = new SnesVram();
            image.LoadBytes(0, vram);
            return new(cgram, ObjCharacterImageConstants.Draws(oam, image, objectSelection));
        }
    }

    /// <summary>Reads what a captured frame description shows.</summary>
    extension(FramePaint memory)
    {
        /// <summary>The colour each ink shows, in ink order, for fixtures that compare or rebuild a frame's colours.</summary>
        internal ReadOnlySpan<RgbColor> Colors => memory.Backgrounds.AllColours;

        /// <summary>Every sprite part the frame shows, frontmost first.</summary>
        internal PlacedSpritePart[] SpriteParts() => PlacedSpritePart.Of(memory.SpriteDraws);
    }
}
