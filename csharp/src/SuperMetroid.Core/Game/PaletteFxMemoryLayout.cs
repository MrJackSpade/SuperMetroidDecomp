namespace SuperMetroid.Core.Game;

/// <summary>Adjacent native WRAM words intentionally read by palette pre-instruction $8D:F621.</summary>
public static class PaletteFxMemoryLayout
{
    /// <summary>$7E:1E79 PaletteFXObject_Enable; bit 15 must be set for the native handler to run.</summary>
    public const ushort HandlerEnabled = 0x8000;
    /// <summary>$7E:1E7B palettefx_index follows the enable word by one 16-bit word.</summary>
    public const int CurrentIndexWordOffset = 1;
    /// <summary>$7E:1E7D palettefx_ids starts two words after PaletteFXObject_Enable.</summary>
    public const int IdArrayWordOffset = 2;
}
