using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Reads the bytes an audit uploaded into VRAM back through the little-endian words that hold them.</summary>
internal static class VramBytes
{
    /// <summary>The byte at <paramref name="offset"/> in <paramref name="vram"/>: its word's low byte at an even offset, high at an odd one.</summary>
    internal static byte At(SnesVram vram, int offset) => (byte)(vram.ReadWord(offset / 2) >> offset % 2 * 8);
}
