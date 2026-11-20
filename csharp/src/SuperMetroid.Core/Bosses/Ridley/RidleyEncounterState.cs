using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of Ridley's encounters, Ceres's (<see cref="CeresRidleyState"/>) or Lower Norfair's (<see cref="NorfairRidleyState"/>),
/// as his body holds it from its initialization (#2228, #2231): the flight and tail both encounters share, his colours,
/// the reveal both open with and the phase timer every phase counts on.
/// </summary>
public abstract class RidleyEncounterState
{
    /// <summary>Begins an encounter whose body and tail start as given.</summary>
    /// <param name="flight">His body in flight.</param>
    /// <param name="tail">His tail.</param>
    private protected RidleyEncounterState(RidleyFlightState flight, RidleyTailState tail)
    {
        Flight = flight;
        Tail = tail;
    }

    /// <summary>His body in flight: facing, motion, wings, swoop and body animation.</summary>
    public RidleyFlightState Flight { get; }

    /// <summary>His tail.</summary>
    public RidleyTailState Tail { get; }

    /// <summary>The colours the encounter has loaded over its sprites and its room (#1512).</summary>
    internal RidleyColours Colours { get; } = new();

    /// <summary>Gets the encounter mode: not fighting during the reveal, fighting or dodging a power bomb in combat, and dying during Lower Norfair's death.</summary>
    public RidleyFightMode FightMode { get; internal set; }

    /// <summary>Gets whether body motion and the wing and tail updates run; Ceres's eye fade enables them, Lower Norfair's death hides them.</summary>
    public bool MovementAnimationEnabled { get; internal set; }

    /// <summary>The current AI phase's countdown, which times how long the phase lasts (#2233).</summary>
    public BossCountdown PhaseTimer { get; } = new();

    /// <summary>Which of his drawn parts show his hurt flash's colours, as his step last picked them (#2562).</summary>
    internal RidleyHurtFlashShowing HurtFlashShows { private get; set; }

    /// <summary>The shading Ridley's tail and wings draw in: his own colours, or the hurt flash's (#1512).</summary>
    public PictureShading TailAndWingsShading => Colours.SpriteShading(HurtFlashShows.TailAndWings);

    /// <summary>The shading the common enemy draw path draws Ridley's body in (#1512).</summary>
    public PictureShading CommonDrawShading => Colours.SpriteShading(HurtFlashShows.Body);

    /// <summary>Whether Ridley holds Samus in his claws, which keeps his tail tip from striking her; only Lower Norfair Ridley grabs.</summary>
    internal abstract bool CarriesSamus { get; }

    /// <summary>
    /// Whether Ridley has lifted off (#2227), ending the reveal both encounters open with: until then he runs a
    /// <see cref="RevealPhase"/>, and from then on his encounter's own phase. Setting the encounter's phase lifts him off.
    /// </summary>
    public bool LiftedOff { get; private set; }

    /// <summary>The phase of the reveal Ridley runs before he lifts off; he starts waiting for Samus's door.</summary>
    /// <exception cref="InvalidOperationException">He has lifted off.</exception>
    public RidleyRevealPhase RevealPhase
    {
        get => !LiftedOff ? field : throw LiftedOffAlready();
        internal set => field = !LiftedOff ? value : throw LiftedOffAlready();
    }

    /// <summary>
    /// His eyes' fade in (#2228, #2247, #2409), played an update at a time while his reveal fades them in; it starts before
    /// its first shade, and his body's fade begins once it rests.
    /// </summary>
    internal AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> EyeFade { get; } = RidleyFadeAnimations.EyesFadingIn();

    /// <summary>
    /// His body's fade in (#2228, #2409), played an update at a time while his reveal fades it in; it starts before its
    /// first shade, black, and rests on its last, his own colours.
    /// </summary>
    internal AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> BodyFade { get; } = RidleyFadeAnimations.BodyFadingIn();

    /// <summary>Ends the reveal as the encounter's phase is set.</summary>
    private protected void LiftOff() => LiftedOff = true;

    /// <summary>The failure of reading an encounter's phase while Ridley is still in his reveal.</summary>
    private protected InvalidOperationException NotLiftedOff() =>
        new($"Ridley has not lifted off; he runs reveal phase {RevealPhase}.");

    /// <summary>The failure of reading or setting a reveal phase once Ridley has lifted off.</summary>
    private static InvalidOperationException LiftedOffAlready() =>
        new($"Ridley has lifted off; he runs no {nameof(RidleyRevealPhase)}.");
}
