using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Animation;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Shitroid's own state: its animation, motion, colours and what it does next.</summary>
public sealed class ShitroidEnemyState
{
    /// <summary>The Shitroid's animation and where it stands (#641, #1600).</summary>
    internal AnimationPlayback<ShitroidSegment> Playback { get; set; }

    /// <summary>Starts the state in <paramref name="action"/>, playing the animation starting at <paramref name="animation"/> (#2319).</summary>
    internal ShitroidEnemyState(ShitroidAction action, ShitroidSegment animation)
    {
        Action = action;
        Playback = ShitroidAnimations.Starting(animation);
    }

    /// <summary>Gets the Shitroid's current action.</summary>
    public ShitroidAction Action { get; internal set; }

    /// <summary>The side of Samus the departing Shitroid flies to (#2165).</summary>
    public HorizontalDirection DepartureSide { get; internal set; }

    /// <summary>Velocity in pixels per update.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The membrane's pulse playing (#2259, #2404): its eight phases looping at its pace
    /// (<see cref="ShitroidMembraneAnimations"/>). It starts on its first phase, which it leaves on its first update, so
    /// the first phase it shows is its second.
    /// </summary>
    internal AnimationPlayer<ShitroidMembranePace, ShitroidMembranePulse, ShitroidMembraneCue> Membrane { get; set; } =
        ShitroidMembraneAnimations.StandingOn(ShitroidMembranePace.Calm, ShitroidMembranePulse.First, 1);

    /// <summary>The pace of the membrane's pulse: calm, or quick while it feeds.</summary>
    public ShitroidMembranePace MembranePace { get; internal set; }

    /// <summary>
    /// Sets the membrane's pulse to <paramref name="pace"/>: the phase showing keeps the updates it has left, and the phases
    /// after it show at the new pace.
    /// </summary>
    internal void PaceMembrane(ShitroidMembranePace pace)
    {
        MembranePace = pace;
        Membrane.GoOnIn(ShitroidMembraneAnimations.At(pace));
    }

    /// <summary>
    /// The wait of the current action that counts past zero: the entrance delay, the rises after feeding and after
    /// draining Samus, the pause after draining her, the flights beside her and the hold before her release (#2253).
    /// </summary>
    public BossCountdown StateTimer { get; } = new();

    /// <summary>The Shitroid's drain of the Sidehopper, which finishes on the update it runs out on (#2253, #2469).</summary>
    public BossCountdown SidehopperDrain { get; } = new();

    /// <summary>
    /// Its hover at Samus's height (#2469), which a random restart starts and which runs until the update it runs out on;
    /// null while it hovers at its own hover height.
    /// </summary>
    public BossCountdown? SamusHeightHover { get; internal set; }

    /// <summary>
    /// Horizontal-lock accumulator: gains two each update the Shitroid is horizontally
    /// aligned with its target and loses one, never below zero, otherwise.
    /// </summary>
    public int HorizontalLockAccumulator { get; internal set; }

    /// <summary>Whether the periodic Shitroid cry is enabled.</summary>
    public bool CryEnabled { get; internal set; }

    /// <summary>
    /// The cry pulses before its next periodic cry (#2469), counted one a pulse reaching its cry phase while its cry is
    /// enabled: it cries on the pulse the wait runs out on, and the wait starts again.
    /// </summary>
    public BossCountdown CryWait { get; } = BossCountdown.Lasting(ShitroidFightDefinitions.PulsesPerCry);

    /// <summary>The membrane pulse phase last shown, or null before its first step (#1512).</summary>
    internal ShitroidMembranePulse? MembraneShown { get; set; }

    /// <summary>The shading the Shitroid draws in: its entry's colours, with the membrane's pulse phase over them once shown.</summary>
    internal PictureShading Shading => MembraneShown is { } pulse
        ? ShitroidShades.InTargetColours.With(ShitroidShades.Membrane(pulse))
        : ShitroidShades.InTargetColours;

    /// <summary>
    /// Whether Samus's touch, shots and power bombs reach the Shitroid's private reactions: set while
    /// it hovers near Samus or follows her after a release, cleared while it is dormant or latched on.
    /// </summary>
    public bool ReactsToSamus { get; internal set; }
}
