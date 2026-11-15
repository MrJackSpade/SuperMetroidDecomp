namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// How one of Botwoon's body segments or his spit moves itself (#1843).
/// </summary>
internal enum BotwoonProjectileMotion
{
    /// <summary>Orients a Botwoon body segment, shows its hurt colours, and runs its death sequence.</summary>
    BodySegment,

    /// <summary>Flies Botwoon's spit by its 1/65536-pixel vector until it leaves the camera.</summary>
    SpitFlying,
}
