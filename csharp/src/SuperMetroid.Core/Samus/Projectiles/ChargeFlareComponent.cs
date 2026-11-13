namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The charge flare's three animated parts, each playing its own program of pictures.</summary>
public enum ChargeFlareComponent
{
    /// <summary>The flare at the arm cannon's muzzle.</summary>
    MainFlare,

    /// <summary>The slower of the two spark rings around it.</summary>
    SlowSparks,

    /// <summary>The faster spark ring.</summary>
    FastSparks,
}
