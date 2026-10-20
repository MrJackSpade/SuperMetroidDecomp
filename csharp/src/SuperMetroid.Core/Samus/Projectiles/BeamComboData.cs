namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which data table a beam-combo projectile initializes from.</summary>
internal enum BeamComboData
{
    /// <summary>The combo data table, whose program ignores the shot's direction.</summary>
    Combo,

    /// <summary>The charged-beam data table, whose program and radius follow the shot's direction.</summary>
    Charged,

    /// <summary>The Spazer echo data table, whose program follows the shot's direction.</summary>
    Echo,
}
