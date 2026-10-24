// The sprite layer (#1408): each whole-frame sprite draw is an instanced quad over its image in the image texture,
// drawn back to front so the frontmost draw's opaque pixel is the one the layer keeps. The layer holds, per pixel, the
// colour shown and, below it, the pixel's priority against the backgrounds with whether it blends above that; a pixel
// no draw covers keeps the cleared "none" value. D3D11SpriteDrawBuffer packs the records.
StructuredBuffer<uint> SpriteDraws : register(t1);
// Each texel: the image's pre-coloured pixel, then its ink (D3D11ImageAtlas): its ink index, priority above it and its
// part row above that. Compiled apart from Tiles.hlsl, which declares the same texture.
Texture2D<uint2> Images : register(t4);
static const uint SpriteRecordWords = 5;
static const uint SpriteFlipX = 1, SpriteFlipY = 2, SpriteRecolour = 512;
static const uint SpriteRowColours = 16, SpriteFollowsPartRows = 65536;
// The ink row from which a sprite's pixels blend.
static const uint FirstBlendingRow = 4;
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
    uint2 texel = Images.Load(int3(uint2(place & 65535, place >> 16) + source, 0));
    uint ink = texel.y & 255;
    uint priority = (texel.y >> 8) & 3;
    if ((flags & SpriteRecolour) != 0)
    {
        if (ink == 0) discard;
        // A pixel takes its ink row's colours, or its part row's where the draw follows part rows.
        uint colours = SpriteDraws[record + 4];
        uint header = SpriteDraws[colours];
        uint key = (header & SpriteFollowsPartRows) != 0 ? (texel.y >> 10) & 15 : ink >> 4;
        uint blends = ((header >> key) & 1) != 0 ? SpriteBlends : 0;
        return uint2(SpriteDraws[colours + 1 + key * SpriteRowColours + (ink & 15)], priority | blends);
    }
    if ((texel.x >> 24) == 0) discard;
    return uint2(texel.x | 0xff000000, priority | ((ink >> 4) >= FirstBlendingRow ? SpriteBlends : 0));
}
