using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of the escape Etecoon (<c>$B3:E655-$E730</c>). Its walking velocity and pre-instruction are values the
/// cartridge keeps in the enemy's common record, so a kill empties them with that record
/// (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class EscapeEtecoonEnemyState
{
    /// <summary>The actor's typed program cursor, in place of its native instruction-list pointer.</summary>
    internal AnimationProgramCursor<EscapeEtecoonProgram> Program { get; set; }


    /// <summary>
    /// The signed 8.8 walking velocity in pixels per update consumed by <c>$B3:E680</c>; only its X is meaningful. A
    /// wall reverses it by negating it.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; set; }

    /// <summary>The installed pre-instruction; none once a kill has emptied the record.</summary>
    public EscapeEtecoonPreInstruction? PreInstruction { get; set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: no velocity and no pre-instruction.</summary>
    internal void ClearWithRecord()
    {
        Velocity = default;
        PreInstruction = null;
    }
}
