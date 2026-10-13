namespace SuperMetroid.Core.Game;

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

/// <summary>
/// Named projection of Rinka's six common AI variables. Keeping these properties backed by
/// the physical enemy slot makes debugger watches agree with WRAM $0FA8-$0FB2 while avoiding
/// unexplained VariableA/VariableB accesses throughout the translated state machine.
/// </summary>
public sealed class RinkaEnemyState
{
    /// <summary>Physical enemy slot retaining Rinka's dispatch, velocity, spawn token, and timer words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects the bank-$A2 Rinka variables over its live native enemy slot.</summary>
    internal RinkaEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Common variable A ($7E:0FA8 plus slot offset): same-bank $A2 routine word dispatched by main AI, changed by initialization, the fire instruction, aim completion, or the special death tail.</summary>
    public RinkaEnemyFunction Function
    {
        get => (RinkaEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Signed 8.8 velocity: X produced by Math_MultBySin in variable B, Y by Math_MultByCos in
    /// variable C.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableB), new SnesSignedEightEight(_slot.VariableC));
        internal set
        {
            _slot.VariableB = value.X.RawValue;
            _slot.VariableC = value.Y.RawValue;
        }
    }

    /// <summary>
    /// Native extra-RAM selector stored in variable D. Special Mother Brain Rinkas use the
    /// even values $0002..$0016 from $A2:B75B; zero means no spawn resource is reserved.
    /// </summary>
    public ushort SpawnResourceToken
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Wrapping 16-bit countdown used both before flight and before special respawn.</summary>
    public NativeCountdown DelayTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableF);
        internal set => _slot.VariableF = value.RawValue;
    }
}

/// <summary>One literal (X, Y, extra-RAM selector) record from $A2:B75B.</summary>
/// <param name="Position">Authored spawn centre in whole room pixels (not an OAM or camera-relative coordinate), tested against the current layer-one camera when reserving a Mother Brain Rinka location.</param>
/// <param name="Token">Native spawn-availability byte selector plus two: even $0002..$0016 for the eleven table locations, reserved until the owning actor leaves, dies, or is terminated.</param>
public readonly record struct RinkaSpawnResource(
    Position16<RoomPixels> Position,
    ushort Token);
