namespace Engine.World;

/// <summary>What a hit did to what it landed on.</summary>
public enum HitResult
{
    /// <summary>Nothing: the target does not take hits like this one, and it passes as if nothing were there.</summary>
    Ignored,

    /// <summary>The target took the hit and no damage.</summary>
    Absorbed,

    /// <summary>
    /// The target took the hit and no damage, and has itself left what shows that, if anything does: what delivered
    /// the hit is gone, leaving nothing of its own.
    /// </summary>
    Dud,

    /// <summary>The target took damage.</summary>
    Hurt,

    /// <summary>The target took damage and was destroyed.</summary>
    Destroyed,

    /// <summary>
    /// The target turned the hit's projectile another way (<see cref="ITurnsShots"/>): it took nothing, and the
    /// projectile flies on, still its launcher's.
    /// </summary>
    TurnedBack,
}
