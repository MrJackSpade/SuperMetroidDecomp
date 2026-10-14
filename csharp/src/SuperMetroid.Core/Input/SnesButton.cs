namespace SuperMetroid.Core.Input;

/// <summary>
/// Retail SNES controller bit assignments as returned by registers <c>$4218/$4219</c>.
/// </summary>
[Flags]
public enum SnesButton : ushort
{
    /// <summary>No controller buttons are held.</summary>
    None = 0,

    /// <summary>The right shoulder button, returned in bit 4 of the automatic joypad word.</summary>
    R = 0x0010,

    /// <summary>The left shoulder button, returned in bit 5 of the automatic joypad word.</summary>
    L = 0x0020,

    /// <summary>The upper face button, returned in bit 6 of the automatic joypad word.</summary>
    X = 0x0040,

    /// <summary>The right face button, returned in bit 7 of the automatic joypad word.</summary>
    A = 0x0080,

    /// <summary>The directional pad's right direction, returned in bit 8 of the automatic joypad word.</summary>
    Right = 0x0100,

    /// <summary>The directional pad's left direction, returned in bit 9 of the automatic joypad word.</summary>
    Left = 0x0200,

    /// <summary>The directional pad's down direction, returned in bit 10 of the automatic joypad word.</summary>
    Down = 0x0400,

    /// <summary>The directional pad's up direction, returned in bit 11 of the automatic joypad word.</summary>
    Up = 0x0800,

    /// <summary>The Start button, returned in bit 12 of the automatic joypad word.</summary>
    Start = 0x1000,

    /// <summary>The Select button, returned in bit 13 of the automatic joypad word.</summary>
    Select = 0x2000,

    /// <summary>The left face button, returned in bit 14 of the automatic joypad word.</summary>
    Y = 0x4000,

    /// <summary>The lower face button, returned in bit 15 of the automatic joypad word.</summary>
    B = 0x8000,
}
