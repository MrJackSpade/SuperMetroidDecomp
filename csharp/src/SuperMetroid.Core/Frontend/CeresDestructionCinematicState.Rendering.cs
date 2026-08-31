using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class CeresDestructionCinematicState
{
    public Rgba32[] Render()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        var oam = new OamBuffer();
        oam.BeginFrame();
        foreach (IntroDiscoverySprite actor in actors)
            actor.Draw(bus, oam);
        oam.FinalizeFrame();

        if (usesMode7)
        {
            (short matrixA, short matrixB, short matrixC, short matrixD) = CalculateMatrix();
            CompositeObjPriority(pixels, oam, 0);
            Rgba32[] mode7 = SnesMode7Renderer.RenderViewport(
                vram,
                cgram,
                matrixA,
                matrixB,
                matrixC,
                matrixD,
                centerX: Phase <= CeresDestructionPhase.FadeOutCeres ? (short)52 : (short)56,
                centerY: Phase <= CeresDestructionPhase.FadeOutCeres ? (short)48 : (short)24,
                horizontalOffset: unchecked((short)backgroundX),
                verticalOffset: unchecked((short)backgroundY));
            SnesLayerCompositor.Composite(pixels, mode7);
            CompositeObjPriority(pixels, oam, 1);
            CompositeObjPriority(pixels, oam, 2);
            CompositeObjPriority(pixels, oam, 3);
        }
        else
        {
            // SetupPpu_4 selects BG1SC=$5C and BG1NBA=$06. The MOSAIC register changes
            // sampling granularity, but not the underlying cartridge-authored tilemap;
            // rendering the unmosaicked samples keeps this software path deterministic
            // until the compositor gains a general post-BG mosaic stage.
            Rgba32[] bg1 = SnesBgTilemapRenderer.Render4BppViewport(
                vram,
                cgram,
                tilemapBaseWord: 0x5c00,
                characterBaseWord: 0x6000,
                horizontalScroll: 0,
                verticalScroll: 0,
                width: 256,
                height: 224,
                tilemapWidthInTiles: 32,
                tilemapHeightInTiles: 32);
            SnesLayerCompositor.Composite(pixels, bg1);
        }

        MasterBrightnessFilter.Apply(pixels, brightness);
        return pixels;
    }

    private (short A, short B, short C, short D) CalculateMatrix()
    {
        short cosine = ReadSine(unchecked((byte)(angle + 0x40)));
        short sine = ReadSine(angle);
        short a = Scale(cosine, zoom);
        short b = Scale(sine, zoom);
        return (a, b, unchecked((short)-b), a);
    }

    private short ReadSine(byte index) => unchecked((short)
        RomDataReader.ReadWordFixedBank(bus, SignedSineTableAddress + index * 2));

    private static short Scale(short component, ushort scalar) =>
        unchecked((short)((component * unchecked((short)scalar)) >> 8));

    private void CompositeObjPriority(Span<Rgba32> pixels, OamBuffer oam, int priority)
    {
        Rgba32[] layer = SnesObjRenderer.Render(
            oam,
            vram,
            cgram,
            obsel: 3,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, layer);
    }
}
