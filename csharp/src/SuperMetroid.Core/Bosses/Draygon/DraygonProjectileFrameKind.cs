namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What a Draygon projectile frame belongs to.</summary>
public enum DraygonProjectileFrameKind
{
    /// <summary>The goop in flight.</summary>
    Goop,
    /// <summary>The goop bursting after a Samus shot.</summary>
    GoopShot,
    /// <summary>The wall turret's shot forming.</summary>
    WallTurretBloom,
    /// <summary>The wall turret's shot in flight.</summary>
    WallTurretFlight,
}
