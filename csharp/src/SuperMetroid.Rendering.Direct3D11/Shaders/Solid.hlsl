// Integer output preserves the portable packet's exact RGBA bytes. No sRGB,
// filtering, floating-point color conversion or CPU-composited image is involved.
cbuffer SolidParameters : register(b0)
{
    uint PackedColor;
    uint FadeCount;
    uint Width;
    uint Height;
    uint4 FadeLevels[256];
};
RWTexture2D<uint> Output : register(u0);

[numthreads(8, 8, 1)]
void Main(uint3 id : SV_DispatchThreadID)
{
    if (id.x >= Width || id.y >= Height) return;
    uint4 color = uint4(PackedColor & 255, (PackedColor >> 8) & 255,
        (PackedColor >> 16) & 255, PackedColor >> 24);
    if (color.a != 0)
        for (uint i = 0; i < FadeCount; i++)
            color.rgb = color.rgb * FadeLevels[i / 4][i % 4] / 15;
    Output[id.xy] = color.r | (color.g << 8) | (color.b << 16) | (color.a << 24);
}
