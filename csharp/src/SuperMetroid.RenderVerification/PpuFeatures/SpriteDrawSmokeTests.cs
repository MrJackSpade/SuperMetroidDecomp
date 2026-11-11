using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Collections.Immutable;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.RenderVerification.PpuFeatures;

/// <summary>
/// Whole-frame sprite draws (#1308, #1345, #1392): both backends draw sprites only from the published pre-coloured image
/// draws, and place, flip and order them identically.
/// </summary>
internal static class SpriteDrawSmokeTests
{
    /// <summary>Checks the ordering contract on exact colors, then compares seeded draw scenes across backends.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        int cases = 0;
        CheckOrderingAndFlip();
        var random = new Random(1308);
        for (int sample = 0; sample < 24; sample++)
            CheckSeededScene(random);
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {cases} whole-frame sprite draw comparisons passed.");

        void CheckOrderingAndFlip()
        {
            // A sheet of four eight-pixel frames: three solid in colour 1 of rows 0 to 2, and one in row 3 colouring only
            // its left column, so a horizontal mirror moves that column to the frame's right edge.
            var palette = new RgbColor[SnesCgram.ColorCount];
            palette[128 + 1] = new RgbColor(255, 0, 0);
            palette[128 + 16 + 1] = new RgbColor(0, 255, 0);
            palette[128 + 32 + 1] = new RgbColor(0, 0, 255);
            palette[128 + 48 + 1] = new RgbColor(255, 255, 0);
            const int Frames = 4, Side = 8;
            var size = new Extent16<ImagePixels>(Frames * Side, Side);
            var sheetPixels = new RgbaPixel[size.X * size.Y];
            var indices = new byte[sheetPixels.Length];
            // An actor's frame lies in no layer's front tile (#1514).
            var frontTiles = new byte[sheetPixels.Length];
            for (int frame = 0; frame < Frames; frame++)
            for (int y = 0; y < Side; y++)
            for (int x = 0; x < Side; x++)
            {
                if (frame == Frames - 1 && x != 0)
                    continue;
                int pixel = y * size.X + frame * Side + x;
                indices[pixel] = (byte)(frame * 16 + 1);
                sheetPixels[pixel] = palette[128 + indices[pixel]].ToRgbaPixel();
            }
            SpriteImage sheet = ComposedPictures.Install("Ordering", size, sheetPixels, indices, frontTiles, SpriteImageColours.Sprite, palette[128..]);
            SpriteSheetFrame Frame(int frame) => new(sheet, new((ushort)(frame * Side), 0), new(Side, Side), default);
            // Red covers x 10..17, blue x 14..21 and green x 12..19, all on row 10, emitted in that order.
            ImmutableArray<SpriteDraw> draws =
            [
                new ImageSpriteDraw(new(SpriteSourceKind.EscapeTimer), Frame(0), new(10, 10), SpriteMirror.None),
                new ImageSpriteDraw(new(SpriteSourceKind.EscapeTimer, 1), Frame(2), new(14, 10), SpriteMirror.None),
                new ImageSpriteDraw(new(SpriteSourceKind.EscapeTimer, 2), Frame(1), new(12, 10), SpriteMirror.None),
                new ImageSpriteDraw(new(SpriteSourceKind.EscapeTimer, 3), Frame(3), new(40, 30), SpriteMirror.Horizontal),
            ];
            var memory = FramePaint.FromNative(palette, draws);
            RgbaPixel[] pixels = Check(memory, [], "ordering and flip");
            RgbaPixel red = new(255, 0, 0), blue = new(0, 0, 255), yellow = new(255, 255, 0);
            ExpectPixel(11, 10, red, "the first draw alone");
            ExpectPixel(15, 10, red, "the first-emitted draw is in front of later ones");
            ExpectPixel(18, 10, blue, "the second draw is in front of the third");
            ExpectPixel(21, 10, blue, "the second draw alone");
            // Mirroring about the origin puts the 8-pixel frame at x 32..39, its left column at x 39.
            ExpectPixel(39, 30, yellow, "a horizontal whole-frame flip mirrors the frame about the origin");
            ExpectPixel(32, 30, new RgbaPixel(0, 0, 0), "the flipped frame leaves its unpainted columns transparent");
            ExpectPixel(40, 30, new RgbaPixel(0, 0, 0), "nothing is drawn right of a horizontally flipped frame's origin");

            void ExpectPixel(int x, int y, RgbaPixel expected, string what)
            {
                RgbaPixel actual = pixels[y * ScreenGeometry.ScreenWidthPixels + x];
                if (actual.R != expected.R || actual.G != expected.G || actual.B != expected.B)
                    throw new InvalidOperationException($"{device.Kind}: {what}: pixel ({x},{y}) is {actual}, expected {expected}.");
            }
        }

