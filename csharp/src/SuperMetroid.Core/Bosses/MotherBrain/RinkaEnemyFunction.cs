namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Exact same-bank function words dispatched by Rinka main AI at $A2:B7C4.</summary>
public enum RinkaEnemyFunction : ushort
{
    /// <summary>Function_Rinka_Fire, $A2:B7DF: decrements the initially 26-update timer through signed underflow, then samples Samus once to form signed 8.8 velocities at nominal $0120 speed; movement begins on the following AI update.</summary>
    AimDelay = 0xb7df,
    /// <summary>Function_Rinka_Killed, $A2:B844: hidden Mother Brain variant waits for its timer's signed underflow (1, 0, $FFFF), restores ten health, and reinitializes at a reserved spawn point.</summary>
    DeathRespawnDelay = 0xb844,
    /// <summary>Function_Rinka_WaitingToFire, $A2:B852: stationary initial state that recycles an actor outside the expanded camera flight rectangle; animation instruction $B9C7, not this function's timer, starts the aim delay.</summary>
    WatchForLeavingViewport = 0xb852,
    /// <summary>Function_Rinka_Moving, $A2:B85B: adds the fixed signed 8.8 velocities to the actor's fractional room position each update, then recycles it outside the camera-relative -16..271 X / -16..239 Y window without re-aiming.</summary>
    Flying = 0xb85b,
}
