using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Areas.Escape.Artwork;

/// <summary>
/// Compares the escape timer's sheet draws with the OAM sprites the cartridge's timer routine emits: rendered over the
/// timer's characters and the colours a room load leaves in the sprite rows, both must paint the same screen pixels,
/// at the same priority, taking part in colour math alike.
/// </summary>
internal static class EscapeTimerDrawChecks
{
    /// <summary>OBJ character $1E0, where the cartridge loads the timer's first character.</summary>
    private const int FirstCharacter = 0x1e0;

    /// <summary>The OBJ palette rows at or past which a sprite takes part in colour math.</summary>
    private const int FirstBlendingRow = 4;

    /// <summary>Composes the stock timer sheet from the cartridge.</summary>
    internal static SpriteSheet Sheet(ISnesAddressSpace bus)
    {
        ComposedSpriteSheet composed = EscapeTimerSheetComposer.Compose(bus);
        return SpriteSheet.Load(new MemoryStream(SpriteSheet.Write(composed.Manifest), writable: false),
            SpriteSheetDefinitions.FileName(SpriteSheetId.EscapeTimer), composed.Size, composed.Pixels, composed.PaletteIndices, composed.Priorities, composed.PartRows);
    }

    /// <summary>Draws the timer into a fresh sprite frame and returns its finalized draws.</summary>
    internal static IReadOnlyList<SpriteDraw> Draw(EscapeTimer timer, EscapeTimerPresentation presentation, SpriteSheet sheet)
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        EscapeTimerRenderer.Draw(timer, sprites, presentation, sheet);
        sprites.FinalizeFrame();
        return sprites.Draws;
    }

    /// <summary>Asserts that the draws paint exactly the pixels the OAM sprites appended to <paramref name="native"/> paint.</summary>
    /// <param name="native">The cartridge routine's sprites.</param>
    /// <param name="draws">The timer's published draws.</param>
    /// <param name="characters">The timer characters the cartridge loads at OBJ character $1E0.</param>
    /// <param name="palettes">The starting and common sprite colours a room load copies into CGRAM.</param>
    /// <param name="message">Prefix for every assertion.</param>
    internal static void AssertSamePixels(OamBuffer native, IReadOnlyList<SpriteDraw> draws, SpriteArtwork characters,
        GameplayBasePaletteCatalog palettes, string message)
    {
        foreach (SpriteDraw draw in draws)
        {
            AssertEqual(SpriteSourceKind.EscapeTimer, draw.Source.Kind, message + ": draw source");
            AssertTrue(draw is ImageSpriteDraw, message + ": timer draws are sheet images");
        }
        var cgram = new SnesCgram();
        palettes.LoadInitial(cgram);
        palettes.LoadCommonSprites(cgram, RoomLoadingRomData.CommonGameplaySpritePaletteCgramIndex);
        palettes.LoadEnemyProjectileSprites(cgram, RoomLoadingRomData.InitialEnemyProjectilePaletteCgramIndex);

        var sprites = new List<CharacterSpriteDraw>();
        for (int index = 0; index < native.AppendedSpriteCount; index++)
        {
            OamEntry entry = native.GetEntry(index);
            ushort size = entry.IsLarge ? (ushort)16 : (ushort)8;
            var frame = new SpriteFrame($"OAM {index}", [new SpriteFramePart(default, new(size, size), entry.TileNumber - FirstCharacter,
                (entry.FlipX ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) | (entry.FlipY ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None),
                null, null)]);
            // OAM's nine-bit X and eight-bit Y wrap; a screen origin carries the same position as a signed word.
            var position = new Position16<ScreenPixels>(unchecked((ushort)(entry.Position.X >= 256 ? entry.Position.X - 512 : entry.Position.X)),
                unchecked((ushort)(entry.Position.Y >= 224 ? entry.Position.Y - 256 : entry.Position.Y)));
            sprites.Add(new CharacterSpriteDraw(new SpriteSource(SpriteSourceKind.EscapeTimer), frame, characters, position,
                SnesTileFlipFlags.None, entry.Palette, entry.Priority));
        }

        Raster expected = Render(sprites, cgram), actual = Render(draws, cgram);
        int opaque = 0;
        for (int index = 0; index < expected.Pixels.Length; index++)
        {
            string at = $"{message}: pixel ({index % SnesPpuLayout.ScreenSize.X}, {index / SnesPpuLayout.ScreenSize.X})";
            AssertEqual(expected.Pixels[index], actual.Pixels[index], at + " colour");
            AssertEqual(expected.Priorities[index], actual.Priorities[index], at + " priority");
            AssertEqual(Blends(expected.Palettes[index]), Blends(actual.Palettes[index]), at + " colour math");
            if (expected.Priorities[index] != SnesObjRenderer.TransparentPriority)
                opaque++;
        }
        AssertTrue(opaque > 0, message + ": the native timer paints pixels");

        static bool Blends(byte row) => row != SnesObjRenderer.TransparentPriority && row >= FirstBlendingRow;
    }

    /// <summary>Renders draws to a full screen's colours, priorities and palette rows.</summary>
    private static Raster Render(IEnumerable<SpriteDraw> draws, SnesCgram cgram)
    {
        int count = SnesPpuLayout.ScreenSize.X * SnesPpuLayout.ScreenSize.Y;
        var raster = new Raster(new Rgba32[count], new byte[count], new byte[count]);
        SnesObjRenderer.RenderResolved([.. draws], cgram, new ObjRasterTargets(raster.Pixels, raster.Priorities, raster.Palettes));
        return raster;
    }

    /// <summary>Renders the cartridge's character composition to a full screen's colours, priorities and palette rows.</summary>
    private static Raster Render(IEnumerable<CharacterSpriteDraw> draws, SnesCgram cgram)
    {
        int count = SnesPpuLayout.ScreenSize.X * SnesPpuLayout.ScreenSize.Y;
        var raster = new Raster(new Rgba32[count], new byte[count], new byte[count]);
        CharacterSpriteRaster.RenderResolved([.. draws], cgram, new ObjRasterTargets(raster.Pixels, raster.Priorities, raster.Palettes));
        return raster;
    }

    /// <summary>One rendered screen.</summary>
    /// <param name="Pixels">Colours, row by row.</param>
    /// <param name="Priorities">Each pixel's winning priority, or transparent.</param>
    /// <param name="Palettes">Each pixel's palette row, or transparent.</param>
    private sealed record Raster(Rgba32[] Pixels, byte[] Priorities, byte[] Palettes);
}
