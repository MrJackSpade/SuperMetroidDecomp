namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values of <see cref="SnesPpuLayout"/> that only verification reads.</summary>
internal static class SnesPpuLayoutConstants
{
    /// <summary>Size of OAM's two-bits-per-OBJ high table.</summary>
    public const int OamHighTableByteCount = SnesPpuLayout.OamSpriteCount / 4;
    /// <summary>Complete low-plus-high OAM DMA payload.</summary>
    public const int OamUploadByteCount = SnesPpuLayoutConstants.OamLowTableByteCount + OamHighTableByteCount;
    /// <summary>Size of OAM's four-byte-per-OBJ low table.</summary>
    public const int OamLowTableByteCount = SnesPpuLayout.OamSpriteCount * 4;
    /// <summary>The last VRAM word a render layer may name as a tilemap or character base.</summary>
    public const int MaximumVramWord = SnesPpuLayout.VramWordCount - 1;
    /// <summary>Returns <paramref name="word"/> when it names a VRAM word, or throws.</summary>
    /// <param name="word">A VRAM word address, 0 through <see cref="MaximumVramWord"/>.</param>
    public static int RequireVramWord(int word) => Ensure.BetweenInclusive(0, MaximumVramWord, word);
}
