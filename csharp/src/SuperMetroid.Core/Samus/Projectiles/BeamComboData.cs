namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which data table a beam-combo projectile initializes from.</summary>
internal enum BeamComboData
{
    /// <summary>The combo data table, whose animation ignores the shot's direction.</summary>
    Combo,

    /// <summary>The charged-beam data table, whose animation and radius follow the shot's direction.</summary>
    Charged,

    /// <summary>The Spazer echo data table, whose animation follows the shot's direction.</summary>
    Echo,
}
