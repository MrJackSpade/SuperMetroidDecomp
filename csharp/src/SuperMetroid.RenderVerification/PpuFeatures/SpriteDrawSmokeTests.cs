using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.TestSupport.FrameCapture;
using System.Collections.Immutable;

namespace SuperMetroid.RenderVerification.PpuFeatures;

/// <summary>
/// Whole-frame sprite draws (#1308, #1345): both backends draw sprites only from the published draws and place, flip
/// and order them identically, and a frontend screen's OAM records reach them only as the draw they convert to.
/// </summary>
internal static class SpriteDrawSmokeTests
{
    /// <summary>A plane-0 character row with every pixel set.</summary>
    private const byte SolidRow = byte.MaxValue;
    /// <summary>A plane-0 character row with only its leftmost pixel (bit 7) set.</summary>
    private const byte LeftmostPixel = 1 << 7;

    /// <summary>Checks the ordering contract on exact colors, then compares seeded draw scenes across backends.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM records, native VRAM words")]
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
            // VRAM character 1 is solid color 1; artwork character 0 is solid color 1 and character 1 colors only
            // its left column, so a horizontal flip moves that column to the right edge.
            var vramBytes = new byte[SnesPpuLayout.VramByteCount];
            for (int row = 0; row < 8; row++) vramBytes[SpriteArtwork.CharacterByteCount + row * 2] = SolidRow;
            var vram = new SnesVram();
            vram.LoadBytes(0, vramBytes);
            var characters = new byte[2 * SpriteArtwork.CharacterByteCount];
            for (int row = 0; row < 8; row++)
            {
                characters[row * 2] = SolidRow;
                characters[SpriteArtwork.CharacterByteCount + row * 2] = LeftmostPixel;
            }
            var artwork = new SpriteArtwork(new(SpriteArtworkKind.EscapeTimerTiles), characters);
            var palette = new Bgr555[SnesPpuLayout.CgramColorCount];
            palette[128 + 1] = new Bgr555(31, 0, 0);
            palette[128 + 16 + 1] = new Bgr555(0, 31, 0);
            palette[128 + 32 + 1] = new Bgr555(0, 0, 31);
            palette[128 + 48 + 1] = new Bgr555(31, 31, 0);
            var solid = new SpriteFrame("Solid", [new(new(0, 0), new(8, 8), 0, SnesTileFlipFlags.None, null, null)]);
            var edge = new SpriteFrame("Edge", [new(new(0, 0), new(8, 8), 1, SnesTileFlipFlags.None, null, null)]);
            // Red covers x 10..17, blue x 14..21 and green x 12..19, all on row 10, emitted in that order.
            ImmutableArray<SpriteDraw> draws =
            [
                new CharacterSpriteDraw(new(SpriteSourceKind.EscapeTimer), solid, artwork, new(10, 10), SnesTileFlipFlags.None, 0, 3),
                new CharacterSpriteDraw(new(SpriteSourceKind.EscapeTimer, 1), solid, artwork, new(14, 10), SnesTileFlipFlags.None, 2, 3),
                new CharacterSpriteDraw(new(SpriteSourceKind.EscapeTimer, 2), solid, artwork, new(12, 10), SnesTileFlipFlags.None, 1, 3),
                new CharacterSpriteDraw(new(SpriteSourceKind.EscapeTimer, 3), edge, artwork, new(40, 30), SnesTileFlipFlags.Horizontal, 3, 3),
            ];
            // A frontend screen's OAM record 0 (red, x 60..67) and record 1 (green, x 62..69) on row 50.
            var oam = new OamBuffer();
            oam.BeginFrame();
            oam.AddRawSmallSprite(new Position16<OamPixels>(60, 50), SnesObjAttributeWord.Create(1, 0, 3));
            oam.AddRawSmallSprite(new Position16<OamPixels>(62, 50), SnesObjAttributeWord.Create(1, 1, 3));
            oam.FinalizeFrame();
            draws = draws.AddRange(ObjCharacterImageConstants.Draws(oam, vram, objectSelection: 0));
            var memory = new PpuMemorySnapshot(vramBytes, palette, draws);
            Rgba32[] pixels = Check(memory, [new ObjRenderLayer()], "ordering and flip");
            Rgba32 red = new(255, 0, 0), green = new(0, 255, 0), blue = new(0, 0, 255), yellow = new(255, 255, 0);
            ExpectPixel(11, 10, red, "the first draw alone");
            ExpectPixel(15, 10, red, "the first-emitted draw is in front of later ones");
            ExpectPixel(18, 10, blue, "the second draw is in front of the third");
            ExpectPixel(21, 10, blue, "the second draw alone");
            // Mirroring about the origin puts the 8-pixel part at x 32..39, its left column at x 39.
            ExpectPixel(39, 30, yellow, "a horizontal whole-frame flip mirrors the part about the origin");
            ExpectPixel(32, 30, new Rgba32(0, 0, 0), "the flipped frame leaves its unpainted columns transparent");
            ExpectPixel(40, 30, new Rgba32(0, 0, 0), "nothing is drawn right of a horizontally flipped frame's origin");
            ExpectPixel(63, 50, red, "OAM record 0 converts in front of record 1");
            ExpectPixel(69, 50, green, "OAM record 1 converts with its own palette and position");

