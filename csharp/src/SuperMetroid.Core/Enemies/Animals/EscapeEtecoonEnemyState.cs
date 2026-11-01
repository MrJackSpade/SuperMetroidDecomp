using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The state of the escape Etecoon. Its walking velocity and pre-instruction are values the
/// game keeps in the enemy's common record, so a kill empties them with that record
/// (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class EscapeEtecoonEnemyState
{
    /// <summary>The actor's typed program cursor.</summary>
    internal AnimationProgramCursor<EscapeEtecoonProgram> Program { get; set; }


    /// <summary>
    /// The signed 8.8 walking velocity in pixels per update consumed by the walking behaviour; only its X is meaningful. A
    /// wall reverses it by negating it.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; set; }

    /// <summary>The installed pre-instruction; none once a kill has emptied the record.</summary>
    public EscapeEtecoonFrameBehavior? FrameBehavior { get; set; }

    /// <summary>Empties the values the game keeps in the enemy's common record: no velocity and no pre-instruction.</summary>
    internal void ClearWithRecord()
    {
        Velocity = default;
        FrameBehavior = null;
    }
}
