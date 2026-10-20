using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of one Norfair Rio, parent or flame follower. The velocity, function and follower selection are values
/// the cartridge keeps in the enemy's common record, so they are emptied with that record when the enemy is killed
/// (<see cref="ClearWithRecord"/>); the installed animation, handshake and flame offset outlive the kill.
/// </summary>
public sealed class NorfairRioEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<NorfairRioProgram> Program { get; set; }

    /// <summary>Extra word $00: the last animation installed by function seven.</summary>
    internal NorfairRioProgram InstalledProgram { get; set; }

    /// <summary>Extra word $01: animation handshake set by the swoop-start-finished cue ($C1C9).</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// The flame follower's placement relative to its parent; only Y is meaningful (-16 through 12 pixels). Private
    /// animation cues publish it, and its sign selects the follower's ascending or descending flames.
    /// </summary>
    public Offset16<RoomPixels> FollowerOffset { get; internal set; }

    /// <summary>Signed 8.8 velocity; the horizontal component feeds the 16.16 collision helper.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The behaviour shared by parent and flame-follower dispatch; none once a kill has emptied the record.</summary>
    public NorfairRioEnemyFunction? Function { get; internal set; }

    /// <summary>Whether population parameter 1's sign bit selects the flame follower instead of the parent; a follower must immediately follow a parent of this same enemy definition.</summary>
    public bool IsFollower { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record.</summary>
    internal void ClearWithRecord()
    {
        Velocity = new EightEightVector<RoomPixels>(0, 0);
        Function = null;
        IsFollower = false;
    }
}
