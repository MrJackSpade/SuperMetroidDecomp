using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The state of a Rinka. The values the cartridge keeps in the enemy's common record are emptied with it when the enemy
/// is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class RinkaEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<RinkaProgram> Program { get; set; }

    /// <summary>
    /// Same-bank $A2 routine word dispatched by main AI, changed by initialization, the fire instruction, aim
    /// completion, or the special death tail; none once a kill has emptied the record.
    /// </summary>
    public RinkaEnemyFunction? Function { get; internal set; }

    /// <summary>Signed 8.8 velocity: X produced by Math_MultBySin, Y by Math_MultByCos.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Reservation token of the spawn location a special Mother Brain Rinka holds: one of the even tokens 2 through 22
    /// from $A2:B75B, or zero when it holds none.
    /// </summary>
    public int SpawnResourceToken
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>Wrapping 16-bit countdown used both before flight and before special respawn.</summary>
    public NativeCountdown DelayTimer { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        Velocity = default;
        SpawnResourceToken = 0;
        DelayTimer = NativeCountdown.Start(0);
    }
}
