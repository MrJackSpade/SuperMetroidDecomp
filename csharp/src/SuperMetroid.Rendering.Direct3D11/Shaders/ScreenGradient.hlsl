// Shares Tiles.hlsl's resources and the sprite layer, but compiles a separate entry point.
// Keeping this operation in Tiles.Main changed FXC /O3 output for unrelated
// BG add/subtract dispatches; the color-window and native-title tests cover both.
#include "Tiles.hlsl"

// A gradient (ScreenGradientLayer, #1468) draws the lines FirstScanline up to EndScanline. Its stops are the first
// Level rows of ScanlineParameters, top to bottom: each row's red, green and blue, then its blend in the low eight bits
// of w with the line its band begins on above them. A line takes the last stop beginning on or above it.
static const uint StopLineShift = 8, StopBlendMask = 255;

[numthreads(8, 8, 1)]
void ScreenGradientMain(uint3 id : SV_DispatchThreadID)
{
    if (id.x >= 256 || id.y < FirstScanline || id.y >= EndScanline) return;
    uint4 band = ScanlineParameters[0];
    [loop] for (uint stop = 1; stop < Level; stop++)
    {
        uint4 next = ScanlineParameters[stop];
        if ((next.w >> StopLineShift) > id.y) break;
        band = next;
    }
    uint blend = band.w & StopBlendMask;
    bool blends;
    uint2 obj = SpriteAt(id.xy, blends);
    bool enabled = obj.y == NoSprite ? (blend & 1) != 0 : blends && (blend & 16) != 0;
    if (enabled)
    {
        uint color = Output[id.xy];
        int3 source = int3(Unpack(color) >> 3);
        int3 result = (blend & 128) != 0 ? max(0, source - int3(band.xyz)) : min(31, source + int3(band.xyz));
        Output[id.xy] = Pack(uint3((result << 3) | (result >> 2)), color >> 24);
    }
}
