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
/// Overlapping actors in a gameplay frame show in the order they are drawn (#2620): the actor drawn first shows where they
/// overlap, at its own depth among the room's layers, though the one drawn after it stands at a depth further forward. Ceres
/// Ridley, still unseen and behind Samus, covered her for standing in front of the room's front tiles.
/// </summary>
internal static class ActorOrderTests
{
    /// <summary>Draws a red actor behind the front tiles, then a blue one in front of them over it, and reads the overlap.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var palette = new RgbColor[SnesCgram.ColorCount];
        palette[128 + 1] = new RgbColor(255, 0, 0);
        palette[128 + 16 + 1] = new RgbColor(0, 0, 255);
        const int Side = 8;
        var size = new Extent16<ImagePixels>(2 * Side, Side);
        var pixels = new RgbaPixel[size.X * size.Y];
        var inks = new byte[pixels.Length];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            inks[pixel] = (byte)(pixel % size.X < Side ? 1 : 17);
            pixels[pixel] = palette[128 + inks[pixel]].ToRgbaPixel();
        }
        SpriteImage sheet = ComposedPictures.Install("ActorOrder", size, pixels, ComposedPictures.InksAt(inks.Select(ink => (int)ink), SpriteImageColours.Sprite),
            SpriteImageColours.Sprite, palette[128..]);
        var red = new SpriteSheetFrame(sheet, new(0, 0), new(Side, Side), default) { Depth = ActorDepth.BehindFrontParts };
        var blue = new SpriteSheetFrame(sheet, new(Side, 0), new(Side, Side), default) { Depth = ActorDepth.InFront };
        // Red covers x 20..27 and blue x 24..31, both on rows 40..47; red is drawn first.
        ImmutableArray<SpriteDraw> draws =
        [
            new ImageSpriteDraw(new(SpriteSourceKind.Samus), red, new(20, 40), SpriteMirror.None) { Shading = PictureShading.Standard },
            new ImageSpriteDraw(new(SpriteSourceKind.Enemy), blue, new(24, 40), SpriteMirror.None) { Shading = PictureShading.Standard },
        ];
        var view = new GameplaySceneView(default, default, ScreenLayers.Sprites);
        var rooms = RoomBackgroundsFixtures.Seeded(new Random(2620), new(511, 255), new(65535, 511), door: false, overlay: false);
        var frame = new DisplayPacket(0, new LayeredScene(FramePaint.FromNativeGameplay(palette, draws),
            new DisplayLayer[] { new GameplaySceneLayer(view, rooms, default) }, DisplayBrightness.Full));
        RgbaPixel[] shown = renderer.RenderForReadback(frame);
        Expect(22, 44, new RgbaPixel(255, 0, 0), "the first actor alone");
        Expect(26, 44, new RgbaPixel(255, 0, 0), "the first-drawn actor shows over the later one standing further forward");
        Expect(30, 44, new RgbaPixel(0, 0, 255), "the later actor alone");
        Console.WriteLine($"{device.Kind}: overlapping actors show in their drawing order.");

        void Expect(int x, int y, RgbaPixel expected, string what)
        {
            RgbaPixel actual = shown[y * ScreenGeometry.ScreenWidthPixels + x];
            if (actual.R != expected.R || actual.G != expected.G || actual.B != expected.B)
                throw new InvalidOperationException($"{device.Kind}: {what}: pixel ({x},{y}) is {actual}, expected {expected}.");
        }
    }
}
