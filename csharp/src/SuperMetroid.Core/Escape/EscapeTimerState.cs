namespace SuperMetroid.Core.Escape;

/// <summary>Low-byte entries in the dispatch table at <c>$80:9DFB</c>.</summary>
public enum EscapeTimerState
{
    /// <summary>Dispatch state 0, native <c>ProcessTimer_Inactive</c> at <c>$80:9E1A</c>: leaves timer fields unchanged and reports no expiration.</summary>
    Inactive = 0,
    /// <summary>Dispatch state 1, native <c>ProcessTimer_CeresStart</c> at <c>$80:9E09</c>: clears timer-owned state, installs 01:00.00 in packed BCD, and activates the initial delay as raw status <c>$8003</c>.</summary>
    CeresStart = 1,
    /// <summary>Dispatch state 2, native <c>ProcessTimer_MotherBrainStart</c> at <c>$80:9E1C</c>: clears timer-owned state, installs 03:00.00 in packed BCD, and activates the initial delay as raw status <c>$8003</c>.</summary>
    MotherBrainStart = 2,
    /// <summary>Dispatch state 3, native <c>ProcessTimer_InitialDelay</c> at <c>$80:9E2F</c>: increments the aliased X low-byte counter to <c>$10</c> without decrementing time, then selects the running movement delay.</summary>
    InitialDelay = 3,
    /// <summary>Dispatch state 4, native <c>ProcessTimer_RunningMovementDelayed</c> at <c>$80:9E41</c>: counts down time at the original center position while the X low-byte counter advances to <c>$60</c>, then clears that fraction and starts movement.</summary>
    RunningMovementDelayed = 4,
    /// <summary>Dispatch state 5, native <c>ProcessTimer_RunningMovingIntoPlace</c> at <c>$80:9E58</c>: decrements time while moving the 8.8 display origin toward screen pixel (220, 48), selecting state 6 after both axes clamp.</summary>
    RunningMovingIntoPlace = 5,
    /// <summary>Dispatch state 6, native <c>ProcessTimer_RunningMovingIntoPlace_return</c> at <c>$80:9E89</c>: decrements packed-BCD time at the settled display origin without further movement.</summary>
    RunningInPlace = 6,
}
