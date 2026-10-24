using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameCapture;

namespace SuperMetroid.Verification.Areas.Escape.Artwork;

/// <summary>
/// Compares the escape timer's sheet draws with the OAM sprites the cartridge's timer routine emits: drawn over the
/// timer's characters and the colours a room load leaves in the sprite rows, both must paint the same screen pixels.
/// </summary>
internal static class EscapeTimerDrawChecks
{
    /// <summary>OBJ character $1E0, where the cartridge loads the timer's first character.</summary>
    private const int FirstCharacter = 0x1e0;

    /// <summary>The gameplay OBSEL value, under which the timer's first character $1E0 lies at VRAM byte $FC00.</summary>
    private const int TimerObjectSelection = 3;

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

    /// <summary>
    /// Asserts that the draws paint exactly the pixels the OAM sprites appended to <paramref name="native"/> paint, both
    /// drawn by the display renderer: the cartridge's sprites as a frontend capture converts their records into draws over
    /// the characters it loads at OBJ character $1E0.
    /// </summary>
    /// <param name="native">The cartridge routine's sprites; finalized here.</param>
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
        var cgram = new ScreenPalette();
        palettes.LoadInitial(cgram);
        palettes.LoadCommonSprites(cgram.SpriteRow(SpriteColourSetDefinitions.Common));
        palettes.LoadEnemyProjectileSprites(cgram.SpriteRow(SpriteColourSetDefinitions.EnemyProjectiles));

        var vram = new SnesVram();
        vram.LoadBytes(ObjCharacterImage.ResolveTileByteAddress(FirstCharacter, TimerObjectSelection), characters.Characters);
        native.FinalizeFrame();
        Rgba32[] expected = Render(FramePaintConstants.CaptureFrontend(vram, cgram, native, TimerObjectSelection));
        Rgba32[] actual = Render(new FramePaint(cgram.Colors, [.. draws]));
        Rgba32[] backdrop = Render(new FramePaint(cgram.Colors, []));
        int opaque = 0;
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], actual[index],
                $"{message}: pixel ({index % SnesPpuLayout.ScreenSize.X}, {index / SnesPpuLayout.ScreenSize.X}) colour");
            if (expected[index] != backdrop[index])
                opaque++;
        }
        AssertTrue(opaque > 0, message + ": the native timer paints pixels");
    }

    /// <summary>The frame the display renderer draws of <paramref name="paint"/>'s sprites alone.</summary>
    private static Rgba32[] Render(FramePaint paint) =>
        DisplayRendering.Render(new LayeredScene(paint, new RenderLayer[] { new ObjRenderLayer() }, FadeLevel.Full));
}
