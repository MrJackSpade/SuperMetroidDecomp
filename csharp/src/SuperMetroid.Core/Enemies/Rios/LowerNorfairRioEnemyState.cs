using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The state of the Lower Norfair Rio. This variant shares the physical parent/follower layout with the Norfair Rio but
/// uses extra word two as a visibility switch.
/// </summary>
public sealed class LowerNorfairRioEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<LowerNorfairRioProgram> Playback { get; set; }

    /// <summary>The last animation installed for this actor; distinct from the advancing cursor and prevents timer resets when the same animation is requested again.</summary>
    internal LowerNorfairRioProgram InstalledProgram { get; set; }

    /// <summary>Extra word one, set by a private animation cue.</summary>
    public bool AnimationSignal { get; internal set; }

    /// <summary>
    /// Extra word two, cleared/set by its hide and show cues. The following physical slot reads this
    /// word through the game's spawn-data alias and shows its lower sprite only at one.
    /// </summary>
    public bool FollowerVisible { get; internal set; }

    /// <summary>Signed 8.8 velocity in room pixels per update.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The current behaviour, shared by parent and flame follower dispatch.</summary>
    public LowerNorfairRioEnemyFunction? Function { get; internal set; }

    /// <summary>Whether population parameter 1's sign bit selects the lower flame follower instead of the parent; a follower must immediately follow a parent of the same enemy definition.</summary>
    public bool IsFollower { get; internal set; }
}
