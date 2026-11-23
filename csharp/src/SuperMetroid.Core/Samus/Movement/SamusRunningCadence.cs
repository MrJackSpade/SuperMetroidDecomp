namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The running cadence Samus animates by: ordinary running, or one Speed Booster stage.</summary>
internal enum SamusRunningCadence
{
    /// <summary>Ordinary running, at its own steady pace.</summary>
    Ordinary,
    /// <summary>Speed Booster stage zero, the slowest boost.</summary>
    BoostStage0,
    /// <summary>Speed Booster stage one, a little quicker.</summary>
    BoostStage1,
    /// <summary>Speed Booster stage two, as quick as ordinary running.</summary>
    BoostStage2,
    /// <summary>Speed Booster stage three, quicker still.</summary>
    BoostStage3,
    /// <summary>Speed Booster stage four, the quickest.</summary>
    BoostStage4,
}
