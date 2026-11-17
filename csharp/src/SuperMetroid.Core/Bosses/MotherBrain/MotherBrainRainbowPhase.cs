namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's rainbow beam attack (#2208): charging, firing, pinning and draining Samus, and deciding what comes next. Its values are arbitrary.</summary>
public enum MotherBrainRainbowPhase
{
    /// <summary>Sets the initial extended neck geometry and starts the neck's extension before the charge.</summary>
    ExtendNeck,

    /// <summary>Waits out the extension, installs the charging head animation, and falls into backward-walk/neck retraction.</summary>
    StartCharging,

    /// <summary>Requests really-slow backward walking toward X 40, then retracts the head and starts the charge wait.</summary>
    RetractNeck,

    /// <summary>Waits out the charge, queues the charge sound, and falls into downward neck extension.</summary>
    WaitForCharge,

    /// <summary>Sets downward neck targets and projectile cooldown, starts the neck's lowering before firing, and immediately runs its first update.</summary>
    ExtendNeckDown,

    /// <summary>Widens/aims the beam while an active Power Bomb freezes its countdown, then locks Samus and starts the active beam.</summary>
    StartFiring,

    /// <summary>Maintains beam effects and forced wallward movement until Samus reaches the wall, which selects the one-update delay.</summary>
    MoveSamusTowardWall,

    /// <summary>Runs another beam/movement call, runs its one-update delay out, and starts the pinning earthquake before draining setup.</summary>
    OneFrameDelay,

    /// <summary>Starts the drain and its earthquake and immediately executes the first draining update.</summary>
    StartDrainingSamus,

    /// <summary>Applies beam damage, ammunition drain, forced wall-centering movement, and effects until the drain runs out.</summary>
    DrainingSamus,

    /// <summary>Narrows the beam to an angular width of an eighth of a turn, stops the beam effect, sound and earthquake, releases input, and starts the falling handoff.</summary>
    FinishFiring,

    /// <summary>Puts Samus in her drained pose and movement and takes the first custom falling movement step at once.</summary>
    LetSamusFall,

    /// <summary>Advances forced falling until Samus lands.</summary>
    WaitForSamusToLand,

    /// <summary>Lowers the neck targets and starts the pause before the next action.</summary>
    LowerHead,

    /// <summary>After the pause runs out, repeats the beam at health 400 or above; otherwise begins the low-health finish-off cutscene.</summary>
    DecideNextAction,
}
