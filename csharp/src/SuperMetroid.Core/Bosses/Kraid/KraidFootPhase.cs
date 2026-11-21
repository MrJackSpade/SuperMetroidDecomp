namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase Kraid's foot runs.</summary>
public enum KraidFootPhase
{
    /// <summary>The foot rests, doing nothing on its turn: between its walks and once Kraid grows or dies (#1870).</summary>
    Resting,
    /// <summary>
    /// The first-phase foot thinks while its phase wait runs, then prepares its lunge
    /// (<see cref="PrepareFirstPhaseLunge"/>).
    /// </summary>
    FirstPhaseThinking,
    /// <summary>Prepares the first-phase foot's forward lunge.</summary>
    PrepareFirstPhaseLunge,
    /// <summary>Advances the first-phase forward lunge before retreat.</summary>
    FirstPhaseLunge,
    /// <summary>Runs the foot's phase wait after the lunge, then retreats (<see cref="FirstPhaseRetreat"/>).</summary>
    WaitBeforeRetreat,
    /// <summary>Retreats from the first-phase lunge and restores the foot's thinking cycle.</summary>
    FirstPhaseRetreat,
    /// <summary>Walks to the second-phase starting point before its setup delay.</summary>
    SecondPhaseWalkToStart,
    /// <summary>
    /// Runs the foot's phase wait at the second-phase starting point, then sets its thinking up
    /// (<see cref="SecondPhaseInitialize"/>).
    /// </summary>
    WaitBeforeSecondPhase,
    /// <summary>Initializes second-phase foot thinking after the starting-point walk.</summary>
    SecondPhaseInitialize,
    /// <summary>Counts the second-phase foot's think delay and selects a walking target.</summary>
    SecondPhaseThinking,
    /// <summary>The walk moving the body toward a rightward target, its foot stepping backward (#2560).</summary>
    SecondPhaseWalkingRight,
    /// <summary>The walk moving the body toward a leftward target, its foot stepping forward (#2560).</summary>
    SecondPhaseWalkingLeft,
}