            void ExpectPixel(int x, int y, Rgba32 expected, string what)
            {
                Rgba32 actual = pixels[y * SnesPpuLayout.ScreenWidthPixels + x];
                if (actual.R != expected.R || actual.G != expected.G || actual.B != expected.B)
                    throw new InvalidOperationException($"{device.Kind}: {what}: pixel ({x},{y}) is {actual}, expected {expected}.");
            }
        }

        void CheckSeededScene(Random random)
        {
            var vram = new byte[SnesPpuLayout.VramByteCount];
            random.NextBytes(vram);
            Bgr555[] palette = Enumerable.Range(0, SnesPpuLayout.CgramColorCount)
                .Select(_ => Bgr555.FromWord((ushort)random.Next(32768))).ToArray();
            var characters = new byte[160 * SpriteArtwork.CharacterByteCount];
            random.NextBytes(characters);
            var shared = new SpriteArtwork(new(SpriteArtworkKind.EscapeTimerTiles, 1), characters);
            var other = new SpriteArtwork(new(SpriteArtworkKind.EscapeTimerTiles, 2), characters.AsSpan(0, 128 * SpriteArtwork.CharacterByteCount));
            var draws = ImmutableArray.CreateBuilder<SpriteDraw>();
            for (int index = random.Next(1, 12); index > 0; index--)
            {
                SpriteArtwork artwork = random.Next(2) == 0 ? shared : other;
                var parts = ImmutableArray.CreateBuilder<SpriteFramePart>();
                for (int part = random.Next(1, 5); part > 0; part--)
                {
                    var size = new Extent16<ScreenPixels>((ushort)(8 << random.Next(0, 4)), (ushort)(8 << random.Next(0, 4)));
                    int span = (size.Y / 8 - 1) * SpriteArtwork.CharactersPerRow + size.X / 8;
                    parts.Add(new(new((short)random.Next(-40, 40), (short)random.Next(-40, 40)), size,
                        random.Next(0, artwork.CharacterCount - span + 1), (SnesTileFlipFlags)(random.Next(4) << 14),
                        random.Next(3) == 0 ? (byte)random.Next(8) : null, random.Next(3) == 0 ? (byte)random.Next(4) : null));
                }
                var frame = new SpriteFrame($"Seeded.{index}", parts.ToImmutable());
                var origin = new Position16<ScreenPixels>(unchecked((ushort)random.Next(-60, 300)), unchecked((ushort)random.Next(-60, 270)));
                draws.Add(new CharacterSpriteDraw(new(SpriteSourceKind.EscapeTimer, index), frame, artwork, origin,
                    (SnesTileFlipFlags)(random.Next(4) << 14), (byte)random.Next(8), (byte)random.Next(4)));
            }
            var memory = new PpuMemorySnapshot(vram, palette, draws.ToImmutable());
            Check(memory, [new ObjRenderLayer()], $"seeded scene {cases}");
            Check(memory, [
                new ObjPriorityRenderLayer(0),
                new Bg4BppRenderLayer(GameplayVramLayout.Bg2TilemapWord, GameplayVramLayout.HudCharacterWord, new Position16<BgPlanePixels>(3, 11), new Extent16<TilemapCells>(32, 32), false),
                new ObjPriorityRenderLayer(1), new ObjPriorityRenderLayer(2),
                new Bg4BppRenderLayer(GameplayVramLayout.Bg2TilemapWord, GameplayVramLayout.HudCharacterWord, new Position16<BgPlanePixels>(3, 11), new Extent16<TilemapCells>(32, 32), true),
                new ObjPriorityRenderLayer(3)], $"seeded priority ladder {cases}");
        }

        Rgba32[] Check(PpuMemorySnapshot memory, RenderLayer[] layers, string label)
        {
            var frame = new RenderFrameSnapshot(new(++cases, 1, 0), new LayeredRenderSnapshot(memory, layers, 0, 15));
            Rgba32[] expected = SoftwareFrameSnapshotRenderer.Render(frame);
            PixelComparison.Verify(frame, expected, renderer.RenderForReadback(frame), $"{device.Kind}: {label}");
            return expected;
        }
    }
}
