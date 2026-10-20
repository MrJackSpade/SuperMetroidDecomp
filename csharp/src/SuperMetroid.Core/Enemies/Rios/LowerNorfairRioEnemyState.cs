using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of the Lower Norfair Rio. This variant shares the physical parent/follower layout with $D2FF but uses
/// extra word $02 as a visibility switch. Its velocity, behaviour and follower selection are values the cartridge
/// keeps in the enemy's common record, so a kill empties them with that record (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class LowerNorfairRioEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<LowerNorfairRioProgram> Program { get; set; }

    /// <summary>The last animation installed for this actor, native <c>Holtz.instList</c> at <c>$7E:7800,x</c>; distinct from the advancing cursor and prevents timer resets when the same animation is requested again.</summary>
    internal LowerNorfairRioProgram InstalledProgram { get; set; }

    /// <summary>Extra word $01, set by private animation instruction $C6D2.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// Extra word $02, cleared/set by $C6DD/$C6E8. The following physical slot reads this
    /// word through the cartridge's spawn-data alias and shows its lower sprite only at one.
    /// </summary>
    public bool FollowerVisible { get; internal set; }

    /// <summary>Signed 8.8 velocity in room pixels per update.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The current behaviour, shared by parent and flame follower dispatch; none once a kill has emptied the record.</summary>
    public LowerNorfairRioEnemyFunction? Function { get; internal set; }

    /// <summary>Whether population parameter 1's sign bit selects the lower flame follower instead of the parent; a follower must immediately follow a parent of the same enemy definition.</summary>
    public bool IsFollower { get; internal set; }

    /// <summary>
    /// Empties the values the cartridge keeps in the enemy's common record: no velocity, no behaviour, and the parent
    /// selection of the cleared parameter. The animation signal and follower visibility outlive the kill.
    /// </summary>
    internal void ClearWithRecord()
    {
        Velocity = default;
        Function = null;
        IsFollower = false;
    }
}
