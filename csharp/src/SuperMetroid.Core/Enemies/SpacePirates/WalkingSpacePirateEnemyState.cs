using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of one walking Space Pirate. The patrol posts are room positions that wrap at sixteen bits, because
/// population parameter two is added and subtracted without clamping by <c>$B2:FD02</c>; the walk compares the Pirate
/// with them by the wrapped spatial difference. The behaviour and posts are values the cartridge keeps in the enemy's
/// common record, so a kill empties them with that record (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class WalkingSpacePirateEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WalkingSpacePirateProgram> Program { get; set; }

    /// <summary>The current behaviour; none once a kill has emptied the record.</summary>
    public WalkingSpacePirateFunction? Function { get; internal set; }

    /// <summary>
    /// Left patrol post. Only the X axis is stored and meaningful (Y reads zero): a leftward walk turns back once the
    /// Pirate passes it.
    /// </summary>
    public Position16<RoomPixels> LeftPost
    {
        get;
        internal set => field = new(value.X, 0);
    }

    /// <summary>
    /// Right patrol post. Only the X axis is stored and meaningful (Y reads zero): a rightward walk turns back once the
    /// Pirate reaches it.
    /// </summary>
    public Position16<RoomPixels> RightPost
    {
        get;
        internal set => field = new(value.X, 0);
    }

    /// <summary>Successful allocations from the actor's three-shot firing animations.</summary>
    public int SpawnedLaserCount { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: no behaviour and both posts at zero.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        LeftPost = Position16<RoomPixels>.Zero;
        RightPost = Position16<RoomPixels>.Zero;
    }
}
