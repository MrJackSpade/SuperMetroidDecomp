using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The state of the Shitroid, with its extended enemy WRAM. The values the cartridge keeps in the enemy's common record
/// are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class ShitroidEnemyState
{
    /// <summary>The colours each of the encounter's target rows fades to, staged across its colour transitions (#1405).</summary>
    private readonly Dictionary<ShitroidColorTarget, RgbColor[]> targetColours = new();

    /// <summary>Creates the state for the Shitroid's slot.</summary>
    internal ShitroidEnemyState(RoomEnemySlot slot) => Slot = slot;

    /// <summary>The Shitroid's enemy slot.</summary>
    public RoomEnemySlot Slot { get; }

    /// <summary>Where the Shitroid's program resumes (#641).</summary>
    internal AnimationProgramCursor<ShitroidProgram> Cursor { get; set; }

    /// <summary>
    /// Gets the current bank-$A9 Shitroid AI function pointer; none once a kill has emptied the record.
    /// </summary>
    public ShitroidAiFunction? Function { get; internal set; }

    /// <summary>Signed 8.8 velocity: X, Y.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>Membrane palette-cycle phase, 0 through 7, held in a byte.</summary>
    public int PalettePhase
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>
    /// Updates left before the membrane palette advances to its next phase; each update decrements it until zero, and
    /// the advance reloads it from <see cref="PaletteDelay"/>. It is held in a byte.
    /// </summary>
    public int PaletteTimer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>Updates between palette-cycle phases: ten at the ordinary pace, one while draining quickly.</summary>
    public int PaletteDelay
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Gets the current AI state's general-purpose countdown.</summary>
    public WrappingCountdown StateTimer { get; internal set; }

    /// <summary>Native extended word 01: randomized vertical-follow duration.</summary>
    public WrappingCountdown HoverTimer { get; internal set; }

    /// <summary>
    /// Horizontal-lock accumulator (native extended word 02): gains two each update the Shitroid is horizontally
    /// aligned with its target and loses one, never below zero, otherwise.
    /// </summary>
    public int HorizontalLockAccumulator { get; internal set; }

    /// <summary>Whether the periodic Shitroid cry is enabled (native extended word 04).</summary>
    public bool CryEnabled { get; internal set; }

    /// <summary>Palette cycles toward the next periodic cry, 0 through 3 (native extended word 05).</summary>
    public int CryCounter { get; internal set; }

    /// <summary>
    /// Diagnostic mirror of native word 08 in physical slot one. Shitroid writes the actual dead-sidehopper palette
    /// stage through <c>ActivateDeadSidehopperVictim</c>; retaining this value on the boss state makes that cross-slot
    /// side effect directly auditable.
    /// </summary>
    public bool VictimActivationFlag { get; internal set; }
    /// <summary>The colours <paramref name="target"/>'s row fades to, which encounter colour updates change in place.</summary>
    internal Span<RgbColor> TargetColours(ShitroidColorTarget target) =>
        targetColours.TryGetValue(target, out RgbColor[]? colours)
            ? colours
            : targetColours[target] = new RgbColor[ShitroidColorDefinitions.TargetColorCount];

    /// <summary>
    /// Whether Samus's touch, shots and power bombs reach the Shitroid's private reactions (parameter two): set while
    /// it hovers near Samus or follows her after a release, cleared while it is dormant or latched on.
    /// </summary>
    public bool ReactsToSamus { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        ReactsToSamus = false;
        Function = null;
        Velocity = default;
        PalettePhase = 0;
        PaletteTimer = 0;
        PaletteDelay = 0;
        StateTimer = WrappingCountdown.Start(0);
    }
}
