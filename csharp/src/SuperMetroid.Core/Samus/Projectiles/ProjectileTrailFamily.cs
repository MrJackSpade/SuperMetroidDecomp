namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A trail family: the direction table one row of a trail selector names.</summary>
internal enum ProjectileTrailFamily
{
    /// <summary>The trails of uncharged beams that start every trail at the projectile.</summary>
    UnchargedDefault,

    /// <summary>The trails of uncharged Wave and Ice Wave.</summary>
    UnchargedWave,

    /// <summary>The trails of uncharged Ice Spazer.</summary>
    UnchargedIceSpazer,

    /// <summary>The trails of uncharged Wave Ice Spazer.</summary>
    UnchargedWaveIceSpazer,

    /// <summary>The trails of uncharged Ice Plasma.</summary>
    UnchargedIcePlasma,

    /// <summary>The trails of uncharged Wave Ice Plasma.</summary>
    UnchargedWaveIcePlasma,

    /// <summary>The trails of charged beams that start every trail at the projectile.</summary>
    ChargedDefault,

    /// <summary>The trails of charged Wave and Ice Wave.</summary>
    ChargedWave,

    /// <summary>The trails of charged Ice Spazer.</summary>
    ChargedIceSpazer,

    /// <summary>The trails of charged Wave Ice Spazer.</summary>
    ChargedWaveIceSpazer,

    /// <summary>The trails of charged Ice Plasma.</summary>
    ChargedIcePlasma,

    /// <summary>The trails of charged Wave Ice Plasma.</summary>
    ChargedWaveIcePlasma,

    /// <summary>The trails of the Wave Spazer special beam attack.</summary>
    SbaWaveSpazer,

    /// <summary>The trails of the Spazer and Ice Spazer special beam attack, which the cartridge leaves unused.</summary>
    SbaSpazer,
}
