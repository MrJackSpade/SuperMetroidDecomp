namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="SamusBeamCombination"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SamusBeamCombinationKeys
{
    /// <summary>Each SamusBeamCombination member's installed key.</summary>
    internal static InstalledKeys<SamusBeamCombination> Installed { get; } = new(new Dictionary<SamusBeamCombination, string>
    {
        [SamusBeamCombination.Power] = "Power",
        [SamusBeamCombination.Wave] = "Wave",
        [SamusBeamCombination.Ice] = "Ice",
        [SamusBeamCombination.IceWave] = "IceWave",
        [SamusBeamCombination.Spazer] = "Spazer",
        [SamusBeamCombination.SpazerWave] = "SpazerWave",
        [SamusBeamCombination.SpazerIce] = "SpazerIce",
        [SamusBeamCombination.SpazerIceWave] = "SpazerIceWave",
        [SamusBeamCombination.Plasma] = "Plasma",
        [SamusBeamCombination.PlasmaWave] = "PlasmaWave",
        [SamusBeamCombination.PlasmaIce] = "PlasmaIce",
        [SamusBeamCombination.PlasmaIceWave] = "PlasmaIceWave",
        [SamusBeamCombination.SpazerPlasma] = "SpazerPlasma",
        [SamusBeamCombination.SpazerPlasmaWave] = "SpazerPlasmaWave",
        [SamusBeamCombination.SpazerPlasmaIce] = "SpazerPlasmaIce",
        [SamusBeamCombination.SpazerPlasmaIceWave] = "SpazerPlasmaIceWave",
    });
}
