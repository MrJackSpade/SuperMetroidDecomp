Texture2D<uint> Source : register(t0);
cbuffer DisplayParameters : register(b0)
{
    uint Left, Top, Width, Height;
};

float4 Vertex(uint id : SV_VertexID) : SV_Position
{
    float2 position = float2((id << 1) & 2, id & 2);
    return float4(position * float2(2, -2) + float2(-1, 1), 0, 1);
}

float4 Pixel(float4 position : SV_Position) : SV_Target
{
    uint2 pixel = uint2(position.xy);
    if (pixel.x < Left || pixel.y < Top || pixel.x >= Left + Width || pixel.y >= Top + Height)
        return float4(0, 0, 0, 1);
    uint2 source = (pixel - uint2(Left, Top)) * uint2(256, 224) / uint2(Width, Height);
    uint packed = Source.Load(int3(source, 0));
    return float4(packed & 255, (packed >> 8) & 255, (packed >> 16) & 255, 255) / 255.0;
}
