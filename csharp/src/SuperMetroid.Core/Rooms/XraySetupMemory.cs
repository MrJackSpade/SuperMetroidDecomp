using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>WRAM scratch buffers and setup-stage identities used by the native X-ray tilemap transfers.</summary>
public static class XraySetupMemory
{
    /// <summary>$91:CB1C reads the second BG1 screen during setup call two.</summary>
    public const byte ReadSecondScreenStage = 2;
    /// <summary>$91:CB57 reads the first BG1 screen during setup call three.</summary>
    public const byte ReadFirstScreenStage = 3;
    /// <summary>$91:CB8E constructs the reveal map once during setup call four.</summary>
    public const byte BuildRevealStage = 4;
    /// <summary>$7E:6000, first BG1 screen captured by $91:CB57.</summary>
    public const int SavedBg1 = 0x7E6000;
    /// <summary>$7E:6800, second BG1 screen captured by $91:CB1C.</summary>
    public const int SavedBg1SecondScreen = SavedBg1 + XrayTilemapLayout.ScreenWords * 2;
    /// <summary>$7E:4000-$7E:4FFF, reveal map built by $91:CB8E and transferred by stages six/seven.</summary>
    public const int RevealTilemap = 0x7E4000;

    /// <summary>Reads the frozen reveal map, never rebuilding it from subsequently changed room/VRAM data.</summary>
    public static ushort[] ReadReveal(ISnesAddressSpace bus)
    {
        var words = new ushort[XrayTilemapLayout.BufferWords];
        for (int i = 0; i < words.Length; i++)
            words[i] = (ushort)(bus.ReadByte(RevealTilemap + i * 2) | bus.ReadByte(RevealTilemap + i * 2 + 1) << 8);
        return words;
    }
}
