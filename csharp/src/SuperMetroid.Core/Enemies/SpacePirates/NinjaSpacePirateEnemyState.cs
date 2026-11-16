using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The state of one ninja Pirate. The speed and spawn position are the ninja's own state, which nothing else reads.
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

    /// <summary>The active stance animation, facing Samus, that becoming active continues at.</summary>
    internal NinjaSpacePirateSegment ActiveAnimation { get; set; }

    /// <summary>
    /// The room column midway between the adjusted posts. Samus and the Pirate are compared with it by the wrapped
    /// difference.
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
    /// The current signed 1/256-pixel speed in 1/256 pixel per update: the spin jump's horizontal speed, which rises by 32 per
    /// update until the midpoint and falls back to zero, or the divekick's vertical speed, which starts at 1,536 and
    /// falls by 64 per update. Its sign is load-bearing: the divekick rise ends on the update the speed turns negative.
    /// </summary>
    public SignedEightEight Speed { get; internal set; }

    /// <summary>
    /// The room position recorded at initialization, once the Pirate stands at its starting post; a divekick landing
    /// restores its Y.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>
    /// Changes <see cref="Speed"/> by <paramref name="delta"/>; a change past the fastest speed one way carries on from
    /// the fastest speed the other way.
    /// </summary>
    internal void ChangeSpeed(SignedEightEight delta) => Speed += delta;
}
