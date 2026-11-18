namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How Ridley's fireball or one of its afterburns moves itself (#1843).
/// </summary>
internal enum RidleyFireballMotion
{
    /// <summary>Flies a Ridley fireball.</summary>
    FireballFlying,

    /// <summary>Flies one of Ridley's horizontal afterburns by its velocity.</summary>
    HorizontalAfterburnFlying,

    /// <summary>Flies one of Ridley's vertical afterburns by its velocity.</summary>
    VerticalAfterburnFlying,
}
