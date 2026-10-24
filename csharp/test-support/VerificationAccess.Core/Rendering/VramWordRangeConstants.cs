using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="VramWordRange"/> that only verification reads.</summary>
internal static class VramWordRangeConstants
{
    /// <summary>The last addressable VRAM word.</summary>
    public const int MaximumWord = SnesPpuLayoutConstants.MaximumVramWord;
}
