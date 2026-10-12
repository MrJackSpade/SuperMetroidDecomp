using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe void DrawLayers(RenderFrameSnapshot packet, LayeredRenderSnapshot scene,
        ReadOnlySpan<Core.Frontend.TitleGradientLine> gradient = default)
    {
        // Reject unimplemented operations before changing GPU state. No CPU fallback.
        foreach (RenderLayer layer in scene.Layers)
            if (layer is not (Bg4BppRenderLayer or Bg2BppRenderLayer or Bg2BppViewportRenderLayer or FixedColorAddRenderLayer or ObjRenderLayer or ObjPriorityRenderLayer or Mode7RenderLayer or Mode7GameplayRenderLayer or ScanlineColorAddRenderLayer or Bg2BppColorMathRenderLayer or BgSubscreenAddRenderLayer or MessageBoxRenderLayer or OrdinaryGameplayRenderLayer or WindowedSceneRenderLayer or GameplayColorMathRenderLayer))
                throw new NotSupportedException($"GPU layer {layer.GetType().Name} is not implemented yet.");
        var memory = memoryUpload;
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Vram).CopyTo(memory);
        for (int i = 0; i < scene.Memory.Cgram.Length; i++) memory[D3D11ShaderLayout.VramPackedWords + i] = scene.Memory.Cgram[i].ToWord();
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Oam).CopyTo(memory.AsSpan(D3D11ShaderLayout.OamPackedWordOffset));
        fixed (uint* source = memory) UploadBuffer(memoryBuffer, (nint)source, memory.Length * sizeof(uint));
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, memoryView);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
        DispatchTile(D3D11TileOperation.Backdrop, new D3D11TileDispatch());
        bool needsObjects = false;
        foreach (RenderLayer layer in scene.Layers) needsObjects |= layer is ObjRenderLayer or ObjPriorityRenderLayer or Mode7GameplayRenderLayer or OrdinaryGameplayRenderLayer or Mode7RenderLayer { SubtractObjSubscreen: true } or BgSubscreenAddRenderLayer { IncludeObjects: true };
        if (needsObjects) DispatchTile(D3D11TileOperation.ResolveObj, new D3D11TileDispatch { ObjectCount = (uint)scene.Memory.ModeledSpriteCount, ObjectSelection = scene.ObjectSelection });
        foreach (RenderLayer layer in scene.Layers)
        {
            switch (layer)
            {
                case GameplayColorMathRenderLayer gameplayXray:
                    DispatchGameplayColorMath(scene, gameplayXray);
                    break;
                case WindowedSceneRenderLayer window:
                    DrawWindow(packet, window);
                    break;
                case OrdinaryGameplayRenderLayer ordinary:
                    DispatchOrdinaryGameplay(ordinary);
                    break;
                case MessageBoxRenderLayer message:
                    DispatchMessage(message);
                    break;
                case BgSubscreenAddRenderLayer sub:
                    DispatchSubscreen(sub, (uint)scene.Memory.ModeledSpriteCount, scene.ObjectSelection);
                    break;
                case Bg2BppColorMathRenderLayer math:
                    DispatchBackgroundMath(math);
                    break;
                case ScanlineColorAddRenderLayer windows:
                    DispatchColorWindows(windows);
                    break;
                case Mode7GameplayRenderLayer gameplay:
                    DispatchMode7Gameplay(gameplay);
                    break;
                case Mode7RenderLayer mode7:
                    DispatchMode7(mode7.Registers, subtractObj: mode7.SubtractObjSubscreen, addBg1: mode7.AddBg1Subscreen,
                        objectCount: scene.Memory.ModeledSpriteCount, objectSelection: scene.ObjectSelection);
                    break;
                case ObjRenderLayer objLayer:
                    DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Red = objLayer.AddToScreen ? 1u : 0u, Green = objLayer.FixedColor is not null ? 1u : 0u, Blue = objLayer.FixedColor is { } color ? (uint)(color.Red | color.Green << 5 | color.Blue << 10) : 0, ObjectCount = (uint)scene.Memory.ModeledSpriteCount, ObjectSelection = scene.ObjectSelection });
                    break;
                case ObjPriorityRenderLayer obj:
                    DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Priority = (uint)obj.Priority + 1, Green = obj.FixedColor is not null ? 1u : 0u, Blue = obj.FixedColor is { } priorityColor ? (uint)(priorityColor.Red | priorityColor.Green << 5 | priorityColor.Blue << 10) : 0, ObjectCount = (uint)scene.Memory.ModeledSpriteCount, ObjectSelection = scene.ObjectSelection });
                    break;
                case Bg4BppRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg4, new D3D11TileDispatch { Map = bg.TilemapWord, Characters = bg.CharacterWord, Scroll = bg.Scroll, MapSize = bg.MapSize, Priority = Priority(bg.Priority) });
                    break;
                case Bg2BppRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { Map = bg.TilemapWord, Characters = bg.CharacterWord, Priority = Priority(bg.Priority) });
                    break;
                case Bg2BppViewportRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { Map = bg.TilemapWord, Characters = bg.CharacterWord, Scroll = new Position16<BgPlanePixels>(0, bg.VerticalScroll), Priority = Priority(bg.Priority), TransparentZero = bg.TransparentColorZero ? 1u : 0u });
                    break;
                case FixedColorAddRenderLayer add:
                    DispatchTile(D3D11TileOperation.FixedAdd, new D3D11TileDispatch { Red = add.Red, Green = add.Green, Blue = add.Blue });
                    break;
            }
        }
        if (!gradient.IsEmpty) DispatchTitleGradient(gradient, scene.Memory.ModeledSpriteCount, scene.ObjectSelection);
        DispatchTile(D3D11TileOperation.Brightness, new D3D11TileDispatch { Level = scene.Brightness });
        foreach (byte level in packet.BrightnessPasses) DispatchTile(D3D11TileOperation.Brightness, new D3D11TileDispatch { Level = level });
    }

    private unsafe void DispatchTile(D3D11TileOperation operation, D3D11TileDispatch tile)
    {
        // Upload the complete allocated cbuffer, so UpdateSubresource cannot read past
        // a short managed array. The trailing header words supply scanline clipping.
        // The tile shader's header words three through six are the BG scroll and map size;
        // unmapped operations keep the shader's default 32x32-cell plane.
        Extent16<TilemapCells> size = tile.MapSize ?? new Extent16<TilemapCells>(32, 32);
        var data = ClearUploadConstants();
        data[0] = (uint)operation; data[1] = tile.Map; data[2] = tile.Characters; data[3] = tile.Scroll.X;
        data[4] = tile.Scroll.Y; data[5] = size.X; data[6] = size.Y; data[7] = tile.Priority;
        data[8] = tile.TransparentZero; data[9] = tile.Level; data[10] = tile.Red; data[11] = tile.Green; data[12] = tile.Blue;
        data[13] = tile.ObjectCount; data[14] = tile.ObjectSelection;
        data[25] = tile.FirstScanline; data[26] = tile.EndScanline;
        SetWindowConstants(data, tile.Windows, tile.WindowMask, tile.WindowTarget);
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    private static uint Priority(bool? priority) => priority is null ? 0u : priority.Value ? 2u : 1u;

    private unsafe void DispatchMode7(Mode7RenderRegisters registers, int firstScanline = 0, int endScanline = 224,
        bool subtractObj = false, bool addBg1 = false, int objectCount = 0, byte objectSelection = 0)
    {
        // Preserve signed register values, including negative products and arithmetic
        // right shifts. No projected coordinates or raster pixels are uploaded.
        var data = MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = (int)D3D11TileOperation.Mode7;
        data[13] = objectCount; data[14] = objectSelection;
        data[16] = registers.MatrixA; data[17] = registers.MatrixB;
        data[18] = registers.MatrixC; data[19] = registers.MatrixD;
        data[20] = registers.Center.X; data[21] = registers.Center.Y;
        data[22] = registers.Scroll.X; data[23] = registers.Scroll.Y;
        data[24] = (int)Mode7OverflowPolicy.FromRegisters(registers);
        data[25] = firstScanline; data[26] = endScanline;
        data[27] = (int)(addBg1 ? Mode7ColorMathOperation.AddBg1
            : subtractObj ? Mode7ColorMathOperation.SubtractObj : Mode7ColorMathOperation.None);
        fixed (int* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }
}
