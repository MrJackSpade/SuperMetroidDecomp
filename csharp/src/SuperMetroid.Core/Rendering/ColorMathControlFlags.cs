namespace SuperMetroid.Core.Rendering;

/// <summary>CGADSUB ($2131): the layers colour math applies to and how it combines; games compose these bits.</summary>
[Flags]
public enum ColorMathControlFlags : byte
{
    /// <summary>No layer takes colour math.</summary>
    None = 0,
    /// <summary>BG1 takes colour math.</summary>
    Bg1 = 0x01,
    /// <summary>BG2 takes colour math.</summary>
    Bg2 = 0x02,
    /// <summary>BG3 takes colour math.</summary>
    Bg3 = 0x04,
    /// <summary>BG4 takes colour math.</summary>
    Bg4 = 0x08,
    /// <summary>OBJs with palettes 4-7 take colour math.</summary>
    Obj = 0x10,
    /// <summary>The backdrop takes colour math.</summary>
    Backdrop = 0x20,
    /// <summary>The result is halved.</summary>
    Half = 0x40,
    /// <summary>The fixed or sub-screen colour is subtracted rather than added.</summary>
    Subtract = 0x80,
}
