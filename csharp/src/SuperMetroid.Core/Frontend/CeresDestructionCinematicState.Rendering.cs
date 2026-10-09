using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class CeresDestructionCinematicState
{
    // OBJ layer reused across renders; the span overload clears it first. Never saved state.
    /// <summary>Scratch target for rendering one OBJ priority before compositing it over the current frame.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    /// <summary>Reusable 256-by-224 output array returned by <see cref="Render"/>.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Composes the current cinematic scene into a reusable 256-by-224 RGBA frame.</summary>
    /// <returns>The rendered frame buffer, which remains valid until this scene renders again.</returns>
    public Rgba32[] Render()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224, frameBuffer ??= new Rgba32[256 * 224]);
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
            SnesMode7Renderer.CompositeViewport(
                pixels,
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
            SnesBgTilemapRenderer.Composite4BppViewport(
                pixels,
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

    /// <summary>Captures the station explosion's scanline color-add layer for insertion over the scene.</summary>
    /// <returns>The explosion layer when its effect is active; otherwise <see langword="null"/>.</returns>
    private ScanlineColorAddRenderLayer? CaptureStationExplosion() =>
        SnesGameplayFrameRenderer.CapturePowerBombColorMath(bus, stationExplosion,
            layer1X: 0, layer1Y: 0, firstVisibleScanline: 0);

    /// <summary>Draws cinematic actors into a finalized OAM buffer, preserving the phase-specific overlap order.</summary>
    /// <returns>OAM for the current actors using the installed sprite artwork.</returns>
    private OamBuffer PrepareRenderOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        // Same-priority OBJ overlap is decided by OAM order. Allocation order is
        // not stable slot order after an earlier explosion dies and its slot is reused.
        IEnumerable<IntroDiscoverySprite> drawActors = Phase <= CeresDestructionPhase.FadeOutCeres
            ? actors.OrderByDescending(actor => ceresActorSlots[actor]) : actors;
        foreach (IntroDiscoverySprite actor in drawActors)
            actor.Draw(bus, oam, installedArt: spriteArtwork);
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Builds the Mode 7 affine matrix from the cinematic angle and zoom scalar.</summary>
    /// <returns>The A, B, C, and D matrix coefficients used to transform the background.</returns>
    private (short A, short B, short C, short D) CalculateMatrix()
    {
        short cosine = ReadSine(angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(angle.TableIndex);
        short a = Scale(cosine, zoom);
        short b = Scale(sine, zoom);
        return (a, b, unchecked((short)-b), a);
    }

    /// <summary>Reads one signed sine-table component for the Mode 7 transform.</summary>
    /// <param name="index">Table index derived from the cinematic angle.</param>
    /// <returns>The signed trigonometric component stored at that index.</returns>
    private static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(index);

    /// <summary>Applies the cinematic's fixed-point zoom scalar to a matrix component.</summary>
    /// <param name="component">Signed sine or cosine coefficient to scale.</param>
    /// <param name="scalar">Zoom value represented with eight fractional bits.</param>
    /// <returns>The scaled coefficient after shifting the fixed-point product back to its native range.</returns>
    private static short Scale(short component, ushort scalar) =>
        unchecked((short)((component * unchecked((short)scalar)) >> 8));

    /// <summary>Renders one OBJ priority into the reusable scratch layer and composites it onto the frame.</summary>
    /// <param name="pixels">Destination frame receiving this priority layer.</param>
    /// <param name="oam">Finalized sprite list used to render the layer.</param>
    /// <param name="priority">OBJ priority value selected for this pass.</param>
    private void CompositeObjPriority(Span<Rgba32> pixels, OamBuffer oam, int priority)
    {
        Rgba32[] layer = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(layer, oam,
            vram,
            cgram,
            obsel: 3,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, layer);
    }
}
