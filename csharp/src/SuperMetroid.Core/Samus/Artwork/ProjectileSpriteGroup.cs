namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A family of Samus projectile or charge-flare compositions, named in installed composition and binding files.</summary>
public enum ProjectileSpriteGroup
{
    /// <summary>The empty composition an invisible projectile draws.</summary>
    Nothing,
    /// <summary>The eight small Power beam poses.</summary>
    Power,
    /// <summary>Plasma's axial and diagonal strips.</summary>
    Plasma,
    /// <summary>The missile explosion's growth from a small central flash to five full bursts.</summary>
    MissileExplosion,
    /// <summary>The bomb explosion's five growing bursts.</summary>
    BombExplosion,
    /// <summary>The super-missile explosion's cores, core with spikes, ring and ring with spikes.</summary>
    SuperMissileExplosion,
    /// <summary>The three Power Bomb poses.</summary>
    PowerBomb,
    /// <summary>The beam explosion's two small cores and four growing bursts.</summary>
    BeamExplosion,
    /// <summary>The four bomb poses.</summary>
    Bomb,
    /// <summary>The eight missile compass poses: each axial pose followed by a diagonal one.</summary>
    Missile,
    /// <summary>The eight super-missile compass poses: each axial pose followed by a diagonal one.</summary>
    SuperMissile,
    /// <summary>The Wave beam's centred core and its four stages in each of eight displacement directions.</summary>
    Wave,
    /// <summary>Charged Wave's two cores, then its paired lobes.</summary>
    ChargedWave,
    /// <summary>Charged IceWave's two cores, then its paired lobes.</summary>
    ChargedIceWave,
    /// <summary>PlasmaWave's cores and lobes in each of its twelve orientations and alternates.</summary>
    PlasmaWave,
    /// <summary>Spazer's seeds and three-lane spreads: four large diagonal orientations, then four small axial ones.</summary>
    Spazer,
    /// <summary>Charged Spazer's seeds and spreads in its axial and diagonal orientations.</summary>
    ChargedSpazer,
    /// <summary>Charged Power's sixteen poses.</summary>
    ChargedPower,
    /// <summary>Charged Ice's four poses.</summary>
    ChargedIce,
    /// <summary>Ice beam's four centered poses.</summary>
    Ice,
    /// <summary>Spazer's startup strips and pairs across its orientations.</summary>
    SpazerStartup,
    /// <summary>Charged Plasma's startup growth across its axial and diagonal orientations.</summary>
    PlasmaStartup,
    /// <summary>The charge, Hyper and grapple flare: three small poses and the large crest.</summary>
    FlareMain,
    /// <summary>The six slow right-facing spark poses.</summary>
    FlareSlowSparksRight,
    /// <summary>The six fast right-facing spark poses.</summary>
    FlareFastSparksRight,
    /// <summary>The six slow left-facing spark poses.</summary>
    FlareSlowSparksLeft,
    /// <summary>The six fast left-facing spark poses.</summary>
    FlareFastSparksLeft,
    /// <summary>
    /// The burst a hit of Samus's weapon shows at an enemy it hurts (#1713): four poses of a burst mirrored about its centre,
    /// smallest first.
    /// </summary>
    HitBurst,
}
