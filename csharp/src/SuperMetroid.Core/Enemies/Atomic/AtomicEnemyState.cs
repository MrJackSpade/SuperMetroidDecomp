using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// The state of one Atomic: its two movement selections and its two speeds. Both speeds come whole from the shared
/// linear-speed record; in particular the negative one is the table's own value, not a negation of the positive one.
/// The movement selections are values the cartridge keeps in the enemy's common record, so they are emptied with that
/// record when the enemy is killed (<see cref="ClearWithRecord"/>); the speeds outlive the kill.
/// </summary>
public sealed class AtomicEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<AtomicProgram> Program { get; set; }

    /// <summary>
    /// The vertical movement chosen this update (native <c>Atomic.XMovementFunction</c>; despite that original name,
    /// the selected routine changes the actor's Y coordinate); none once a kill has emptied the record.
    /// </summary>
    public AtomicVerticalMovement? VerticalMovement { get; internal set; }

    /// <summary>
    /// The horizontal movement chosen this update (native <c>Atomic.YMovementFunction</c>; despite that original name,
    /// the selected routine changes the actor's X coordinate); none once a kill has emptied the record.
    /// </summary>
    public AtomicHorizontalMovement? HorizontalMovement { get; internal set; }

    /// <summary>Signed 16.16 room-pixel speed per AI update for down and right movement, the positive half of the linear-speed record selected by parameter two.</summary>
    public SnesSignedSixteenSixteen Speed { get; internal set; }

    /// <summary>Signed 16.16 room-pixel speed per AI update for up and left movement, the negative half of the same record.</summary>
    public SnesSignedSixteenSixteen NegativeSpeed { get; internal set; }

    /// <summary>Empties the movement selections the cartridge keeps in the enemy's common record, when a kill empties that record.</summary>
    internal void ClearWithRecord()
    {
        VerticalMovement = null;
        HorizontalMovement = null;
    }
}
