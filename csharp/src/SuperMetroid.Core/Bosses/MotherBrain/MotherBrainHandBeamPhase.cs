namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Index into Mother Brain's four-entry hand-beam dispatcher at <c>$A9:B887</c>. Phase two
/// is deliberately bytecode-owned: the body list advances it only after its complete
/// charge/fire/hold animation has elapsed.
/// </summary>
public enum MotherBrainHandBeamPhase
{
    /// <summary>Index 0 requests the slow backward gait toward X $28, then sets the charging neck targets.</summary>
    BackUp = 0,
    /// <summary>Index 1 waits for every active Mother Brain bomb to leave the shared projectile pool before installing the hand-beam body list.</summary>
    WaitForBombs = 1,
    /// <summary>Index 2 is an inert body-AI wait; body bytecode owns charging, recursive projectile emission, the 240-update hold, and phase advance.</summary>
    Firing = 2,
    /// <summary>Index 3 restores the neutral head list and neck targets, resets this dispatcher, and returns to thinking.</summary>
    Finish = 3,
}
