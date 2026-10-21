namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A family of Samus projectile or charge-flare compositions; the projectile families follow the cartridge's storage order.</summary>
public enum ProjectileSpriteGroup
{
    /// <summary>The empty composition an invisible projectile draws.</summary>
    Nothing,
    /// <summary>The eight one-cell Power beam poses.</summary>
    Power,
    /// <summary>Plasma's axial four-tile and diagonal six-part strips.</summary>
    Plasma,
    /// <summary>The missile explosion's growth from one central tile to five four-quadrant poses.</summary>
    MissileExplosion,
    /// <summary>The bomb explosion's five four-quadrant stages.</summary>
    BombExplosion,
    /// <summary>The super-missile explosion's cores, core with spikes, ring and ring with spikes.</summary>
    SuperMissileExplosion,
    /// <summary>The three Power Bomb poses.</summary>
    PowerBomb,
    /// <summary>The beam explosion's two single-tile cores and four four-quadrant stages.</summary>
    BeamExplosion,
    /// <summary>The four bomb poses.</summary>
    Bomb,
    /// <summary>The eight missile compass poses: each two-part axial pose followed by a three-part diagonal one.</summary>
    Missile,
    /// <summary>The eight super-missile compass poses: each two-part axial pose followed by a three-part diagonal one.</summary>
    SuperMissile,
    /// <summary>The Wave beam's centered cell and its four stages in each of eight displacement directions.</summary>
    Wave,
    /// <summary>Charged Wave's two four-quadrant cores, then its paired four-quadrant lobes.</summary>
    ChargedWave,
    /// <summary>Charged IceWave's two four-quadrant cores, then its paired four-quadrant lobes.</summary>
    ChargedIceWave,
    /// <summary>PlasmaWave's cores and lobes in each of its twelve orientations and alternates.</summary>
    PlasmaWave,
    /// <summary>Spazer's seeds and three-lane spreads: four large diagonal orientations, then four small axial ones.</summary>
    Spazer,
    /// <summary>Charged Spazer's seeds and spreads in its axial and diagonal orientations.</summary>
    ChargedSpazer,
    /// <summary>Charged Power's sixteen four-quadrant poses.</summary>
    ChargedPower,
    /// <summary>Charged Ice's four four-quadrant poses.</summary>
    ChargedIce,
    /// <summary>Ice beam's four centered poses.</summary>
    Ice,
    /// <summary>Spazer's startup strips and pairs across its orientations.</summary>
    SpazerStartup,
    /// <summary>Charged Plasma's startup growth across its axial and diagonal orientations.</summary>
    PlasmaStartup,
    /// <summary>The charge, Hyper and grapple flare: three one-object poses and the four-object crest.</summary>
    FlareMain,
    /// <summary>The six slow right-facing spark poses.</summary>
    FlareSlowSparksRight,
    /// <summary>The six fast right-facing spark poses.</summary>
    FlareFastSparksRight,
    /// <summary>The six slow left-facing spark poses.</summary>
    FlareSlowSparksLeft,
    /// <summary>The six fast left-facing spark poses.</summary>
    FlareFastSparksLeft,
}
