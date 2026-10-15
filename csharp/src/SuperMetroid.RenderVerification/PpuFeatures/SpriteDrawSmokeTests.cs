using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Collections.Immutable;

namespace SuperMetroid.RenderVerification.PpuFeatures;

/// <summary>
/// Whole-frame sprite draws (#1308): both backends place, flip and order them identically, a draw sits between the
/// OAM sprites around its ordinal, and a version-30 packet carries them without change.
/// </summary>
internal static class SpriteDrawSmokeTests
{
    /// <summary>A plane-0 character row with every pixel set.</summary>
    private const byte SolidRow = byte.MaxValue;
    /// <summary>A plane-0 character row with only its leftmost pixel (bit 7) set.</summary>
    private const byte LeftmostPixel = 1 << 7;

    /// <summary>Checks the ordering contract on exact colors, then compares seeded draw scenes across backends.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native VRAM words")]
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
            var vram = new byte[SnesPpuLayout.VramByteCount];
            for (int row = 0; row < 8; row++) vram[SpriteArtwork.CharacterByteCount + row * 2] = SolidRow;
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
            // OAM sprite 0 (red) covers x 10..17 and sprite 1 (green) covers x 12..19, both on row 10.
            var oam = new byte[SnesPpuLayout.OamUploadByteCount];
            Sprite(0, 10, SnesObjAttributeWord.Create(1, 0, 3));
            Sprite(1, 12, SnesObjAttributeWord.Create(1, 1, 3));
            void Sprite(int slot, byte x, SnesObjAttributeWord attributes)
            {
                oam[slot * 4] = x;
                oam[slot * 4 + 1] = 10;
                oam[slot * 4 + 2] = SnesLittleEndian.LowByte(attributes.Raw);
                oam[slot * 4 + 3] = SnesLittleEndian.HighByte(attributes.Raw);
            }
            var solid = new SpriteFrame("Solid", [new(new(0, 0), new(8, 8), 0, SnesTileFlipFlags.None, null, null)]);
            var edge = new SpriteFrame("Edge", [new(new(0, 0), new(8, 8), 1, SnesTileFlipFlags.None, null, null)]);
            // The blue draw at x 14..21 is emitted after sprite 0 and before sprite 1.
            ImmutableArray<OrderedSpriteDraw> draws =
            [
                new(1, new SpriteDraw(new(SpriteSourceKind.EscapeTimer), solid, artwork, new(14, 10), SnesTileFlipFlags.None, 2, 3)),
                new(2, new SpriteDraw(new(SpriteSourceKind.EscapeTimer, 1), edge, artwork, new(40, 30), SnesTileFlipFlags.Horizontal, 3, 3)),
            ];
            var memory = new PpuMemorySnapshot(vram, palette, oam, 2, draws);
            Rgba32[] pixels = Check(memory, [new ObjRenderLayer()], "ordering and flip");
            Rgba32 red = new(255, 0, 0), blue = new(0, 0, 255), yellow = new(255, 255, 0);
            ExpectPixel(13, 10, red, "sprite 0 alone");
            ExpectPixel(15, 10, red, "sprite 0 is in front of the later-emitted draw");
            ExpectPixel(18, 10, blue, "the draw is in front of sprite 1, emitted after it");
            ExpectPixel(21, 10, blue, "the draw alone");
            // Mirroring about the origin puts the 8-pixel part at x 32..39, its left column at x 39.
            ExpectPixel(39, 30, yellow, "a horizontal whole-frame flip mirrors the part about the origin");
            ExpectPixel(32, 30, new Rgba32(0, 0, 0), "the flipped frame leaves its unpainted columns transparent");
            ExpectPixel(40, 30, new Rgba32(0, 0, 0), "nothing is drawn right of a horizontally flipped frame's origin");

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
            var oam = new byte[SnesPpuLayout.OamUploadByteCount];
            random.NextBytes(vram);
            random.NextBytes(oam);
            Bgr555[] palette = Enumerable.Range(0, SnesPpuLayout.CgramColorCount)
                .Select(_ => Bgr555.FromWord((ushort)random.Next(32768))).ToArray();
            var characters = new byte[160 * SpriteArtwork.CharacterByteCount];
            random.NextBytes(characters);
            var shared = new SpriteArtwork(new(SpriteArtworkKind.EscapeTimerTiles, 1), characters);
            var other = new SpriteArtwork(new(SpriteArtworkKind.EscapeTimerTiles, 2), characters.AsSpan(0, 128 * SpriteArtwork.CharacterByteCount));
            int oamCount = random.Next(0, 129);
            var draws = ImmutableArray.CreateBuilder<OrderedSpriteDraw>();
            int ordinal = 0;
            for (int index = random.Next(1, 12); index > 0; index--)
            {
                ordinal = Math.Min(oamCount, ordinal + random.Next(0, 20));
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
                draws.Add(new(ordinal, new SpriteDraw(new(SpriteSourceKind.EscapeTimer, index), frame, artwork, origin,
                    (SnesTileFlipFlags)(random.Next(4) << 14), (byte)random.Next(8), (byte)random.Next(4))));
            }
            var memory = new PpuMemorySnapshot(vram, palette, oam, oamCount, draws.ToImmutable());
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
            RenderFrameSnapshot restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(frame));
            PixelComparison.Verify(restored, expected, SoftwareFrameSnapshotRenderer.Render(restored), $"{device.Kind}: {label} after packet round trip");
            if (!RenderFrameSnapshotWriter.Serialize(restored).SequenceEqual(RenderFrameSnapshotWriter.Serialize(frame)))
                throw new InvalidOperationException($"{device.Kind}: {label}: re-serialized packet bytes differ.");
            return expected;
        }
    }
}
