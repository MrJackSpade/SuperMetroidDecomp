using SuperMetroid.Core.Hardware;
using SuperMetroid.Core;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>
/// Packs values into 32-bit constant-buffer words (#223): the compute shaders unpack byte 0 from the lowest
/// eight bits upward, and a pair of sixteen-bit halves low half first, so both sides share this layout.
/// </summary>
[SnesEncodingOwner]
internal static class D3D11ConstantPacking
{
    /// <summary>Four bytes in one constant word, <paramref name="byte0"/> in the lowest eight bits.</summary>
    public static uint Bytes(byte byte0, byte byte1, byte byte2, byte byte3) =>
        (uint)(byte0 | byte1 << 8 | byte2 << 16 | byte3 << 24);

    /// <summary>Two sixteen-bit halves in one constant word, <paramref name="low"/> in the lower half.</summary>
    public static uint Halves(ushort low, ushort high) => (uint)(unchecked((int)SnesSixteenSixteen.Compose(high, low)));
}
