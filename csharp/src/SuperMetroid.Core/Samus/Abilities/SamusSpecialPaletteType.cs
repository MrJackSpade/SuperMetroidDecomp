namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Special-Samus-palette handler index, dispatched through the handler table.
/// These are the values the game stores into the word.
/// </summary>
public enum SamusSpecialPaletteType
{
    /// <summary>No special palette handler owns Samus's suit colors.</summary>
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
