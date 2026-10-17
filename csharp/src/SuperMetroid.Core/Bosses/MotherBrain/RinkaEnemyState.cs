using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

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

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<RinkaProgram> Program { get; set; }

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
    /// Reservation token of the spawn location a special Mother Brain Rinka holds, kept in
    /// variable D: one of the even tokens 2 through 22 from $A2:B75B, or zero when it holds none.
    /// </summary>
    public int SpawnResourceToken
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = checked((ushort)value);
    }

    /// <summary>Wrapping 16-bit countdown used both before flight and before special respawn.</summary>
    public NativeCountdown DelayTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableF);
        internal set => _slot.VariableF = value.RawValue;
    }
}
