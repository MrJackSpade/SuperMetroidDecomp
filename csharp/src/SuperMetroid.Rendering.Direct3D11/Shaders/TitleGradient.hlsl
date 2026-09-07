// Share memory/OAM sampling definitions, but compile a separate entry point.
// Keeping this operation in Tiles.Main changed FXC /O3 output for unrelated
// BG add/subtract dispatches; the color-window and native-title tests cover both.
#include "Tiles.hlsl"

[numthreads(8, 8, 1)]
void TitleGradientMain(uint3 id : SV_DispatchThreadID)
{
    if (id.x >= 256 || id.y >= 224) return;
    uint palette;
    uint2 obj = ResolveObjectWithPalette(id.xy, palette);
    uint4 band = ScanlineParameters[id.y];
    bool enabled = palette == 255 ? (band.w & 1) != 0 : palette >= 4 && (band.w & 16) != 0;
    if (enabled)
    {
        uint color = Output[id.xy];
        int3 source = int3(Unpack(color) >> 3);
        int3 result = (band.w & 128) != 0 ? max(0, source - int3(band.xyz)) : min(31, source + int3(band.xyz));
        Output[id.xy] = Pack(uint3((result << 3) | (result >> 2)), color >> 24);
    }
}
