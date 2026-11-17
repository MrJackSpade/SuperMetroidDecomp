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

    /// <summary>Signed 1/256-pixel velocity: X, Y.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>The phase of the membrane's pulse (#2259): each advance moves to the next of the eight phases.</summary>
    public ShitroidMembranePulse MembranePulse { get; internal set; } = ShitroidMembranePulse.First;

    /// <summary>
    /// Updates left before the membrane colours advance to its next phase; each update decrements it until zero, and
    /// the advance reloads it from <see cref="ColourDelay"/>. The decrement is guarded by the zero test, so it never
    /// passes zero.
    /// </summary>
    public int ColourTimer { get; internal set; }

    /// <summary>Updates between colour-cycle phases: ten at the ordinary pace, one while draining quickly.</summary>
    public int ColourDelay { get; internal set; }

    /// <summary>
    /// The wait of the current action that counts past zero: the entrance delay, the rises after feeding and after
    /// draining Samus, the pause after draining her, the flights beside her and the hold before her release (#2253).
    /// </summary>
    public BossCountdown StateTimer { get; } = new();

    /// <summary>
    /// The updates left of the Shitroid's drain of the Sidehopper, zero or more: the drain finishes on the update it
    /// reaches zero (#2253).
    /// </summary>
    public int SidehopperDrainTimer { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>The randomized vertical-follow duration left; it stops at zero.</summary>
    public int HoverTimer { get; internal set; }

    /// <summary>
    /// Horizontal-lock accumulator: gains two each update the Shitroid is horizontally
    /// aligned with its target and loses one, never below zero, otherwise.
    /// </summary>
    public int HorizontalLockAccumulator { get; internal set; }

    /// <summary>Whether the periodic Shitroid cry is enabled.</summary>
    public bool CryEnabled { get; internal set; }

    /// <summary>Membrane pulses toward the next periodic cry, 0 through 3.</summary>
    public int CryCounter { get; internal set; }

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
