using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The state of a Baby Turtle, with its required extension words. The values the game keeps in the enemy's common
/// record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class BabyTurtleEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<MamaTurtleProgram> Program { get; set; }

    /// <summary>
    /// Gets the current Baby Turtle AI function pointer; none once a kill has emptied the record.
    /// </summary>
    public BabyTurtleAiFunction? Function { get; internal set; }

    /// <summary>The parent Mama Turtle's slot ordinal.</summary>
    public int ParentSlotIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, EnemySlotOrder.Capacity - 1, value);
    }

    /// <summary>
    /// The child's spawn point: its spawn X, which the crawl turns around 48 pixels from, and the top of its spawn
    /// hitbox, which the crawl holds it to.
    /// </summary>
    public Position16<RoomPixels> SpawnTopCenter { get; internal set; }

    /// <summary>
    /// Signed whole-pixel velocity per update. The game keeps the horizontal speed in the common record, so a kill
    /// empties it; the vertical speed is an extension word and survives.
    /// </summary>
    public Offset16<RoomPixels> Velocity { get; internal set; }

    /// <summary>Gets the phase-specific Baby Turtle countdown.</summary>
    public int FunctionTimer { get; internal set; }
    /// <summary>Gets the reaction countdown used while the child is not carrying Samus.</summary>
    public int NotCarryingSamusReactionTimer { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        ParentSlotIndex = 0;
        SpawnTopCenter = Position16<RoomPixels>.Zero;
        Velocity = new(0, Velocity.Y);
    }
}
