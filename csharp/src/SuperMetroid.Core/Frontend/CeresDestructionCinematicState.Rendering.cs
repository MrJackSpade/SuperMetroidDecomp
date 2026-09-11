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
        OamBuffer oam = PrepareRenderOam();

        if (usesMode7 && (mainScreenLayers & SnesMainScreenLayers.Bg1) == 0)
        {
            for (byte priority = 0; priority < 4; priority++)
                CompositeObjPriority(pixels, oam, priority);
        }
        else if (usesMode7)
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
                centerX: Phase <= CeresDestructionPhase.FadeOutCeres
                    ? CeresDestructionRomData.Rendering.CeresCenterX : CeresDestructionRomData.Rendering.ZebesCenterX,
                centerY: Phase <= CeresDestructionPhase.FadeOutCeres
                    ? CeresDestructionRomData.Rendering.CeresCenterY : CeresDestructionRomData.Rendering.ZebesCenterY,
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
                tilemapBaseWord: CeresDestructionRomData.Rendering.Mode1TilemapWord,
                characterBaseWord: CeresDestructionRomData.Rendering.Mode1CharacterWord,
                horizontalScroll: 0,
                verticalScroll: 0,
                width: 256,
                height: 224,
                tilemapWidthInTiles: 32,
                tilemapHeightInTiles: 32);
            SnesLayerCompositor.Composite(pixels, bg1);
        }

        if (CaptureStationExplosion() is { } blast)
            SoftwareScanlineColorRenderer.Composite(pixels, blast);
        MasterBrightnessFilter.Apply(pixels, brightness);
        return pixels;
    }

    /// <summary>Captures the scene's exact Mode 7/OBJ insertion order or Mode 1 planet plane.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        OamBuffer oam = PrepareRenderOam();
        RenderLayer[] layers;
        if (usesMode7 && (mainScreenLayers & SnesMainScreenLayers.Bg1) == 0)
        {
            layers = [new ObjPriorityRenderLayer(0), new ObjPriorityRenderLayer(1),
                new ObjPriorityRenderLayer(2), new ObjPriorityRenderLayer(3)];
        }
        else if (usesMode7)
        {
            var (a, b, c, d) = CalculateMatrix();
            var mode7 = new Mode7RenderRegisters(a, b, c, d,
                Phase <= CeresDestructionPhase.FadeOutCeres
                    ? CeresDestructionRomData.Rendering.CeresCenterX : CeresDestructionRomData.Rendering.ZebesCenterX,
                Phase <= CeresDestructionPhase.FadeOutCeres
                    ? CeresDestructionRomData.Rendering.CeresCenterY : CeresDestructionRomData.Rendering.ZebesCenterY,
                unchecked((short)backgroundX), unchecked((short)backgroundY));
            layers = [new ObjPriorityRenderLayer(0), new Mode7RenderLayer(mode7),
                new ObjPriorityRenderLayer(1), new ObjPriorityRenderLayer(2), new ObjPriorityRenderLayer(3)];
        }
        else
        {
            // Preserve the existing non-mosaicked Mode 1 path. Mosaic implementation
            // is a cartridge-correctness change, not an implicit part of this extraction.
            layers = [new Bg4BppRenderLayer(CeresDestructionRomData.Rendering.Mode1TilemapWord,
                CeresDestructionRomData.Rendering.Mode1CharacterWord, 0, 0, 32, 32, null)];
        }
        if (CaptureStationExplosion() is { } blast)
            layers = [.. layers, blast];
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), layers,
            MenuRenderDefinitions.ObjectSelection, checked((byte)brightness));
    }

    private ScanlineColorAddRenderLayer? CaptureStationExplosion() =>
        SnesGameplayFrameRenderer.CapturePowerBombColorMath(bus, stationExplosion,
            layer1X: 0, layer1Y: 0, firstVisibleScanline: 0);

    private OamBuffer PrepareRenderOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        // Same-priority OBJ overlap is decided by OAM order. Allocation order is
        // not stable slot order after an earlier explosion dies and its slot is reused.
        IEnumerable<IntroDiscoverySprite> drawActors = Phase <= CeresDestructionPhase.FadeOutCeres
            ? actors.OrderByDescending(actor => ceresActorSlots[actor]) : actors;
        foreach (IntroDiscoverySprite actor in drawActors) actor.Draw(bus, oam);
        oam.FinalizeFrame();
        return oam;
    }

    private (short A, short B, short C, short D) CalculateMatrix()
    {
        short cosine = ReadSine(angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(angle.TableIndex);
        short a = Scale(cosine, zoom);
        short b = Scale(sine, zoom);
        return (a, b, unchecked((short)-b), a);
    }

    private short ReadSine(byte index) => unchecked((short)
        RomDataReader.ReadWordFixedBank(
            bus,
            CeresDestructionRomData.Assets.SignedSineTable + index * sizeof(ushort)));

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
