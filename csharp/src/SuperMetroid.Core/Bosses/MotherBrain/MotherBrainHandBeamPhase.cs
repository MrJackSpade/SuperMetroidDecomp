namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The step of Mother Brain's hand-beam attack. <see cref="Firing"/> is deliberately owned by the body animation: the body list
/// advances it only after its complete charge/fire/hold animation has elapsed. Its values are arbitrary.
/// </summary>
public enum MotherBrainHandBeamPhase
{
    /// <summary>Requests the slow backward gait toward X 40, then sets the charging neck targets. A new attack starts here.</summary>
    BackUp,
    /// <summary>Waits for every active Mother Brain bomb to leave the shared projectile pool before installing the hand-beam body list.</summary>
    WaitForBombs,
    /// <summary>An inert body-AI wait; the body animation owns charging, recursive projectile emission, the 240-update hold, and phase advance.</summary>
    Firing,
    /// <summary>Restores the neutral head list and neck targets, resets the attack, and returns to thinking.</summary>
    Finish,
}
