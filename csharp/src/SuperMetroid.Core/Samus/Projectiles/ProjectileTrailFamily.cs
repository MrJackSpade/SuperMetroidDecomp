namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A trail family: the trails, one per direction, that a kind of shot leaves.</summary>
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
}
