using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of one ninja Pirate: its speeds and where it spawned.
/// </summary>
public sealed class NinjaSpacePirateEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal NinjaSpacePirateEnemyState(NinjaSpacePirateAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<NinjaSpacePirateSegment> Playback { get; set; }

    /// <summary>Movement/decision action.</summary>
    public NinjaSpacePirateAction Action { get; internal set; }

    /// <summary>The way the Pirate spin jumps, divekicks or walks back to its post (#2165).</summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>
    /// The room column midway between the adjusted posts, which every placement sets inside the room: the Pirate's spin
    /// jump turns to its fall on crossing it, and Samus near it sets off the spin jump and the divekick.
    /// </summary>
    public int PostsMidpointX { get; internal set; }

    /// <summary>
    /// The adjusted left post's room column: a Pirate whose X equals it stands at the left post, and a return walk ends
    /// on passing it.
    /// </summary>
    public int LeftPostX { get; internal set; }

    /// <summary>The adjusted right post's room column: a return walk ends on reaching or passing it.</summary>
    public int RightPostX { get; internal set; }

    /// <summary>
    /// The spin jump's horizontal speed (#2453): none as the spin jump starts, rising by its speed change each update until
    /// the Pirate crosses the posts' midpoint, then falling back by it to nothing, where the Pirate lands.
    /// </summary>
    public SignedEightEight SpinSpeed { get; internal set; }

    /// <summary>
    /// The divekick's vertical speed (#2453): its start speed as the divekick's jump and its dive begin, falling by its speed
    /// change each update. The jump rises by it and ends on the update it turns negative; the dive falls by it and ends at
    /// the floor or when it runs out.
    /// </summary>
    public SignedEightEight DiveSpeed { get; internal set; }

    /// <summary>
    /// The room position recorded at initialization, once the Pirate stands at its starting post; a divekick landing
    /// restores its Y.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

}