        void CheckSeededScene(Random random)
        {
            var vram = new byte[SnesVram.ByteCount];
            random.NextBytes(vram);
            RgbColor[] palette = Enumerable.Range(0, SnesCgram.ColorCount)
                .Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
            var draws = ImmutableArray.CreateBuilder<SpriteDraw>();
            // Pre-coloured image draws (#1392): a sheet with transparent pixels, frames cut anywhere on it, any origin,
            // mirroring and rows, with colour math. In about half the scenes the sheet's standard
            // colours differ from the scene's, as under a palette effect, so frames whose rows differ show the live
            // colours through their palette indices.
            bool effect = random.Next(2) == 0;
            var standard = new RgbColor[ImageColourGroup.HalfColours];
            for (int colour = 0; colour < standard.Length; colour++)
                standard[colour] = effect ? RgbColor.FromWord((ushort)random.Next(32768)) : palette[128 + colour];
            var sheetPixels = new RgbaPixel[64 * 48];
            var sheetIndices = new byte[64 * 48];
            var sheetFrontTiles = new byte[64 * 48];
            for (int pixel = 0; pixel < sheetPixels.Length; pixel++)
            {
                if (random.Next(3) == 0)
                    continue;
                sheetIndices[pixel] = (byte)(random.Next(8) * 16 + random.Next(1, 16));
                sheetPixels[pixel] = standard[sheetIndices[pixel]].ToRgbaPixel();
            }
            SpriteImage sheet = ComposedPictures.Install("Seeded", new(64, 48), sheetPixels, sheetIndices, sheetFrontTiles, SpriteImageColours.Sprite, standard);
            for (int index = random.Next(1, 6); index > 0; index--)
            {
                var size = new Extent16<ImagePixels>((ushort)random.Next(1, 33), (ushort)random.Next(1, 33));
                var corner = new Position16<ImagePixels>((ushort)random.Next(0, 64 - size.X + 1), (ushort)random.Next(0, 48 - size.Y + 1));
                var image = new SpriteSheetFrame(sheet, corner, size, new((short)random.Next(-8, 40), (short)random.Next(-8, 40)));
                var origin = new Position16<ScreenPixels>(unchecked((ushort)random.Next(-60, 300)), unchecked((ushort)random.Next(-60, 270)));
                draws.Insert(random.Next(draws.Count + 1), new ImageSpriteDraw(new(SpriteSourceKind.EscapeTimer, 100 + index), image, origin,
                    (SpriteMirror)random.Next(4)));
            }
            var memory = FramePaint.FromNative(palette, draws.ToImmutable());
            PicturedPlane plane = VramPictureFixtures.Plane(vram, palette, GameplayVramLayout.Bg2TilemapWord, GameplayVramLayout.HudCharacterWord, TileInkDepth.SixteenColours,
                new Extent16<CharacterCells>(32, 32));
            Check(memory, [], $"seeded scene {cases}");
            Check(memory, [new PicturedPlaneLayer(plane, new Offset16<PlanePixels>(3, 11))], $"seeded sprites over a plane {cases}");
        }

        // Draws the frame's sprite draws as one sprite layer over `behind` (#1514).
        RgbaPixel[] Check(FramePaint memory, DisplayLayer[] behind, string label)
        {
            cases++;
            var frame = new DisplayPacket(0, memory.SpritesScene(15, SpriteBlend.Drawn, behind));
            RgbaPixel[] expected = DisplayRendering.Render(frame);
            PixelComparison.Verify(frame, expected, renderer.RenderForReadback(frame), $"{device.Kind}: {label}");
            return expected;
        }
    }
}
