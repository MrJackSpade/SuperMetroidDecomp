using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe void DispatchXrayGameplay(LayeredRenderSnapshot scene, XrayGameplayRenderLayer layer)
    {
        var r = layer.Gameplay.Registers;
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.XrayGameplay;
        // Xray.hlsli reuses the tile header for BG1/BG2 registers and CGADSUB.
        data[1] = r.Bg1CharacterWord; data[2] = r.Bg2CharacterWord;
        data[3] = r.Bg1X; data[4] = r.Bg1Y;
        data[5] = (uint)r.Bg2WidthTiles; data[6] = (uint)r.Bg2HeightTiles;
        data[7] = (uint)r.MainScreenLayers; data[8] = layer.RevealBlocks ? 1u : 0u;
        data[9] = (uint)layer.ColorMath;
        data[10] = layer.FixedRed; data[11] = layer.FixedGreen; data[12] = layer.FixedBlue;
        data[13] = (uint)scene.Memory.ModeledSpriteCount; data[14] = scene.ObjectSelection;
        data[16] = r.Bg2TilemapWord; data[17] = r.HudCharacterWord;
        data[22] = layer.AddSubscreen ? 1u : 0u;
        data[26] = 224;
        if (layer.Subscreen is { } sub)
        {
            data[15] = 1;
            data[18] = sub.TilemapWord; data[19] = sub.CharacterWord;
            data[20] = (uint)sub.MapHeightTiles; data[21] = (uint)sub.FirstScanline;
        }
        for (int y = 32; y < 224; y++)
        {
            int offset = D3D11ShaderLayout.ScanlineParametersWordOffset + y * 4, line = y - 32;
            data[offset] = layer.Gameplay.HorizontalScrolls.IsEmpty ? r.Bg2X : layer.Gameplay.HorizontalScrolls[line];
            data[offset + 1] = layer.Gameplay.VerticalScrolls.IsEmpty ? r.Bg2Y : layer.Gameplay.VerticalScrolls[line];
            data[offset + 2] = (uint)(layer.Lines[y].Left | layer.Lines[y].Right << 8);
            if (layer.Subscreen is { } bg3)
                data[offset + 3] = (uint)(bg3.Scrolls[y].X | bg3.Scrolls[y].Y << 16);
        }
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
    }
}
