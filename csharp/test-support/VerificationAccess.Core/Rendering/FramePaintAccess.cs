using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Verification access to <see cref="FramePaint"/> (#1512): frames painted from a native 256-colour image, OAM fixtures,
/// and the parts a frame shows. A native-coloured frame draws each of its pictures as a copy whose standard colours are
/// the native colours its inks show (<see cref="NativeColouredPictures"/>), in the standard shading, and shows native
/// colour 0 as its backdrop; production frames stay shading-only.
/// </summary>
internal static class FramePaintAccess
{
    /// <summary>Builds frames from native colour images and OAM fixture tables.</summary>
    extension(FramePaint)
    {
        /// <summary>
        /// A frame whose sprites are an OAM fixture's records, converted as a frontend capture converts them
        /// (the extraction pipeline's ObjCharacterImage), coloured from <paramref name="cgram"/>, a native 256-colour image,
        /// so the records reach both backends only as a frame description.
        /// </summary>
        internal static FramePaint FromOam(ReadOnlySpan<byte> vram, ReadOnlySpan<RgbColor> cgram,
            ReadOnlySpan<byte> oamUpload, int spriteCount, int objectSelection) =>
            FramePaint.FromNative(cgram, OamDraws(vram, oamUpload, spriteCount, objectSelection));

        /// <summary>
        /// A frame painted from <paramref name="nativeColours"/>, a native 256-colour image: it shows native colour 0 where
        /// nothing is drawn, and each draw shows its picture in the native colours its inks name.
        /// </summary>
        /// <exception cref="ArgumentException">The image does not hold 256 colours, or the draw list is missing or holds a missing draw.</exception>
        internal static FramePaint FromNative(ReadOnlySpan<RgbColor> nativeColours, ImmutableArray<SpriteDraw> spriteDraws)
        {
            Ensure.LengthEqual(nativeColours, NativeColouredPictures.ColorCount);
            return new FramePaint(nativeColours[0], NativeColouredPictures.Draws(spriteDraws, nativeColours));
        }

        /// <summary>
        /// A gameplay frame painted from <paramref name="nativeColours"/> as <see cref="FromNative"/> paints a frontend one:
        /// every group of background pictures in its standard colours, which a native-coloured fixture's pictures hold, and
        /// no display fade.
        /// </summary>
        /// <exception cref="ArgumentException">The image does not hold 256 colours, or the draw list is missing or holds a missing draw.</exception>
        internal static FramePaint FromNativeGameplay(ReadOnlySpan<RgbColor> nativeColours, ImmutableArray<SpriteDraw> spriteDraws)
        {
            Ensure.LengthEqual(nativeColours, NativeColouredPictures.ColorCount);
            var gameplay = new GameplayShading(nativeColours[0],
                Enum.GetValues<GameplayPictures>().ToImmutableDictionary(pictures => pictures, _ => PictureShading.Standard), fade: null);
            return new FramePaint(gameplay, NativeColouredPictures.Draws(spriteDraws, nativeColours));
        }
    }

    /// <summary>Repaints and reads what a captured frame description shows.</summary>
    extension(FramePaint memory)
    {
        /// <summary>
        /// This frame painted from <paramref name="nativeColours"/>, a native 256-colour image: its sprite draws show their
        /// pictures in the native colours their inks name and its backdrop is native colour 0. A gameplay frame's
        /// background pictures keep the shading the port gave them, without the display's fade over the sprites, as the
        /// native colours already show it.
        /// </summary>
        internal FramePaint WithNativeColours(ReadOnlySpan<RgbColor> nativeColours) =>
            Repainted(memory, nativeColours, memory.SpriteDraws);

        /// <summary>
        /// This frame painted from <paramref name="nativeColours"/> as <see cref="WithNativeColours"/> paints it, with its
        /// sprites an OAM fixture's records, converted as <see cref="FromOam"/> converts them.
        /// </summary>
        internal FramePaint WithNativeColoursAndOam(ReadOnlySpan<byte> vram, ReadOnlySpan<RgbColor> nativeColours,
            ReadOnlySpan<byte> oamUpload, int spriteCount, int objectSelection) =>
            Repainted(memory, nativeColours, OamDraws(vram, oamUpload, spriteCount, objectSelection));

        /// <summary>
        /// This frame with <paramref name="draws"/> in place of its sprite draws, each already showing the shading it
        /// shows here: a gameplay frame keeps each group of background pictures' shading, the fade included, and lays no
        /// fade or escape timer shading over the draws again; a frontend screen's frame keeps its backdrop.
        /// </summary>
        /// <exception cref="ArgumentException">The draw list is missing, holds a missing draw, or holds a draw that names no shading.</exception>
        internal FramePaint WithShownDraws(ImmutableArray<SpriteDraw> draws)
        {
            Ensure.NotNull(memory);
            return memory.Gameplay is { } gameplay
                ? new FramePaint(NativeGameplay(gameplay, gameplay.Backdrop), draws)
                : new FramePaint(memory.Backdrop, draws);
        }

        /// <summary>Every sprite part the frame shows, frontmost first.</summary>
        internal PlacedSpritePart[] SpriteParts() => PlacedSpritePart.Of(memory.SpriteDraws);

        /// <summary>
        /// A scene showing this frame's sprite draws as its one sprite layer (#1514), shown as <paramref name="blend"/> says,
        /// over its backdrop and with <paramref name="before"/> behind them; a fixture builds its sprites as a frame's draws.
        /// </summary>
        /// <param name="brightness">The scene's brightness.</param>
        /// <param name="blend">How the sprites show.</param>
        /// <param name="before">The layers drawn behind the sprites, back to front.</param>
        internal LayeredScene SpritesScene(int brightness, SpriteBlend blend = SpriteBlend.Drawn, params DisplayLayer[] before) =>
            new(new FramePaint(memory.Backdrop), [.. before, new SpritesLayer(memory.SpriteDraws, blend)], brightness);
    }

    /// <summary>
    /// <paramref name="draws"/> painted from <paramref name="nativeColours"/> in <paramref name="memory"/>'s colour model:
    /// a gameplay frame keeps each group of background pictures' shading with native colour 0 as its backdrop and no fade
    /// over its draws; a frontend screen's frame shows native colour 0 as its backdrop.
    /// </summary>
    private static FramePaint Repainted(FramePaint memory, ReadOnlySpan<RgbColor> nativeColours, ImmutableArray<SpriteDraw> draws)
    {
        Ensure.NotNull(memory);
        Ensure.LengthEqual(nativeColours, NativeColouredPictures.ColorCount);
        ImmutableArray<SpriteDraw> shown = NativeColouredPictures.Draws(draws, nativeColours);
        return memory.Gameplay is { } gameplay
            ? new FramePaint(NativeGameplay(gameplay, nativeColours[0]), shown)
            : new FramePaint(nativeColours[0], shown);
    }

    /// <summary>
    /// <paramref name="gameplay"/> with <paramref name="backdrop"/> shown where nothing is drawn: each group of background
    /// pictures shows what it showed, the fade included, and no fade or escape timer shading is laid over the
    /// native-coloured draws, whose native colours already show them.
    /// </summary>
    private static GameplayShading NativeGameplay(GameplayShading gameplay, RgbColor backdrop) =>
        new(backdrop, Enum.GetValues<GameplayPictures>().ToImmutableDictionary(pictures => pictures,
            pictures => pictures == GameplayPictures.EscapeTimer ? PictureShading.Standard : gameplay.Of(pictures)), fade: null);

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
