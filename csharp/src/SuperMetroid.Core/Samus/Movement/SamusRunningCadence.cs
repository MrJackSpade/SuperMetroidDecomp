namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The running cadence Samus animates by: ordinary running, or one Speed Booster stage.</summary>
internal enum SamusRunningCadence
{
    /// <summary>Ordinary running: ten two-tick frames.</summary>
    Ordinary,
    /// <summary>Speed Booster stage zero: three ticks a frame.</summary>
    BoostStage0,
    /// <summary>Speed Booster stage one: two and three ticks alternating.</summary>
    BoostStage1,
    /// <summary>Speed Booster stage two: two ticks a frame.</summary>
    BoostStage2,
    /// <summary>Speed Booster stage three: one and two ticks alternating.</summary>
    BoostStage3,
    /// <summary>Speed Booster stage four: one tick a frame.</summary>
    BoostStage4,
    /// <summary>
    /// The cadence a full sound queue selects, which the game never authored
    /// (<see cref="SuperMetroid.Core.Compatibility.UnauthoredBoostCadence"/>).
    /// </summary>
    UnauthoredBoost,
}
