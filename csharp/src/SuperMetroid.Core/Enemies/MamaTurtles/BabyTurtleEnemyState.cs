using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The state of a Baby Turtle, with its required extension words. The values the cartridge keeps in the enemy's common
/// record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class BabyTurtleEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MamaTurtleProgram> Program { get; set; }

    /// <summary>
    /// Gets the current bank-$A2 Baby Turtle AI function pointer; none once a kill has emptied the record.
    /// </summary>
    public BabyTurtleAiFunction? Function { get; internal set; }

    /// <summary>The parent Mama Turtle's physical slot index.</summary>
    public int ParentSlotIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue / EnemySlotTable.NativeSlotSize, value);
    }

    /// <summary>
    /// The child's spawn point: its spawn X, which the crawl turns around 48 pixels from, and the top of its spawn
    /// hitbox, which the crawl holds it to.
    /// </summary>
    public Position16<RoomPixels> SpawnTopCenter { get; internal set; }

    /// <summary>
    /// Signed whole-pixel velocity per update. The cartridge keeps the horizontal speed in the common record, so a kill
    /// empties it; the vertical speed is an extension word and survives.
    /// </summary>
    public Offset16<RoomPixels> Velocity { get; internal set; }

    /// <summary>Gets the phase-specific Baby Turtle countdown.</summary>
    public WrappingCountdown FunctionTimer { get; internal set; }
    /// <summary>Gets the reaction countdown used while the child is not carrying Samus.</summary>
    public WrappingCountdown NotCarryingSamusReactionTimer { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        ParentSlotIndex = 0;
        SpawnTopCenter = Position16<RoomPixels>.Zero;
        Velocity = new(0, Velocity.Y);
    }
}
