/// <summary>Mutually exclusive input schedules for the native bomb-chain parity fixture.</summary>
internal enum BombChainAuditScenario
{
    /// <summary>A single bomb or three evenly spaced bombs, with one optional direction pulse.</summary>
    Short,
    /// <summary>Repeated bombs at the longer, single-bomb ascent interval.</summary>
    Repeated,
    /// <summary>Three bombs with independently varied second and third timestamps.</summary>
    Triple,
    /// <summary>Three bombs with varied steering duration near the first apex.</summary>
    Steering,
    /// <summary>A starting double jump followed by repeated shorter ladder intervals.</summary>
    Ladder,
    /// <summary>Repeated low-ceiling bombs with a swept turn-and-return input phase.</summary>
    CeilingSteering,
}
