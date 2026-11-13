namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The step count every gradual fade in the game shares (#1544): a door's fade out and in, Samus's death fade, the escape's
/// time-up whiteout, Kraid's backdrop fades and the Torizos' body fades each read it as their current step, advance it
/// after each step and reset it when they complete. None resets it when it starts, except Kraid, whose room-background
/// redraw clears it before his fades.
/// </summary>
/// <remarks>
/// <para>
/// Gameplay behaviour reproduced: a fade starts at whatever step the last fade left, so a fade that stops part-way
/// shortens the next. A Torizo's wake-up fade takes sixteen steps on a fade of twelve: it completes on its fifteenth and
/// starts again on its sixteenth, leaving the count at one. The Torizo's own death fade starts there and so completes in
/// its fourteen steps; Samus's death fade during the fight starts there too and completes one update sooner, as does a
/// death fade after a Kraid backdrop fade her death cut short. When the death fade completes decides when the game-over
/// screen appears and takes input; when a door's fade completes decides when the door transition moves on. The room's
/// enemies run between a door fade's steps, and the Zebetites' pulse and the Shitroid's membrane pulse hold while the
/// count is past zero, so how far either has pulsed when Samus enters or leaves their room follows the door's fade.
/// </para>
/// <para>
/// Consulted and advanced once per update by the running fade's owner: the door transition's fade phases, the death and
/// time-up phases, Kraid's backdrop fade phases and the Torizos' fade cues; reset by the fade that completes and
/// by Kraid's background redraw; read, never written, by the Zebetites' and the Shitroid's pulses. Created with each gameplay runtime, which hands it to its door transition, its death and
/// time-up fades and every room's enemies, so the count outlives every room. Evidence:
/// the original gradual colour change routines, which all read, advance and clear one shared step count and none clears it
/// at its start; the Torizo animations' sixteen-step wake-up and fourteen-step death fade, which only complete in fourteen
/// steps from the count the wake-up leaves; the Zebetites' and the Shitroid's palette routines, which skip their
/// colour step while the count is nonzero, and the door fade routines, which run the room's enemies between steps.
/// </para>
/// </remarks>
[CompatibilityService("every gradual fade counts its steps on one shared count, starting where the last fade left it, so a fade left part-way shortens the next: the Torizos' death fades and Samus's death fade among them")]
public sealed class SharedFadeStepCount
{
    /// <summary>
    /// The current step: zero once a fade completes, then one more per step taken. Every fade resets it once it passes the
    /// fade's length plus one, so it never exceeds the longest fade's length plus two.
    /// </summary>
    public int Step { get; private set; }

    /// <summary>
    /// Whether the count is past a fade's start: a fade is under way, or one stopped part-way has left the count where it
    /// stopped. The Zebetites' and the Shitroid's colour pulses hold while it is.
    /// </summary>
    public bool IsPastStart => Step != 0;

    /// <summary>Moves to the next step after a step is taken.</summary>
    public void Advance() => Step++;

    /// <summary>Restarts the count, as a fade that completes and Kraid's background redraw do.</summary>
    public void Reset() => Step = 0;
}
