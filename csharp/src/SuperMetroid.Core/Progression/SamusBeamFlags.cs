namespace SuperMetroid.Core.Progression;

/// <summary>
/// Samus's independent beam components and the Charge Beam upgrade, one flag each; they combine freely. The values are
/// arbitrary and nothing reads them: the saved beam encoding belongs to the import's equipment codec.
/// </summary>
[Flags]
public enum SamusBeamFlags
{
    /// <summary>The unmodified Power Beam, with no optional beam components enabled.</summary>
    None = 0,

    /// <summary>Adds the Wave Beam's terrain-passing behavior and waveform presentation.</summary>
    Wave = 1 << 0,

    /// <summary>Adds the Ice Beam's enemy-freezing behavior.</summary>
    Ice = 1 << 1,

    /// <summary>Adds the Spazer's widened three-part projectile; mutually exclusive with Plasma in retail equipment UI.</summary>
    Spazer = 1 << 2,

    /// <summary>Adds the Plasma Beam's penetrating high-damage projectile; mutually exclusive with Spazer in retail equipment UI.</summary>
    Plasma = 1 << 3,

    /// <summary>Allows the fire button to charge the equipped beam combination.</summary>
    Charge = 1 << 4,
}
