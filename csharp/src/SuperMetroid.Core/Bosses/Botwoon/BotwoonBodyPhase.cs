namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's body-segment phases, each naming the behaviour it runs; the segment's state holds the member (#1413).</summary>
internal enum BotwoonBodyPhase
{
    /// <summary>The living body: the segment shows its picture for its angle and the head's hurt flash.</summary>
    Main,
    /// <summary>Botwoon has died: the segment starts its delay before falling, then waits it out.</summary>
    BeginDying,
    /// <summary>The segment waits out its delay before falling, flashing.</summary>
    DyingDelay,
    /// <summary>The segment falls to the floor.</summary>
    Falling,
    /// <summary>The inert state after a body segment lands.</summary>
    Landed,
}
