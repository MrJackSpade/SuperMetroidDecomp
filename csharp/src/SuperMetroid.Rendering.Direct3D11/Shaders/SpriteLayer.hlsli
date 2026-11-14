// The sprite layer Sprites.hlsl draws (#1408): per pixel, the colour of the frontmost sprite pixel, then its draw's depth
// among the room's layers (#1514) with SpriteLayerBlends where it blends; 255 where no sprite shows.
static const uint SpriteLayerBlends = 16, NoSprite = 255;
// The depths, as D3D11FrameRenderer.DepthWord packs them: behind the whole room, behind the room's front tiles, and in
// front of the whole room. A frontend sprite layer's draws are packed in front.
static const uint DepthBehindRoom = 0, DepthBehindFrontParts = 1, DepthInFront = 2;

// The frontmost sprite pixel at `screen`: its colour, then its depth, or NoSprite; `blends` says whether it takes part in
// colour blending. Its depth decides only where it is inserted among the room's layers, never which sprite shows.
uint2 SpriteAt(uint2 screen, out bool blends)
{
    uint2 shown = SpriteLayer[screen];
    blends = shown.y != NoSprite && (shown.y & SpriteLayerBlends) != 0;
    return shown.y == NoSprite ? uint2(0, NoSprite) : uint2(shown.x, shown.y & 3);
}

uint2 SpriteAt(uint2 screen)
{
    bool blends;
    return SpriteAt(screen, blends);
}
