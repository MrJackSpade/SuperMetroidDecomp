namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="SamusSound"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class SamusSoundKeys
{
    /// <summary>Each SamusSound member's installed key.</summary>
    internal static InstalledKeys<SamusSound> Installed { get; } = new(new Dictionary<SamusSound, string>
    {
        [SamusSound.PowerBombExplosion] = "PowerBombExplosion",
        [SamusSound.Missile] = "Missile",
        [SamusSound.SuperMissile] = "SuperMissile",
        [SamusSound.GrappleStart] = "GrappleStart",
        [SamusSound.Grappling] = "Grappling",
        [SamusSound.GrappleEnd] = "GrappleEnd",
        [SamusSound.ChargingBeam] = "ChargingBeam",
        [SamusSound.XrayStart] = "XrayStart",
        [SamusSound.UnchargedPowerBeam] = "UnchargedPowerBeam",
        [SamusSound.UnchargedIceBeam] = "UnchargedIceBeam",
        [SamusSound.UnchargedWaveBeam] = "UnchargedWaveBeam",
        [SamusSound.UnchargedSpazer] = "UnchargedSpazer",
        [SamusSound.UnchargedSpazerIce] = "UnchargedSpazerIce",
        [SamusSound.UnchargedPlasma] = "UnchargedPlasma",
        [SamusSound.ChargedPowerBeam] = "ChargedPowerBeam",
        [SamusSound.ChargedIceBeam] = "ChargedIceBeam",
        [SamusSound.ChargedWaveBeam] = "ChargedWaveBeam",
        [SamusSound.ChargedSpazer] = "ChargedSpazer",
        [SamusSound.ChargedPlasma] = "ChargedPlasma",
        [SamusSound.IceSpecialAttack] = "IceSpecialAttack",
        [SamusSound.IceSpecialAttackEnd] = "IceSpecialAttackEnd",
        [SamusSound.SpazerSpecialAttack] = "SpazerSpecialAttack",
        [SamusSound.SpazerSpecialAttackEnd] = "SpazerSpecialAttackEnd",
        [SamusSound.PlasmaSpecialAttack] = "PlasmaSpecialAttack",
        [SamusSound.WaveSpecialAttack] = "WaveSpecialAttack",
        [SamusSound.FileSelected] = "FileSelected",
        [SamusSound.Saving] = "Saving",
        [SamusSound.UnderwaterSpaceJump] = "UnderwaterSpaceJump",
        [SamusSound.ResumedSpinJump] = "ResumedSpinJump",
        [SamusSound.SpinJump] = "SpinJump",
        [SamusSound.ScrewAttack] = "ScrewAttack",
        [SamusSound.SamusDamaged] = "SamusDamaged",
        [SamusSound.MapScroll] = "MapScroll",
        [SamusSound.MenuCursor] = "MenuCursor",
        [SamusSound.MenuConfirm] = "MenuConfirm",
        [SamusSound.HudWeaponSelect] = "HudWeaponSelect",
        [SamusSound.MapExpand] = "MapExpand",
        [SamusSound.MapReturn] = "MapReturn",
        [SamusSound.DudShot] = "DudShot",
        [SamusSound.SpaceJump] = "SpaceJump",
        [SamusSound.MotherBrainRainbowBeam] = "MotherBrainRainbowBeam",
        [SamusSound.ResumeChargingBeam] = "ResumeChargingBeam",
    });
}
