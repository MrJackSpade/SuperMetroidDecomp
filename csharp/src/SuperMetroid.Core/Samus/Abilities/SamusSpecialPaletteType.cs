namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Special-Samus-palette handler index at WRAM <c>$0ACC</c>, dispatched through the table at
/// <c>$91:D72D</c>. These are the values the cartridge stores into the word.
/// </summary>
public enum SamusSpecialPaletteType : ushort
{
    /// <summary>No special palette handler owns Samus's suit colors.</summary>
    None = 0,

    /// <summary>Stored Speed Booster shine (<c>HandleSpeedBoosterShinePalette</c>, stored at <c>$90:F010</c>, <c>$90:F545</c>, <c>$91:F7C4</c>).</summary>
    SpeedBoosterShine = 1,

    /// <summary>Active shinespark (<c>HandleShinesparkingPalette</c>, stored at <c>$90:D045</c>).</summary>
    Shinesparking = 6,

    /// <summary>Crystal Flash owns the special-palette timer and suit-color sequence.</summary>
    CrystalFlash = 7,

    /// <summary>X-ray mode owns the special-palette timer and animated visor colors.</summary>
    Xray = 8,
}
