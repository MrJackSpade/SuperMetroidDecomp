using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Reference composition from immutable state, without scene or ROM access.</summary>
public static class SoftwareLayeredSnapshotRenderer
{
    public static Rgba32[] Render(LayeredRenderSnapshot snapshot, Rgba32[]? gameplayOutputBuffer = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var memory = new SoftwarePpuSnapshotMemory(snapshot.Memory);
        // The fused ordinary/X-ray gameplay base owns and fills its output. Creating a
        // backdrop here first would immediately discard a native-sized large object
        // every frame. Other layer sequences still require the initialized backdrop.
        bool ownsOutput = !snapshot.Layers.IsEmpty && snapshot.Layers[0] is OrdinaryGameplayRenderLayer or XrayGameplayRenderLayer;
        Rgba32[] output = ownsOutput ? Array.Empty<Rgba32>() : SnesLayerCompositor.CreateBackdrop(memory.Cgram,
            SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels, gameplayOutputBuffer);
        // Resolve OAM precedence once. Drawing independently filtered OBJ lists would
        // wrongly allow a higher-numbered record to shine through the winning record.
        bool usesObjInsertion = false;
        foreach (RenderLayer layer in snapshot.Layers)
            usesObjInsertion |= layer is ObjRenderLayer or ObjPriorityRenderLayer or Mode7RenderLayer { SubtractObjSubscreen: true } or Mode7RenderLayer { AddBg1Subscreen: true }
                or BgSubscreenAddRenderLayer { IncludeObjects: true } or BgSubscreenAddRenderLayer { MainObjects: true };
        ResolvedObjFrame objects = usesObjInsertion
            ? SnesObjRenderer.RenderResolved(memory.Oam, memory.Vram, memory.Cgram, snapshot.ObjectSelection)
            : default;
        foreach (RenderLayer layer in snapshot.Layers)
        {
            switch (layer)
            {
                case XrayGameplayRenderLayer gameplayXray:
                    output = SoftwareXrayGameplayRenderer.Render(memory, gameplayXray, snapshot.ObjectSelection, gameplayOutputBuffer);
                    break;
                case XrayWindowRenderLayer xray:
                    Rgba32[] revealed = Render(xray.Reveal);
                    for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < SnesPpuLayout.ScreenHeightPixels; y++)
                    for (int x = 0; x < SnesPpuLayout.ScreenWidthPixels; x++)
                    {
                        int index = y * SnesPpuLayout.ScreenWidthPixels + x;
                        XrayWindowLine line = xray.Lines[y];
                        output[index] = x >= line.Left && x <= line.Right
                            ? revealed[index] : SnesGameplayFrameRenderer.ApplyXrayOutsideHalfColor(output[index]);
                    }
                    break;
                case WindowedSceneRenderLayer window:
                    Rgba32[] scene = Render(window.Scene);
                    for (int y = window.Top; y < window.Bottom; y++)
                    {
                        int offset = y * SnesPpuLayout.ScreenWidthPixels + window.Left;
                        scene.AsSpan(offset, window.Right - window.Left).CopyTo(output.AsSpan(offset));
                    }
                    break;
                case BgSubscreenAddRenderLayer sub:
                    Rgba32[] subscreen = sub.FourBpp
                        ? SnesBgTilemapRenderer.Render4BppViewport(memory.Vram, memory.Cgram,
                            sub.TilemapWord, sub.CharacterWord, 0, 0, 256, 224, 32, 32)
                        : SnesBgTilemapRenderer.Render2Bpp(memory.Vram, memory.Cgram,
                            sub.TilemapWord, sub.CharacterWord, rowCount: 28, transparentColorZero: true);
                    if (sub.IncludeObjects)
                    {
                        Rgba32[] high = sub.FourBpp
                            ? SnesBgTilemapRenderer.Render4BppViewport(memory.Vram, memory.Cgram,
                                sub.TilemapWord, sub.CharacterWord, 0, 0, 256, 224, 32, 32, priority: true)
                            : SnesBgTilemapRenderer.Render2Bpp(memory.Vram, memory.Cgram,
                                sub.TilemapWord, sub.CharacterWord, rowCount: 28, transparentColorZero: true, priority: true);
                        for (int i = 0; i < subscreen.Length; i++)
                            if (objects.Pixels[i].A != 0 && (subscreen[i].A == 0
                                || objects.Priorities[i] >= (high[i].A != 0 ? 3 : 2)))
                                subscreen[i] = objects.Pixels[i];
                    }
                    if (sub.MainCoverage is { } coverage)
                    {
                        Rgba32[] mask = SnesBgTilemapRenderer.Render4BppViewport(memory.Vram, memory.Cgram,
                            coverage.TilemapWord, coverage.CharacterWord, coverage.HorizontalScroll, coverage.VerticalScroll,
                            SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels,
                            coverage.MapWidthTiles, coverage.MapHeightTiles, priority: coverage.Priority);
                        byte[]? palettes = null;
                        Rgba32[]? highMain = null;
                        if (sub.MainObjects)
                        {
                            palettes = new byte[subscreen.Length];
                            SnesObjRenderer.CompositeUnfiltered(memory.Oam, memory.Vram, memory.Cgram,
                                snapshot.ObjectSelection, new Rgba32[subscreen.Length], palettes);
                            highMain = SnesBgTilemapRenderer.Render4BppViewport(memory.Vram, memory.Cgram,
                                coverage.TilemapWord, coverage.CharacterWord, coverage.HorizontalScroll, coverage.VerticalScroll,
                                256, 224, coverage.MapWidthTiles, coverage.MapHeightTiles, priority: true);
                        }
                        for (int i = 0; i < subscreen.Length; i++)
                        {
                            bool eligible = mask[i].A != 0;
                            if (sub.MainObjects && objects.Pixels[i].A != 0 &&
                                (!eligible || objects.Priorities[i] >= (highMain![i].A != 0 ? 3 : 2)))
                                eligible = palettes![i] >= 4;
                            if (!eligible) subscreen[i] = default;
                        }
                    }
                    SnesLayerCompositor.AddSubscreen(output, subscreen);
                    break;
                case Mode7GameplayRenderLayer gameplay7:
                    output = SoftwareMode7GameplayRenderer.Render(memory, gameplay7, snapshot.ObjectSelection);
                    break;
                case Bg2BppColorMathRenderLayer bgMath:
                    SoftwareBgColorMathRenderer.Composite(output, memory.Vram, memory.Cgram, bgMath);
                    break;
                case MessageBoxRenderLayer message:
                    GameplayMessageBoxRenderer.Composite(output, message, memory.Vram, memory.Cgram);
                    break;
                case ScanlineColorAddRenderLayer colorWindows:
                    SoftwareScanlineColorRenderer.Composite(output, colorWindows);
                    break;
                case OrdinaryGameplayRenderLayer gameplay:
                    OrdinaryGameplayRegisters r = gameplay.Registers;
                    output = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                        memory.Vram, memory.Cgram, memory.Oam, r.Bg1X, r.Bg1Y, r.Bg2X, r.Bg2Y,
                        gameplay.HorizontalScrolls.IsEmpty ? null : gameplay.HorizontalScrolls.ToArray(),
                        gameplay.VerticalScrolls.IsEmpty ? null : gameplay.VerticalScrolls.ToArray(),
                        r.Bg2WidthTiles, r.Bg2HeightTiles, r.Bg2TilemapWord,
                        r.Bg1CharacterWord, r.Bg2CharacterWord, r.HudCharacterWord,
                        snapshot.ObjectSelection, r.MainScreenLayers, gameplayOutputBuffer,
                        r.Bg2FirstScanline, r.Bg2EndScanline, r.Windows, r.MainScreenWindowMask);
                    break;
                case Bg2BppViewportRenderLayer bg:
                    Rgba32[] plane = SnesBgTilemapRenderer.Render2Bpp(memory.Vram, memory.Cgram,
                        bg.TilemapWord, bg.CharacterWord, rowCount: 32,
                        transparentColorZero: bg.TransparentColorZero, priority: bg.Priority);
                    for (int y = 0; y < SnesPpuLayout.ScreenHeightPixels; y++)
                        SnesLayerCompositor.Composite(output.AsSpan(y * SnesPpuLayout.ScreenWidthPixels,
                            SnesPpuLayout.ScreenWidthPixels), plane.AsSpan(
                            ((y + bg.VerticalScroll) & 255) * SnesPpuLayout.ScreenWidthPixels,
                            SnesPpuLayout.ScreenWidthPixels));
                    break;
                case FixedColorAddRenderLayer color:
                    for (int pixel = 0; pixel < output.Length; pixel++)
                    {
                        Rgba32 before = output[pixel];
                        output[pixel] = new(AddFixed(before.R, color.Red), AddFixed(before.G, color.Green),
                            AddFixed(before.B, color.Blue), 255);
                    }
                    break;
                case Mode7RenderLayer mode7:
                    Mode7RenderRegisters m = mode7.Registers;
                    Rgba32[] mode7Pixels = SnesMode7Renderer.RenderViewport(
                        memory.Vram, memory.Cgram, m.MatrixA, m.MatrixB, m.MatrixC, m.MatrixD,
                        m.CenterX, m.CenterY, m.HorizontalOffset, m.VerticalOffset,
                        fillOutsideWithCharacterZero: m.FillOutsideWithCharacterZero, wrapOutsideMap: m.WrapOutsideMap);
                    if (mode7.AddBg1Subscreen)
                    {
                        var palettes = new byte[output.Length];
                        SnesObjRenderer.CompositeUnfiltered(memory.Oam, memory.Vram, memory.Cgram,
                            snapshot.ObjectSelection, new Rgba32[output.Length], palettes);
                        for (int i = 0; i < output.Length; i++)
                        {
                            Rgba32 sub = mode7Pixels[i];
                            bool objWins = objects.Pixels[i].A != 0 && (objects.Priorities[i] != 0 || sub.A == 0);
                            Rgba32 main = objWins ? objects.Pixels[i] : sub.A != 0 ? sub : output[i];
                            if (sub.A != 0 && (!objWins || palettes[i] >= 4))
                                main = new Rgba32(AddFixed(main.R, (byte)(sub.R >> 3)),
                                    AddFixed(main.G, (byte)(sub.G >> 3)), AddFixed(main.B, (byte)(sub.B >> 3)));
                            output[i] = main;
                        }
                        break;
                    }
                    if (mode7.SubtractObjSubscreen)
                        for (int i = 0; i < mode7Pixels.Length; i++)
                        {
                            if (mode7Pixels[i].A == 0 || objects.Pixels[i].A == 0) continue;
                            Rgba32 main = mode7Pixels[i], sub = objects.Pixels[i];
                            mode7Pixels[i] = new Rgba32(Subtract(main.R, sub.R), Subtract(main.G, sub.G), Subtract(main.B, sub.B));
                        }
                    SnesLayerCompositor.Composite(output, mode7Pixels);
                    break;
                case ObjRenderLayer objLayer:
                    if (objLayer.FixedColor is { } fixedObj)
                    {
                        var palettes = new byte[output.Length];
                        var coloredObjects = new Rgba32[output.Length];
                        SnesObjRenderer.CompositeUnfiltered(memory.Oam, memory.Vram, memory.Cgram,
                            snapshot.ObjectSelection, coloredObjects, palettes);
                        for (int i = 0; i < output.Length; i++)
                        {
                            Rgba32 pixel = coloredObjects[i];
                            if (pixel.A == 0) continue;
                            output[i] = palettes[i] >= 4 ? new Rgba32(AddFixed(pixel.R, fixedObj.Red),
                                AddFixed(pixel.G, fixedObj.Green), AddFixed(pixel.B, fixedObj.Blue)) : pixel;
                        }
                    }
                    else if (objLayer.AddToScreen) SnesLayerCompositor.AddSubscreen(output, objects.Pixels);
                    else SnesLayerCompositor.Composite(output, objects.Pixels);
                    break;
                case ObjPriorityRenderLayer obj:
                    byte[]? priorityPalettes = null;
                    if (obj.FixedColor is not null)
                    {
                        priorityPalettes = new byte[output.Length];
                        SnesObjRenderer.CompositeUnfiltered(memory.Oam, memory.Vram, memory.Cgram,
                            snapshot.ObjectSelection, new Rgba32[output.Length], priorityPalettes);
                    }
                    for (int pixel = 0; pixel < output.Length; pixel++)
                        if (objects.Priorities[pixel] == obj.Priority)
                        {
                            Rgba32 value = objects.Pixels[pixel];
                            output[pixel] = obj.FixedColor is { } priorityWhite && priorityPalettes![pixel] >= 4
                                ? new Rgba32(AddFixed(value.R, priorityWhite.Red), AddFixed(value.G, priorityWhite.Green),
                                    AddFixed(value.B, priorityWhite.Blue)) : value;
                        }
                    break;
                case Bg4BppRenderLayer bg:
                    SnesBgTilemapRenderer.Composite4BppViewport(output, memory.Vram, memory.Cgram,
                        bg.TilemapWord, bg.CharacterWord, bg.HorizontalScroll, bg.VerticalScroll,
                        SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels,
                        bg.MapWidthTiles, bg.MapHeightTiles, priority: bg.Priority);
                    break;
                case Bg2BppRenderLayer bg:
                    SnesLayerCompositor.Composite(output, SnesBgTilemapRenderer.Render2Bpp(
                        memory.Vram, memory.Cgram, bg.TilemapWord, bg.CharacterWord,
                        rowCount: bg.RowCount, transparentColorZero: true, priority: bg.Priority));
                    break;
                default:
                    throw new InvalidDataException($"Unsupported render layer {layer.GetType().Name}.");
            }
        }
        MasterBrightnessFilter.Apply(output, snapshot.Brightness);
        return output;
    }

    private static byte AddFixed(byte component, byte addend)
    {
        int reduced = (component * 31 + 127) / 255;
        int sum = Math.Min(31, reduced + addend);
        return (byte)((sum << 3) | (sum >> 2));
    }

    private static byte Subtract(byte main, byte sub)
    {
        int difference = Math.Max(0, (main >> 3) - (sub >> 3));
        return (byte)((difference << 3) | (difference >> 2));
    }
}
