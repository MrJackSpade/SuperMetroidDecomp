using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>VRAM address arithmetic (#223): 15-bit word addresses over 64 KiB of byte storage.</summary>
[PpuLayoutAccess]
[SnesEncodingOwner]
public static class SnesVramAddressing
{

    /// <summary>Wraps a word address onto the physical 15-bit VMADD bus, as the PPU does.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WrapWord(int wordAddress) => wordAddress & (SnesPpuLayout.VramWordCount - 1);
}
