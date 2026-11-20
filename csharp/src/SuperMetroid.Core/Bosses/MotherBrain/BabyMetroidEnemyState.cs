using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Animation;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid's own state: its animation, motion, colours and what it does next.</summary>
public sealed class BabyMetroidEnemyState
{
    /// <summary>The Baby Metroid's animation and where it stands (#641, #1600).</summary>
    internal AnimationPlayback<BabyMetroidSegment> Playback { get; set; }

    /// <summary>Starts the state in <paramref name="action"/>, playing the animation starting at <paramref name="animation"/> (#2319).</summary>
    internal BabyMetroidEnemyState(BabyMetroidAction action, BabyMetroidSegment animation)
    {
        Action = action;
        Playback = BabyMetroidAnimations.Starting(animation);
    }

    /// <summary>Gets the Baby Metroid's current action.</summary>
    public BabyMetroidAction Action { get; internal set; }

    /// <summary>The side of Samus the departing Baby Metroid flies to (#2165).</summary>
    public HorizontalDirection DepartureSide { get; internal set; }

    /// <summary>Velocity in pixels per update.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The membrane's pulse playing (#2259, #2404): its eight phases looping at its pace
    /// (<see cref="BabyMetroidMembraneAnimations"/>). It starts on its first phase, which it leaves on its first update, so
    /// the first phase it shows is its second.
    /// </summary>
    internal AnimationPlayer<BabyMetroidMembranePace, BabyMetroidMembranePulse, BabyMetroidMembraneCue> Membrane { get; private set; } =
        BabyMetroidMembraneAnimations.StandingOn(BabyMetroidMembranePace.Calm, BabyMetroidMembranePulse.First, 1);

    /// <summary>The pace of the membrane's pulse: calm, or quick while it feeds.</summary>
    public BabyMetroidMembranePace MembranePace => Membrane.Playback.PlayingSegment;

    /// <summary>
    /// Sets the membrane's pulse to <paramref name="pace"/>: the phase showing keeps the updates it has left, and the phases
    /// after it show at the new pace.
    /// </summary>
    internal void PaceMembrane(BabyMetroidMembranePace pace)
    {
        Membrane.GoOnIn(BabyMetroidMembraneAnimations.At(pace));
    }

    /// <summary>
    /// The current action's wait: the entrance delay, the rises after feeding and after draining Samus, the pause after
    /// draining her, the flights beside her and the hold before her release (#2253).
    /// </summary>
    public BossCountdown StateTimer { get; } = new();

    /// <summary>The Baby Metroid's drain of the Sidehopper, which finishes on the update it runs out on (#2253, #2469).</summary>
    public BossCountdown SidehopperDrain { get; } = new();

    /// <summary>
    /// Its hover at Samus's height (#2469), which a random restart starts and which runs until the update it runs out on;
    /// null while it hovers at its own hover height.
    /// </summary>
    public BossCountdown? SamusHeightHover { get; internal set; }

    /// <summary>The Baby Metroid's horizontal lock on its target, built while it keeps aligned with it.</summary>
    public BabyMetroidHorizontalLock HorizontalLock { get; } = new();

    /// <summary>Whether the periodic Baby Metroid cry is enabled.</summary>
    public bool CryEnabled { get; internal set; }

    /// <summary>
    /// The cry pulses before its next periodic cry (#2469), counted one a pulse reaching its cry phase while its cry is
    /// enabled: it cries on the pulse the wait runs out on, and the wait starts again.
    /// </summary>
    public BossCountdown CryWait { get; } = BossCountdown.Lasting(BabyMetroidFightDefinitions.PulsesPerCry);

    /// <summary>The membrane pulse phase last shown, or null before its first step (#1512).</summary>
    internal BabyMetroidMembranePulse? MembraneShown { get; set; }

    /// <summary>The shading the Baby Metroid draws in: its entry's colours, with the membrane's pulse phase over them once shown.</summary>
    internal PictureShading Shading => MembraneShown is { } pulse
        ? BabyMetroidShades.InTargetColours.With(BabyMetroidShades.Membrane(pulse))
        : BabyMetroidShades.InTargetColours;

    /// <summary>
    /// Whether Samus's touch, shots and power bombs reach the Baby Metroid's private reactions: set while
    /// it hovers near Samus or follows her after a release, cleared while it is dormant or latched on.
    /// </summary>
    public bool ReactsToSamus { get; internal set; }
}
