using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of an Etecoon. The values the cartridge keeps in the enemy's common record are emptied with it when the
/// enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class EtecoonEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<EtecoonProgram> Program { get; set; }

    /// <summary>
    /// Signed 16.16 velocity in room pixels per update.
    /// </summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Wrapped countdown.</summary>
    public NativeCountdown FunctionTimer { get; internal set; }

    /// <summary>Bank-$A7 function address; none once a kill has emptied the record.</summary>
    public EtecoonAiFunction? Function { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Velocity = FixedVector<RoomPixels>.Zero;
        FunctionTimer = NativeCountdown.Start(0);
        Function = null;
    }
}
