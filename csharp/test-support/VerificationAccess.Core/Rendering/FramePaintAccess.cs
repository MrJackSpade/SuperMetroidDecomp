using System.Collections.Immutable;
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
            ReadOnlySpan<byte> oamUpload, int spriteCount, int objectSelection) =>
            FramePaint.FromNative(cgram, OamDraws(vram, oamUpload, spriteCount, objectSelection));

        /// <summary>A frame whose colour memory holds <paramref name="nativeColours"/>, a native 256-colour image.</summary>
        internal static FramePaint FromNative(ReadOnlySpan<RgbColor> nativeColours, ImmutableArray<SpriteDraw> spriteDraws)
        {
            BackgroundColours colours = NativeColourImage.Decode(nativeColours);
            return new FramePaint(colours, spriteDraws);
        }
    }

    /// <summary>Reads what a captured frame description shows.</summary>
    extension(FramePaint memory)
    {
        /// <summary>
        /// The frame's colours in the cartridge's colour order, for fixtures that compare or rebuild a frame's colours;
        /// built once per frame, as a published frame's colours never change.
        /// </summary>
        internal ReadOnlySpan<RgbColor> Colors => NativeColourImage.Of(memory.Backgrounds);

        /// <summary>
        /// This frame with its colour memory holding <paramref name="nativeColours"/>, a native 256-colour image read in the
        /// frame's own colour model, gameplay's or a frontend screen's; its sprite draws are kept.
        /// </summary>
        internal FramePaint WithNativeColours(ReadOnlySpan<RgbColor> nativeColours) =>
            new(NativeColourImage.DecodeLike(memory.Backgrounds, nativeColours), memory.SpriteDraws);

        /// <summary>
        /// This frame with its colour memory holding <paramref name="nativeColours"/>, read in the frame's own colour model,
        /// and its sprites an OAM fixture's records, converted as <see cref="FromOam"/> converts them.
        /// </summary>
        internal FramePaint WithNativeColoursAndOam(ReadOnlySpan<byte> vram, ReadOnlySpan<RgbColor> nativeColours,
            ReadOnlySpan<byte> oamUpload, int spriteCount, int objectSelection) =>
            new(NativeColourImage.DecodeLike(memory.Backgrounds, nativeColours), OamDraws(vram, oamUpload, spriteCount, objectSelection));

        /// <summary>Every sprite part the frame shows, frontmost first.</summary>
        internal PlacedSpritePart[] SpriteParts() => PlacedSpritePart.Of(memory.SpriteDraws);
    }

    /// <summary>The sprite draws an OAM fixture's records make over <paramref name="vram"/>, as a frontend capture converts them.</summary>
    private static ImmutableArray<SpriteDraw> OamDraws(ReadOnlySpan<byte> vram, ReadOnlySpan<byte> oamUpload, int spriteCount, int objectSelection)
    {
        var oam = new OamBuffer();
        oam.LoadUploadPayload(oamUpload, spriteCount);
        var image = new SnesVram();
        image.LoadBytes(0, vram);
        return ObjCharacterImageConstants.Draws(oam, image, objectSelection);
    }
}
