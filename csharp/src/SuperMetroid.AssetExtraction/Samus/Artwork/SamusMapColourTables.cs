namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// The selected Samus colour tables the map installation holds (#1512): only the import reads them, pre-colouring her
/// sheets in the shades her state names. Gameplay holds none of these colours.
/// </summary>
/// <param name="Visor">Her visor's colours, shared by X-ray and the room palette cycles.</param>
/// <param name="Hurt">Her hurt-flash and cinematic-restoration colours.</param>
/// <param name="Suits">Her Power, Varia and Gravity Suit colours.</param>
/// <param name="FullBodyCycles">Her full-body cycle colours.</param>
/// <param name="Charge">Her charged-beam and pseudo-Screw-Attack colours.</param>
/// <param name="HyperBeam">Her Hyper Beam cycle colours.</param>
/// <param name="CrystalFlash">Her Crystal Flash body and bubble colours.</param>
public sealed record SamusMapColourTables(InstalledSamusVisorColors Visor, InstalledSamusHurtColors Hurt,
    InstalledSamusSuitColors Suits, SamusFullBodyCycleColours FullBodyCycles, SamusChargeColours Charge,
    SamusHyperBeamColours HyperBeam, InstalledCrystalFlashColors CrystalFlash);
