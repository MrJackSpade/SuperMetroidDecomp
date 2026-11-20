using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The state of an ordinary Metroid, among it the two outer-body effect actors it spawned and its energy drain.
/// </summary>
public sealed class MetroidEnemyState
{
    /// <summary>Connects the Metroid to its two live outer-body effect actors.</summary>
    internal MetroidEnemyState(RoomEffectActor electricity,
        RoomEffectActor shell)
    {
        Electricity = electricity;
        Shell = shell;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MetroidSegment> Playback { get; set; }

    /// <summary>The Metroid's velocity in pixels per movement update, positive right and down.</summary>
    public FixedVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The escape a bomb shakes the Metroid loose into, at most <see cref="MetroidBehaviorDefinitions.EscapeDuration"/>
    /// moves and over when it is not escaping: the escape makes one an update, and while the Metroid is frozen it spends them straining against the ice, flashing. A freeze always
    /// turns the Metroid to chase Samus, so one thawed with none left chases rather than escaping on (#2373).
    /// </summary>
    public UpdateCountdown EscapeMovesLeft { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>The step of the Metroid's movement it is in, one of four; not an AI behaviour.</summary>
    public MetroidMovement Movement { get; internal set; }

    /// <summary>The electricity effect actor the Metroid spawned; its owner repositions, freezes and ends it alongside the central body.</summary>
    public RoomEffectActor Electricity { get; }
    /// <summary>The shell effect actor the Metroid spawned; distinct from both the electricity layer and the central body.</summary>
    public RoomEffectActor Shell { get; }

    /// <summary>
    /// The Metroid's drain of Samus's energy, a fraction an update: it takes an energy whenever its suit's fraction is
    /// more than what is held over from the last energy it took (<see cref="FractionalEnergyDrain"/>).
    /// </summary>
    internal FractionalEnergyDrain Drain { private get; set; }

    /// <summary>
    /// Drains one update's <paramref name="fraction"/> of an energy and reports whether it takes a whole energy from
    /// Samus (<see cref="FractionalEnergyDrain.TakesEnergy"/>).
    /// </summary>
    /// <param name="fraction">The suit-selected fraction of an energy drained each update.</param>
    internal bool DrainTakesEnergy(EnergyAmount fraction)
    {
        bool takesEnergy = Drain.TakesEnergy(fraction.FractionParts);
        Drain = Drain.After(fraction.FractionParts);
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
