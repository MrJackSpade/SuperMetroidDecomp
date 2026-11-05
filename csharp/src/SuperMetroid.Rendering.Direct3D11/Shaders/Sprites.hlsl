// The sprite layer (#1408): each whole-frame sprite draw is an instanced quad over its image in the image texture,
// drawn back to front so the frontmost draw's opaque pixel is the one the layer keeps. The layer holds, per pixel, the
// colour shown and, below it, the pixel's priority against the backgrounds with whether it blends above that; a pixel
// no draw covers keeps the cleared "none" value. D3D11SpriteDrawBuffer packs the records.
StructuredBuffer<uint> SpriteDraws : register(t1);
// Each texel: the image's pre-coloured pixel, then its ink (D3D11ImageAtlas): its ink index with its priority above it.
// Compiled apart from Tiles.hlsl, which declares the same texture.
Texture2D<uint2> Images : register(t4);
static const uint SpriteRecordWords = 6;
static const uint SpriteFlipX = 1, SpriteFlipY = 2, SpriteRecolour = 512;
// A rotting corpse's row word naming a row its rot has cleared (#1531), which draws nothing.
static const uint SpriteClearedRow = 0xffffffffu;
// A record flag marking a draw whose colouring's set takes part in colour math, which the cartridge ORs into every
// part's palette, so each of its pixels blends whatever its ink's own group (#1512).
static const uint SpriteColourMath = 1024;
// The inks of one sprite colour group.
static const uint SpriteGroupInks = 16;
// The first of a sprite sheet's colour groups whose pixels take part in colour math, as every later group's do (#1514
// owns colour math): an ink's group is its index over SpriteGroupInks.
static const uint FirstColourMathGroup = 4;
// A layer pixel's flag marking a sprite pixel that blends.
static const uint SpriteBlends = 16;

struct SpriteVertex
{
    float4 position : SV_Position;
    nointerpolation uint record : RECORD;
};

SpriteVertex SpriteQuad(uint corner : SV_VertexID, uint instance : SV_InstanceID)
{
    // Instance zero is the backmost draw, the last one emitted.
    uint record = 1 + (SpriteDraws[0] - 1 - instance) * SpriteRecordWords;
    uint position = SpriteDraws[record];
    uint extent = SpriteDraws[record + 1];
    float2 origin = float2(((int)(position << 16)) >> 16, ((int)position) >> 16);
    float2 size = float2(extent & 65535, extent >> 16);
    float2 pixel = origin + size * float2(corner & 1, corner >> 1);
    SpriteVertex vertex;
    vertex.position = float4(pixel / float2(256, 224) * float2(2, -2) + float2(-1, 1), 0, 1);
    vertex.record = record;
    return vertex;
}

uint2 SpritePixel(SpriteVertex vertex) : SV_Target
{
    uint record = vertex.record;
    uint position = SpriteDraws[record];
    uint extent = SpriteDraws[record + 1];
    uint flags = SpriteDraws[record + 2];
    uint place = SpriteDraws[record + 3];
    int2 origin = int2(((int)(position << 16)) >> 16, ((int)position) >> 16);
    uint2 size = uint2(extent & 65535, extent >> 16);
    uint2 delta = uint2(int2(vertex.position.xy) - origin);
    uint2 source = uint2((flags & SpriteFlipX) != 0 ? size.x - 1 - delta.x : delta.x,
        (flags & SpriteFlipY) != 0 ? size.y - 1 - delta.y : delta.y);
    // A rotting corpse's image row shows the image row its rot has moved there, or nothing once the rot has cleared it
    // (#1531): one lookup per row, in the words the record's sixth word starts.
    uint rows = SpriteDraws[record + 5];
    if (rows != 0)
    {
        uint shown = SpriteDraws[rows + source.y];
        if (shown == SpriteClearedRow) discard;
        source.y = shown;
    }
    uint2 texel = Images.Load(int3(uint2(place & 65535, place >> 16) + source, 0));
    uint ink = texel.y & 255;
    uint priority = (texel.y >> 8) & 3;
    uint blends = (ink / SpriteGroupInks) >= FirstColourMathGroup || (flags & SpriteColourMath) != 0 ? SpriteBlends : 0;
    if ((flags & SpriteRecolour) != 0)
    {
        if (ink == 0) discard;
        // A shaded draw's pixel shows its ink's colour under the draw's shading (#1512).
        return uint2(SpriteDraws[SpriteDraws[record + 4] + ink], priority | blends);
    }
    if ((texel.x >> 24) == 0) discard;
    return uint2(texel.x | 0xff000000, priority | blends);
}
