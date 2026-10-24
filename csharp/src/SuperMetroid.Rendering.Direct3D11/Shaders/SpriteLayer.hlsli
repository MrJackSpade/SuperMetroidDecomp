// The sprite layer Sprites.hlsl draws (#1408): per pixel, the colour of the frontmost sprite pixel, then its priority
// against the backgrounds with SpriteLayerBlends where it blends; 255 where no sprite shows.
static const uint SpriteLayerBlends = 16, NoSprite = 255;

// The frontmost sprite pixel at `screen`: its colour, then its priority, or NoSprite; `blends` says whether it takes
// part in colour blending. Its priority decides only where it is inserted, never which sprite shows.
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
