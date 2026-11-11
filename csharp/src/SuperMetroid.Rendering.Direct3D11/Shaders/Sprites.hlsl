// The sprite layer (#1408): each whole-frame sprite draw is an instanced quad over its image in the image texture,
// drawn back to front so the frontmost draw's opaque pixel is the one the layer keeps. The layer holds, per pixel, the
// colour shown and, below it, the draw's depth among the room's layers (#1514) with whether the pixel blends above that;
// a pixel no draw covers keeps the cleared "none" value. D3D11SpriteDrawBuffer packs the records, a gameplay frame's
// draws by depth from the front back, so the frontmost draw is the one at the frontmost depth.
StructuredBuffer<uint> SpriteDraws : register(t1);
// The run of draws this pass draws (#1514): a sprite layer's own, or every draw of the frame. FirstDraw counts from the
// frontmost packed draw; D3D11FrameRenderer.DrawSpriteLayer uploads both.
cbuffer SpriteRun : register(b0)
{
    uint FirstDraw, DrawCount;
};
// Each texel: the image's pixel, then its key word (D3D11ImageAtlas.KeyWord): the place of the pixel's colour key among
// its image's in the low sixteen bits (#1633), then whether it is clear and whether it blends.
// Compiled apart from Tiles.hlsl, which declares the same texture.
Texture2D<uint2> Images : register(t4);
static const uint SpriteRecordWords = 6;
static const uint SpriteFlipX = 1, SpriteFlipY = 2, SpriteRecolour = 512;
// Where a record's flags keep the draw's depth among the room's layers (#1514), two bits: D3D11SpriteDrawBuffer's
// DepthShift, holding D3D11FrameRenderer.DepthWord.
static const uint SpriteDepthShift = 3, SpriteDepthMask = 3;
// A rotting corpse's row word naming a row its rot has cleared (#1539), which draws nothing.
static const uint SpriteClearedRow = 0xffffffffu;
// A record flag marking a draw whose shading blends over every part, so each of its pixels blends whether or not its
// image marks it as blending (#1512).
static const uint SpriteShadingBlends = 1024;
// A key word's bits: the place of the pixel's colour key, the mark on a clear pixel, whose alpha is zero, and the mark on
// a pixel that blends, as its frame or its sheet's blend map says (#1514, #1633).
static const uint KeyPlaceMask = 65535, KeyClear = 65536, KeyBlends = 131072;
// A layer pixel's flag marking a sprite pixel that blends.
static const uint SpriteBlends = 16;

struct SpriteVertex
{
    float4 position : SV_Position;
    nointerpolation uint record : RECORD;
};

SpriteVertex SpriteQuad(uint corner : SV_VertexID, uint instance : SV_InstanceID)
{
    // Instance zero is the run's backmost draw, the last one emitted.
    uint record = 1 + (FirstDraw + DrawCount - 1 - instance) * SpriteRecordWords;
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
    // A rotting corpse's image pixel shows the image row the rot has moved there, across the whole image, or nothing once
    // the rot has cleared it (#1539): the record's sixth word starts its table of rows, a word per image row.
    uint rows = SpriteDraws[record + 5];
    if (rows != 0)
    {
        uint shown = SpriteDraws[rows + source.y];
        if (shown == SpriteClearedRow) discard;
        source.y = shown;
    }
    uint2 texel = Images.Load(int3(uint2(place & 65535, place >> 16) + source, 0));
    // A sprite's clear pixel shows what lies behind it.
    if ((texel.y & KeyClear) != 0) discard;
    uint key = texel.y & KeyPlaceMask;
    uint depth = (flags >> SpriteDepthShift) & SpriteDepthMask;
    uint blends = (texel.y & KeyBlends) != 0 || (flags & SpriteShadingBlends) != 0 ? SpriteBlends : 0;
    // A shaded draw's pixel shows the colour its key shows under the draw's shading (#1512, #1633); any other draw's shows
    // its own.
    uint colour = texel.x | 0xff000000;
    if ((flags & SpriteRecolour) != 0) colour = SpriteDraws[SpriteDraws[record + 4] + key];
    return uint2(colour, depth | blends);
}
