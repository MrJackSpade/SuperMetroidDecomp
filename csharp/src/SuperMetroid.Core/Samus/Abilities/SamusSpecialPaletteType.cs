namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Which special suit colouring colours Samus: the stored shine, a shinespark, Crystal Flash or the X-ray visor.
/// <see cref="SamusSpecialColouring"/> holds the one that runs; starting one ends the one before.
/// </summary>
public enum SamusSpecialPaletteType
{
    /// <summary>No special colouring changes Samus's suit colours.</summary>
    None,

    /// <summary>The stored shine's glow, while a shinespark waits to be launched.</summary>
    SpeedBoosterShine,

    /// <summary>The shinespark's flashing, from its windup through its flight.</summary>
    Shinesparking,

    /// <summary>Crystal Flash's pulsing suit and bubble colours.</summary>
    CrystalFlash,

    /// <summary>The X-ray visor's animated colours.</summary>
    Xray,
}
