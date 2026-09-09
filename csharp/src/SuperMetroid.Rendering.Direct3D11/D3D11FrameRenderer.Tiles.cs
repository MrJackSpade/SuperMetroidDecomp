using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe void DrawLayers(RenderFrameSnapshot packet, LayeredRenderSnapshot scene,
        ReadOnlySpan<Core.Frontend.TitleGradientLine> gradient = default)
    {
        // Reject unimplemented operations before changing GPU state. No CPU fallback.
        foreach (RenderLayer layer in scene.Layers)
            if (layer is not (Bg4BppRenderLayer or Bg2BppRenderLayer or Bg2BppViewportRenderLayer or FixedColorAddRenderLayer or ObjRenderLayer or ObjPriorityRenderLayer or Mode7RenderLayer or Mode7GameplayRenderLayer or ScanlineColorAddRenderLayer or Bg2BppColorMathRenderLayer or BgSubscreenAddRenderLayer or MessageBoxRenderLayer or OrdinaryGameplayRenderLayer or WindowedSceneRenderLayer or XrayWindowRenderLayer or XrayGameplayRenderLayer))
                throw new NotSupportedException($"GPU layer {layer.GetType().Name} is not implemented yet.");
        var memory = memoryUpload;
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Vram).CopyTo(memory);
        for (int i = 0; i < scene.Memory.Cgram.Length; i++) memory[D3D11ShaderLayout.VramPackedWords + i] = scene.Memory.Cgram[i];
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Oam).CopyTo(memory.AsSpan(D3D11ShaderLayout.OamPackedWordOffset));
        fixed (uint* source = memory) UploadBuffer(memoryBuffer, (nint)source, memory.Length * sizeof(uint));
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, memoryView);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
        DispatchTile(D3D11TileOperation.Backdrop);
        bool needsObjects = false;
        foreach (RenderLayer layer in scene.Layers) needsObjects |= layer is ObjRenderLayer or ObjPriorityRenderLayer or Mode7GameplayRenderLayer or OrdinaryGameplayRenderLayer or Mode7RenderLayer { SubtractObjSubscreen: true } or BgSubscreenAddRenderLayer { IncludeObjects: true };
        if (needsObjects) DispatchTile(D3D11TileOperation.ResolveObj, objectCount: (uint)scene.Memory.ModeledSpriteCount,
            objectSelection: scene.ObjectSelection);
        foreach (RenderLayer layer in scene.Layers)
        {
            switch (layer)
            {
                case XrayGameplayRenderLayer gameplayXray:
                    DispatchXrayGameplay(scene, gameplayXray);
                    break;
                case XrayWindowRenderLayer xray:
                    DrawXrayWindow(packet, xray);
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
                    DispatchMode7(mode7.Registers, subtractObj: mode7.SubtractObjSubscreen);
                    break;
                case ObjRenderLayer objLayer:
                    DispatchTile(D3D11TileOperation.InsertObj, red: objLayer.AddToScreen ? 1u : 0u,
                        green: objLayer.FixedColor is not null ? 1u : 0u,
                        blue: objLayer.FixedColor is { } color ? (uint)(color.Red | color.Green << 5 | color.Blue << 10) : 0,
                        objectCount: (uint)scene.Memory.ModeledSpriteCount, objectSelection: scene.ObjectSelection);
                    break;
                case ObjPriorityRenderLayer obj:
                    DispatchTile(D3D11TileOperation.InsertObj, priority: (uint)obj.Priority + 1,
                        green: obj.FixedColor is not null ? 1u : 0u,
                        blue: obj.FixedColor is { } priorityColor ? (uint)(priorityColor.Red | priorityColor.Green << 5 | priorityColor.Blue << 10) : 0,
                        objectCount: (uint)scene.Memory.ModeledSpriteCount, objectSelection: scene.ObjectSelection);
                    break;
                case Bg4BppRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg4, bg.TilemapWord, bg.CharacterWord,
                        bg.HorizontalScroll, bg.VerticalScroll, (uint)bg.MapWidthTiles, (uint)bg.MapHeightTiles, Priority(bg.Priority));
                    break;
                case Bg2BppRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg2, bg.TilemapWord, bg.CharacterWord, priority: Priority(bg.Priority));
                    break;
                case Bg2BppViewportRenderLayer bg:
                    DispatchTile(D3D11TileOperation.Bg2, bg.TilemapWord, bg.CharacterWord,
                        y: bg.VerticalScroll, priority: Priority(bg.Priority), transparentZero: bg.TransparentColorZero ? 1u : 0u);
                    break;
                case FixedColorAddRenderLayer add:
                    DispatchTile(D3D11TileOperation.FixedAdd, red: add.Red, green: add.Green, blue: add.Blue);
                    break;
            }
        }
        if (!gradient.IsEmpty) DispatchTitleGradient(gradient, scene.Memory.ModeledSpriteCount, scene.ObjectSelection);
        DispatchTile(D3D11TileOperation.Brightness, level: scene.Brightness);
        foreach (byte level in packet.BrightnessPasses) DispatchTile(D3D11TileOperation.Brightness, level: level);
    }

    private unsafe void DispatchTile(D3D11TileOperation operation, uint map = 0, uint characters = 0,
        uint x = 0, uint y = 0, uint width = 32, uint height = 32, uint priority = 0,
        uint transparentZero = 1, uint level = 15, uint red = 0, uint green = 0, uint blue = 0,
        uint objectCount = 0, uint objectSelection = 0, uint firstScanline = 0, uint endScanline = 224)
    {
        // Upload the complete allocated cbuffer, so UpdateSubresource cannot read past
        // a short managed array. The trailing header words supply scanline clipping.
        var data = ClearUploadConstants();
        data[0] = (uint)operation; data[1] = map; data[2] = characters; data[3] = x;
        data[4] = y; data[5] = width; data[6] = height; data[7] = priority;
        data[8] = transparentZero; data[9] = level; data[10] = red; data[11] = green; data[12] = blue;
        data[13] = objectCount; data[14] = objectSelection;
        data[25] = firstScanline; data[26] = endScanline;
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }

    private static uint Priority(bool? priority) => priority is null ? 0u : priority.Value ? 2u : 1u;

    private unsafe void DispatchMode7(Mode7RenderRegisters registers, int firstScanline = 0, int endScanline = 224, bool subtractObj = false)
    {
        // Preserve signed register values, including negative products and arithmetic
        // right shifts. No projected coordinates or raster pixels are uploaded.
        var data = MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = (int)D3D11TileOperation.Mode7;
        data[16] = registers.MatrixA; data[17] = registers.MatrixB;
        data[18] = registers.MatrixC; data[19] = registers.MatrixD;
        data[20] = registers.CenterX; data[21] = registers.CenterY;
        data[22] = registers.HorizontalOffset; data[23] = registers.VerticalOffset;
        data[24] = registers.FillOutsideWithCharacterZero ? 1 : 0;
        data[25] = firstScanline; data[26] = endScanline;
        data[27] = subtractObj ? 1 : 0;
        fixed (int* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }
}
