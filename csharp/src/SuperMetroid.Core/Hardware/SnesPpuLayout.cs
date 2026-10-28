using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The physical PPU memory capacities the import-only memory images size themselves by. The screen's own geometry is
/// Rendering's ScreenGeometry; nothing here describes the game screen.
/// </summary>
public static class SnesPpuLayout
{
    /// <summary>Physical 15-bit VMADD capacity.</summary>
    public const int VramWordCount = 0x8000;

    /// <summary>Physical 64-KiB VRAM capacity.</summary>
    public const int VramByteCount = VramWordCount * 2;

    /// <summary>Number of native BGR555 entries in CGRAM.</summary>
    public const int CgramColorCount = 256;

    /// <summary>Size of the complete CGRAM image uploaded by NMI.</summary>
    public const int CgramByteCount = CgramColorCount * 2;

    /// <summary>Number of hardware OBJ records.</summary>
    public const int OamSpriteCount = 128;
}
