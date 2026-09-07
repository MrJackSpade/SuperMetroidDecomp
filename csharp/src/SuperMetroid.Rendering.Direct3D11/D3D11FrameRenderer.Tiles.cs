using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe Rgba32[] RenderLayersForReadback(RenderFrameSnapshot packet, LayeredRenderSnapshot scene)
    {
        // Reject unimplemented operations before changing GPU state. No CPU fallback.
        foreach (RenderLayer layer in scene.Layers)
            if (layer is not (Bg4BppRenderLayer or Bg2BppRenderLayer or Bg2BppViewportRenderLayer or FixedColorAddRenderLayer or ObjRenderLayer or ObjPriorityRenderLayer or Mode7RenderLayer or Mode7GameplayRenderLayer or ScanlineColorAddRenderLayer or Bg2BppColorMathRenderLayer))
                throw new NotSupportedException($"GPU layer {layer.GetType().Name} is not implemented yet.");
        var memory = new uint[D3D11ShaderLayout.PpuMemoryWords];
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Vram).CopyTo(memory);
        for (int i = 0; i < scene.Memory.Cgram.Length; i++) memory[D3D11ShaderLayout.VramPackedWords + i] = scene.Memory.Cgram[i];
        MemoryMarshal.Cast<byte, uint>(scene.Memory.Oam).CopyTo(memory.AsSpan(D3D11ShaderLayout.OamPackedWordOffset));
        fixed (uint* source = memory) owner.Context.UpdateSubresource(memoryBuffer, 0, null, (nint)source, 0, 0);
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, memoryView);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
        DispatchTile(D3D11TileOperation.Backdrop);
        bool needsObjects = false;
        foreach (RenderLayer layer in scene.Layers) needsObjects |= layer is ObjRenderLayer or ObjPriorityRenderLayer or Mode7GameplayRenderLayer;
        if (needsObjects) DispatchTile(D3D11TileOperation.ResolveObj, objectCount: (uint)scene.Memory.ModeledSpriteCount,
            objectSelection: scene.ObjectSelection);
        foreach (RenderLayer layer in scene.Layers)
        {
            switch (layer)
            {
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
                    DispatchMode7(mode7.Registers);
                    break;
                case ObjRenderLayer:
                    DispatchTile(D3D11TileOperation.InsertObj);
                    break;
                case ObjPriorityRenderLayer obj:
                    DispatchTile(D3D11TileOperation.InsertObj, priority: (uint)obj.Priority + 1);
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
        DispatchTile(D3D11TileOperation.Brightness, level: scene.Brightness);
        foreach (byte level in packet.BrightnessPasses) DispatchTile(D3D11TileOperation.Brightness, level: level);
        return Readback(packet.Width, packet.Height);
    }

    private unsafe void DispatchTile(D3D11TileOperation operation, uint map = 0, uint characters = 0,
        uint x = 0, uint y = 0, uint width = 32, uint height = 32, uint priority = 0,
        uint transparentZero = 1, uint level = 15, uint red = 0, uint green = 0, uint blue = 0,
        uint objectCount = 0, uint objectSelection = 0, uint firstScanline = 0, uint endScanline = 224)
    {
        // Upload the complete allocated cbuffer, so UpdateSubresource cannot read past
        // a short managed array. The trailing header words supply scanline clipping.
        var data = new uint[D3D11ShaderLayout.SolidConstantWords];
        data[0] = (uint)operation; data[1] = map; data[2] = characters; data[3] = x;
        data[4] = y; data[5] = width; data[6] = height; data[7] = priority;
        data[8] = transparentZero; data[9] = level; data[10] = red; data[11] = green; data[12] = blue;
        data[13] = objectCount; data[14] = objectSelection;
        data[25] = firstScanline; data[26] = endScanline;
        fixed (uint* source = data) owner.Context.UpdateSubresource(constants, 0, null, (nint)source, 0, 0);
        owner.Context.Dispatch(32, 28, 1);
    }

    private static uint Priority(bool? priority) => priority is null ? 0u : priority.Value ? 2u : 1u;

    private unsafe void DispatchMode7(Mode7RenderRegisters registers, int firstScanline = 0, int endScanline = 224)
    {
        // Preserve signed register values, including negative products and arithmetic
        // right shifts. No projected coordinates or raster pixels are uploaded.
        var data = new int[D3D11ShaderLayout.SolidConstantWords];
        data[0] = (int)D3D11TileOperation.Mode7;
        data[16] = registers.MatrixA; data[17] = registers.MatrixB;
        data[18] = registers.MatrixC; data[19] = registers.MatrixD;
        data[20] = registers.CenterX; data[21] = registers.CenterY;
        data[22] = registers.HorizontalOffset; data[23] = registers.VerticalOffset;
        data[24] = registers.FillOutsideWithCharacterZero ? 1 : 0;
        data[25] = firstScanline; data[26] = endScanline;
        fixed (int* source = data) owner.Context.UpdateSubresource(constants, 0, null, (nint)source, 0, 0);
        owner.Context.Dispatch(32, 28, 1);
    }
}
