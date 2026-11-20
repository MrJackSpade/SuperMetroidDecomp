namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed key of each <see cref="ProjectileSpriteGroup"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ProjectileSpriteGroupKeys
{
    /// <summary>Each ProjectileSpriteGroup member's installed key.</summary>
    internal static InstalledKeys<ProjectileSpriteGroup> Installed { get; } = new(new Dictionary<ProjectileSpriteGroup, string>
    {
        [ProjectileSpriteGroup.Power] = "Power",
        [ProjectileSpriteGroup.Plasma] = "Plasma",
        [ProjectileSpriteGroup.MissileExplosion] = "MissileExplosion",
        [ProjectileSpriteGroup.BombExplosion] = "BombExplosion",
        [ProjectileSpriteGroup.SuperMissileExplosion] = "SuperMissileExplosion",
        [ProjectileSpriteGroup.PowerBomb] = "PowerBomb",
        [ProjectileSpriteGroup.BeamExplosion] = "BeamExplosion",
        [ProjectileSpriteGroup.Bomb] = "Bomb",
        [ProjectileSpriteGroup.Missile] = "Missile",
        [ProjectileSpriteGroup.SuperMissile] = "SuperMissile",
        [ProjectileSpriteGroup.Wave] = "Wave",
        [ProjectileSpriteGroup.ChargedWave] = "ChargedWave",
        [ProjectileSpriteGroup.PlasmaWave] = "PlasmaWave",
        [ProjectileSpriteGroup.Spazer] = "Spazer",
        [ProjectileSpriteGroup.ChargedSpazer] = "ChargedSpazer",
        [ProjectileSpriteGroup.ChargedPower] = "ChargedPower",
        [ProjectileSpriteGroup.SpazerStartup] = "SpazerStartup",
        [ProjectileSpriteGroup.PlasmaStartup] = "PlasmaStartup",
        [ProjectileSpriteGroup.FlareMain] = "FlareMain",
        [ProjectileSpriteGroup.FlareSlowSparksRight] = "FlareSlowSparksRight",
        [ProjectileSpriteGroup.FlareFastSparksRight] = "FlareFastSparksRight",
        [ProjectileSpriteGroup.FlareSlowSparksLeft] = "FlareSlowSparksLeft",
        [ProjectileSpriteGroup.FlareFastSparksLeft] = "FlareFastSparksLeft",
        [ProjectileSpriteGroup.HitBurst] = "HitBurst",
    });
}
