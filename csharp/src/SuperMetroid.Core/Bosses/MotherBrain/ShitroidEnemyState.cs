using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The state of the Shitroid, with its extended enemy state.</summary>
public sealed class ShitroidEnemyState
{
    /// <summary>Creates the state for the Shitroid's slot.</summary>
    internal ShitroidEnemyState(RoomEnemySlot slot) => Slot = slot;

    /// <summary>The Shitroid's enemy slot.</summary>
    public RoomEnemySlot Slot { get; }

    /// <summary>Where the Shitroid's program resumes (#641).</summary>
    internal AnimationProgramCursor<ShitroidProgram> Cursor { get; set; }

    /// <summary>Gets the current Shitroid AI function pointer.</summary>
    public ShitroidAiFunction? Function { get; internal set; }

    /// <summary>Signed 8.8 velocity: X, Y.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Membrane palette-cycle phase, 0 through 7: each advance moves to the next of the eight phases.</summary>
    public int PalettePhase { get; internal set; }

    /// <summary>
    /// Updates left before the membrane palette advances to its next phase; each update decrements it until zero, and
    /// the advance reloads it from <see cref="PaletteDelay"/>. The decrement is guarded by the zero test, so it never
    /// passes zero.
    /// </summary>
    public int PaletteTimer { get; internal set; }

    /// <summary>Updates between palette-cycle phases: ten at the ordinary pace, one while draining quickly.</summary>
    public int PaletteDelay { get; internal set; }

    /// <summary>Gets the current AI state's general-purpose countdown.</summary>
    public int StateTimer { get; internal set; }

    /// <summary>The randomized vertical-follow duration left; it stops at zero.</summary>
    public int HoverTimer { get; internal set; }

    /// <summary>
    /// Horizontal-lock accumulator (extended word 02): gains two each update the Shitroid is horizontally
    /// aligned with its target and loses one, never below zero, otherwise.
    /// </summary>
    public int HorizontalLockAccumulator { get; internal set; }

    /// <summary>Whether the periodic Shitroid cry is enabled (extended word 04).</summary>
    public bool CryEnabled { get; internal set; }

    /// <summary>Palette cycles toward the next periodic cry, 0 through 3 (extended word 05).</summary>
    public int CryCounter { get; internal set; }

    /// <summary>
    /// Diagnostic mirror of its victim's palette stage. Shitroid writes the actual dead-sidehopper palette
    /// stage through <c>ActivateDeadSidehopperVictim</c>; retaining this value on the boss state makes that cross-slot
    /// side effect directly auditable.
    /// </summary>
    public bool VictimActivationFlag { get; internal set; }
    /// <summary>The membrane pulse phase last shown, or null before its first step (#1512).</summary>
    internal int? MembraneShown { get; set; }

    /// <summary>The shading the Shitroid draws in: its entry's colours, with the membrane's pulse phase over them once shown.</summary>
    internal PictureShading Shading => MembraneShown is int phase
        ? ShitroidShades.InTargetColours.With(ShitroidShades.Membrane(phase))
        : ShitroidShades.InTargetColours;

    /// <summary>
    /// Whether Samus's touch, shots and power bombs reach the Shitroid's private reactions (parameter two): set while
    /// it hovers near Samus or follows her after a release, cleared while it is dormant or latched on.
    /// </summary>
    public bool ReactsToSamus { get; internal set; }
}
