namespace SuperMetroid.Core.Progression;

/// <summary>
/// Independent beam-equipment bits stored at WRAM <c>$09A6</c>. Low-nibble beam bits may
/// be combined; Charge occupies bit twelve just as it does in the cartridge word.
/// </summary>
[Flags]
public enum SamusBeamFlags : ushort
{
    /// <summary>The unmodified Power Beam, with no optional beam components enabled.</summary>
    None = 0,

    /// <summary>Adds the Wave Beam's terrain-passing behavior and waveform presentation.</summary>
    Wave = 0x0001,

    /// <summary>Adds the Ice Beam's enemy-freezing behavior.</summary>
    Ice = 0x0002,

    /// <summary>Adds the Spazer's widened three-part projectile; mutually exclusive with Plasma in retail equipment UI.</summary>
    Spazer = 0x0004,

    /// <summary>Adds the Plasma Beam's penetrating high-damage projectile; mutually exclusive with Spazer in retail equipment UI.</summary>
    Plasma = 0x0008,

    /// <summary>Allows the fire button to charge the equipped beam combination.</summary>
    Charge = 0x1000,
}
