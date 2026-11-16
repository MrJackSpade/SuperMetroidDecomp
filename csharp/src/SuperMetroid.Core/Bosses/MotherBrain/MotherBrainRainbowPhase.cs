namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's rainbow beam attack (#2208): charging, firing, pinning and draining Samus, and deciding what comes next. Its values are arbitrary.</summary>
public enum MotherBrainRainbowPhase
{
    /// <summary>Sets the initial extended neck geometry and installs the 256-count charging-start pause.</summary>
    ExtendNeck,

    /// <summary>Expires the extension pause, installs the charging head animation, and falls into backward-walk/neck retraction.</summary>
    StartCharging,

    /// <summary>Requests really-slow backward walking toward X 40, then retracts the head and starts a 256-count charge wait.</summary>
    RetractNeck,

    /// <summary>Waits for the timer to pass zero, queues the charge sound, and falls into downward neck extension.</summary>
    WaitForCharge,

    /// <summary>Sets downward neck targets and projectile cooldown, seeds the 16-count firing delay, and immediately runs its first call.</summary>
    ExtendNeckDown,

    /// <summary>Widens/aims the beam while an active Power Bomb freezes its countdown, then locks Samus and starts the active beam.</summary>
    StartFiring,

    /// <summary>Maintains beam effects and forced wallward movement until Samus reaches the wall, which selects the one-call delay.</summary>
    MoveSamusTowardWall,

    /// <summary>Runs another beam/movement call, runs its zero timer out, and starts an eight-count earthquake before draining setup.</summary>
    OneFrameDelay,

    /// <summary>Seeds both drain and earthquake timers with 299 and immediately executes the first draining call.</summary>
    StartDrainingSamus,

    /// <summary>Applies beam damage, ammunition drain, forced wall-centering movement, and effects until the timer runs out.</summary>
    DrainingSamus,

    /// <summary>Narrows the beam to an angular width of an eighth of a turn, stops the beam effect, sound and earthquake, releases input, and starts the falling handoff.</summary>
    FinishFiring,

    /// <summary>Changes the drained Samus controller/pose and falls directly into the first custom falling movement call.</summary>
    LetSamusFall,

    /// <summary>Advances forced falling until Samus lands.</summary>
    WaitForSamusToLand,

    /// <summary>Lowers the neck targets and installs the 128-count next-action pause.</summary>
    LowerHead,

    /// <summary>After the timer runs out, repeats the beam at health 400 or above; otherwise begins the low-health finish-off cutscene.</summary>
    DecideNextAction,
}
