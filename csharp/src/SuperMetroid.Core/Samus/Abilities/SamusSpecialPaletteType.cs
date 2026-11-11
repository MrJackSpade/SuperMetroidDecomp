namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Which special suit colouring colours Samus: the stored shine, a shinespark, Crystal Flash or the X-ray visor.
/// At most one runs at a time; starting one ends the one before.
/// </summary>
public enum SamusSpecialPaletteType
{
    /// <summary>No special colouring changes Samus's suit colours.</summary>
    None = 0,

    /// <summary>Stored Speed Booster shine (the shine's colours, stored).</summary>
    SpeedBoosterShine = 1,

    /// <summary>Active shinespark (the shinespark's colours, stored).</summary>
    Shinesparking = 6,

    /// <summary>Crystal Flash owns the special-palette timer and suit-color sequence.</summary>
    CrystalFlash = 7,

    /// <summary>X-ray mode owns the special-palette timer and animated visor colors.</summary>
    Xray = 8,
}
