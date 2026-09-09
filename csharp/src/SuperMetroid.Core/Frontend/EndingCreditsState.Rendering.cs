using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Projects the current cartridge-backed PPU image into the desktop raster.</summary>
    public Rgba32[] Render()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);

        if (Phase == EndingCreditsPhase.Credits)
        {
            // Function 126 installs font 3 at word $4000 and the circular credits map at
            // BG1SC word $4800. The half-pixel accumulator itself is held by the credits
            // object; the PPU consumes only its whole vertical-scroll word.
            Rgba32[] text = SnesBgTilemapRenderer.Render4BppViewport(
                vram,
                cgram,
                tilemapBaseWord: EndingCreditsRomData.Rendering.CreditsTilemapWord,
                characterBaseWord: EndingCreditsRomData.Rendering.CreditsCharacterWord,
                horizontalScroll: 0,
                verticalScroll: credits!.VerticalScroll,
                width: 256,
                height: 224,
                tilemapWidthInTiles: 32,
                tilemapHeightInTiles: 32);
            SnesLayerCompositor.Composite(pixels, text);
        }
        else if (Phase >= EndingCreditsPhase.PostCreditsBlank)
        {
            RenderPostCreditsBackground(pixels);
            RenderSprites(pixels, obsel: EndingCreditsRomData.Rendering.RewardObjectSelection);
        }
        else
        {
            RenderMode7Background(pixels);
            RenderSprites(pixels, obsel: CurrentEscapeObjectSelection);
        }

        MasterBrightnessFilter.Apply(pixels, brightness);
        return pixels;
    }

    private void RenderMode7Background(Span<Rgba32> pixels)
    {
        // Ending setup uses the standard bank-$8B matrix helper. X/Y are the same signed
        // scroll words and zoom/angle are the same 8.8 scalar and sine-table index used by
        // the Ceres cinematic, so no host camera transform is introduced here.
        short cosine = ReadSine(mode7Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(mode7Angle.TableIndex);
        short matrixA = Scale(cosine, mode7Zoom);
        short matrixB = Scale(sine, mode7Zoom);
        Rgba32[] mode7 = SnesMode7Renderer.RenderViewport(
            vram,
            cgram,
            matrixA,
            matrixB,
            unchecked((short)-matrixB),
            matrixA,
            centerX: EndingCreditsRomData.Rendering.Mode7CenterX,
            centerY: EndingCreditsRomData.Rendering.Mode7CenterY,
            horizontalOffset: unchecked((short)mode7X),
            verticalOffset: unchecked((short)mode7Y));
        SnesLayerCompositor.Composite(pixels, mode7);
    }

    private byte CurrentEscapeObjectSelection => Phase < EndingCreditsPhase.FadeInZebesExplosion
        ? EndingCreditsRomData.Rendering.CloudObjectSelection : EndingCreditsRomData.Rendering.EscapeObjectSelection;

    private void RenderPostCreditsBackground(Span<Rgba32> pixels)
    {
        if (Phase == EndingCreditsPhase.PostCreditsBlank)
            return;

        // The waiting-for-credits map is the literal $97:96F4 stream copied to BG1SC
        // $4C00. It remains the base screen while the shooting-star and reward objects run.
        Rgba32[] waiting = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: EndingCreditsRomData.Rendering.PostCreditsTilemapWord,
            characterBaseWord: EndingCreditsRomData.Rendering.PostCreditsCharacterWord,
            horizontalScroll: 0,
            verticalScroll: postCreditsVerticalScroll,
            width: 256,
            height: 224,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32);
        SnesLayerCompositor.Composite(pixels, waiting);
    }

    private void RenderSprites(Span<Rgba32> pixels, byte obsel)
    {
        OamBuffer oam = PrepareSprites();
        Rgba32[] objects = SnesObjRenderer.Render(oam, vram, cgram, obsel);
        SnesLayerCompositor.Composite(pixels, objects);
    }

    // Keep sprite drawing on the simulation-side display boundary. Neither the
    // immutable packet consumer nor repeated rendering may advance sprite state.
    private OamBuffer PrepareSprites()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        foreach (EndingSprite wrapper in sprites)
            wrapper.Sprite.Draw(bus, oam);
        oam.FinalizeFrame();
        return oam;
    }

    private short ReadSine(byte index) => unchecked((short)
        RomDataReader.ReadWordFixedBank(
            bus,
            EndingCreditsRomData.Assets.SignedSineTable + index * sizeof(ushort)));

    private static short Scale(short component, ushort scalar) =>
        unchecked((short)((component * unchecked((short)scalar)) >> 8));
}
