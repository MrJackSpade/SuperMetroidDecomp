using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;

namespace SuperMetroid.RenderVerification.PpuFeatures;

/// <summary>
/// A translucent draw shows at half strength over what lies behind it at its depth (#2642): over the backdrop alone, and over
/// an opaque actor at its depth, which shows through it. A Metroid's shell is drawn so, the steady equivalent of the
/// game's showing it every other update.
/// </summary>
internal static class TranslucentSpriteTests
{
    /// <summary>Draws an opaque red actor and a translucent blue one over part of it, and reads the pixels.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var palette = new RgbColor[SnesCgram.ColorCount];
        palette[128 + 1] = new RgbColor(248, 0, 0);
        palette[128 + 16 + 1] = new RgbColor(0, 0, 248);
        const int Side = 8;
        var size = new Extent16<ImagePixels>(2 * Side, Side);
        var pixels = new RgbaPixel[size.X * size.Y];
        var inks = new byte[pixels.Length];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            inks[pixel] = (byte)(pixel % size.X < Side ? 1 : 17);
            pixels[pixel] = palette[128 + inks[pixel]].ToRgbaPixel();
        }
        SpriteImage sheet = ComposedPictures.Install("Translucent", size, pixels, ComposedPictures.InksAt(inks.Select(ink => (int)ink), SpriteImageColours.Sprite),
            SpriteImageColours.Sprite, palette[128..]);
        var red = new SpriteSheetFrame(sheet, new(0, 0), new(Side, Side), default) { Depth = ActorDepth.InFront };
        var blue = new SpriteSheetFrame(sheet, new(Side, 0), new(Side, Side), default) { Depth = ActorDepth.InFront };
        // Red covers x 20..27 and the translucent blue x 24..31, both on rows 40..47.
        ImmutableArray<SpriteDraw> draws =
        [
            new ImageSpriteDraw(new(SpriteSourceKind.RoomEffectActor), blue, new(24, 40), SpriteMirror.None) { Shading = PictureShading.Standard, Translucent = true },
            new ImageSpriteDraw(new(SpriteSourceKind.Enemy), red, new(20, 40), SpriteMirror.None) { Shading = PictureShading.Standard },
        ];
        var view = new GameplaySceneView(default, default, ScreenLayers.Sprites);
        var rooms = RoomBackgroundsFixtures.Seeded(new Random(2642), new(511, 255), new(65535, 511), door: false, overlay: false);
        var frame = new DisplayPacket(0, new LayeredScene(FramePaint.FromNativeGameplay(palette, draws),
            new DisplayLayer[] { new GameplaySceneLayer(view, rooms, default) }, DisplayBrightness.Full));
        RgbaPixel[] shown = renderer.RenderForReadback(frame);
        RgbaPixel backdrop = shown[44 * ScreenGeometry.ScreenWidthPixels + 40];
        Expect(22, 44, new RgbaPixel(248, 0, 0), "the opaque actor alone");
        Expect(26, 44, new RgbaPixel(124, 0, 124), "the translucent actor at half strength over the opaque one");
        Expect(30, 44, new RgbaPixel((byte)((backdrop.R + 1) / 2), (byte)((backdrop.G + 1) / 2), (byte)((backdrop.B + 248 + 1) / 2)),
            "the translucent actor at half strength over what lies behind it");
        Console.WriteLine($"{device.Kind}: a translucent actor shows at half strength over what lies behind it.");

        void Expect(int x, int y, RgbaPixel expected, string what)
        {
            RgbaPixel actual = shown[y * ScreenGeometry.ScreenWidthPixels + x];
            if (Math.Abs(actual.R - expected.R) > 1 || Math.Abs(actual.G - expected.G) > 1 || Math.Abs(actual.B - expected.B) > 1)
                throw new InvalidOperationException($"{device.Kind}: {what}: pixel ({x},{y}) is {actual}, expected {expected}.");
        }
    }
}
