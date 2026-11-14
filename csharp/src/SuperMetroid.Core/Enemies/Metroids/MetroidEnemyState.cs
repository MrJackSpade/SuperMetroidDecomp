using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The state of an ordinary Metroid, with its three extra values: the two outer-body sprite objects it spawned and
/// its energy drain.
/// </summary>
public sealed class MetroidEnemyState
{
    /// <summary>Connects the Metroid's common slot to its two live outer-body sprite objects.</summary>
    internal MetroidEnemyState(RoomSpriteObject outerBodyA,
        RoomSpriteObject outerBodyB)
    {
        OuterBodyA = outerBodyA;
        OuterBodyB = outerBodyB;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MetroidSegment> Playback { get; set; }

    /// <summary>Signed 16.16 velocity in pixels per movement update, positive right and down.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Four-frame escape timer and frozen-shake countdown.</summary>
    public int EscapeTimer { get; internal set; }

    /// <summary>Zero-based movement dispatcher index 0..3, not an AI behaviour.</summary>
    public MetroidMovement Movement { get; internal set; }

    /// <summary>The electricity sprite object the Metroid spawned; its owner repositions, freezes and ends it alongside the central body.</summary>
    public RoomSpriteObject OuterBodyA { get; }
    /// <summary>The shell sprite object the Metroid spawned; distinct from both the electricity layer and the central body.</summary>
    public RoomSpriteObject OuterBodyB { get; }

    /// <summary>
    /// The Metroid's drain of Samus's energy, a fraction an update: it takes an energy whenever its suit's fraction is
    /// more than what is held over from the last energy it took (<see cref="FractionalEnergyDrain"/>).
    /// </summary>
    public FractionalEnergyDrain Drain { get; internal set; }

    /// <summary>
    /// Drains one update's <paramref name="fraction"/> of an energy and reports whether it takes a whole energy from
    /// Samus (<see cref="FractionalEnergyDrain.TakesEnergy"/>).
    /// </summary>
    /// <param name="fraction">The suit-selected fraction of an energy drained each update, in <see cref="FractionalEnergyDrain.PartsPerEnergy"/>ths.</param>
    internal bool DrainTakesEnergy(int fraction)
    {
        bool takesEnergy = Drain.TakesEnergy(fraction);
        Drain = Drain.After(fraction);
        return takesEnergy;
    }

    /// <summary>
    /// The ice damage the Metroid still absorbs before freezing, from the authored value; a shot that
    /// reaches it freezes the Metroid and empties it.
    /// </summary>
    public int IceDamageToFreeze
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
}
