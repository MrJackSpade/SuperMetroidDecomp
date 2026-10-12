using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private void DispatchMode7Gameplay(Mode7GameplayRenderLayer layer)
    {
        // Clipping is in physical scanlines. Never translate the projection origin
        // to the start of the band: that changes the matrix/scroll relationship.
        int floorStart = layer.Floor?.FirstScanline ?? 224;
        DispatchMode7(layer.Registers, layer.HudScanlines, floorStart);
        DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { FirstScanline = (uint)layer.HudScanlines, EndScanline = (uint)floorStart });
        if (layer.Floor is { } floor)
        {
            // Only the middle band sampled Mode 7. Transparent floor pixels retain
            // backdrop; the same resolved OAM winner is used in every priority pass.
            Objects(0); Objects(1);
            Background(false);
            Objects(2);
            Background(true);
            Objects(3);

            void Objects(uint priority) => DispatchTile(D3D11TileOperation.InsertObj, new D3D11TileDispatch { Priority = priority + 1, FirstScanline = (uint)floorStart });
            void Background(bool high) => DispatchTile(D3D11TileOperation.Bg4, new D3D11TileDispatch { Map = floor.TilemapWord, Characters = floor.CharacterWord, Scroll = floor.Scroll.AsBgPlaneScroll(), MapSize = floor.MapSize, Priority = Priority(high), FirstScanline = (uint)floorStart });
        }
        // HUD color zero is opaque, and OBJ never overlays this band.
        if (layer.HudScanlines != 0)
            DispatchTile(D3D11TileOperation.Bg2, new D3D11TileDispatch { Map = layer.HudTilemapWord, Characters = layer.HudCharacterWord, TransparentZero = 0, EndScanline = (uint)layer.HudScanlines });
    }
}
