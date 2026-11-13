namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's body-segment phases, each naming the behaviour it runs; the segment's state holds the member (#1413).</summary>
internal enum BotwoonBodyPhase
{
    /// <summary>Enemy projectile behaviour: botwoons body, main.</summary>
    Main = 1,
    /// <summary>Enemy projectile behaviour: botwoons body, dying, set delay.</summary>
    BeginDying,
    /// <summary>Enemy projectile behaviour: botwoons body, dying, delay.</summary>
    DyingDelay,
    /// <summary>Enemy projectile behaviour: botwoons body, dying, falling.</summary>
    Falling,
    /// <summary>The inert state after a body segment lands.</summary>
    Landed,
}
