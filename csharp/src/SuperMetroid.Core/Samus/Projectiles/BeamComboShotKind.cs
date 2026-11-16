namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which shot a special attack's particle takes its damage and animation from.</summary>
internal enum BeamComboShotKind
{
    /// <summary>The special attack's own particle, whose animation ignores the shot's direction.</summary>
    Combo,

    /// <summary>The charged beam's shot, whose animation and radius follow the shot's direction.</summary>
    Charged,

    /// <summary>The Spazer echo, whose animation follows the shot's direction.</summary>
    Echo,
}
