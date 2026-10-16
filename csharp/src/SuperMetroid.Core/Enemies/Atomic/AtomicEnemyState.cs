using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Debugger-visible view of Atomic's two common function words and four bank-$7E speed words.
/// Each speed is kept as the cartridge's separate whole/fraction pair. In particular, the
/// negative pair comes from the ROM table and is not recomputed with host signed arithmetic.
/// </summary>
public sealed class AtomicEnemyState
{
    /// <summary>Enemy slot whose common variables hold the movement dispatch words.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>The slot's Atomic extension words, kept for the room across occupants.</summary>
    private readonly AtomicExtensionWords _words;

    /// <summary>Binds the debugger view to Atomic's physical enemy slot and shared speed arrays.</summary>
    internal AtomicEnemyState(
        RoomEnemySlot slot, AtomicExtensionWords words)
    {
        _slot = slot;
        _words = words;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<AtomicProgram> Program { get; set; }

    /// <summary>
    /// Native <c>Atomic.XMovementFunction</c> at enemy variable A. Despite that original
    /// name, the selected routine changes the actor's Y coordinate.
    /// </summary>
    public AtomicVerticalMovement VerticalMovement
    {
        get => (AtomicVerticalMovement)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Native <c>Atomic.YMovementFunction</c> at enemy variable B. Despite that original
    /// name, the selected routine changes the actor's X coordinate.
    /// </summary>
    public AtomicHorizontalMovement HorizontalMovement
    {
        get => (AtomicHorizontalMovement)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Native $7E:7800 subSpeed word for this slot: positive velocity's low sixteen bits, in 1/65536-pixel units per AI update, added to either coordinate's subposition after its whole-word update.</summary>
    public ushort SpeedFraction
    {
        get => _words.SpeedFraction;
        internal set => _words.SpeedFraction = value;
    }

    /// <summary>Native $7E:7802 speed word for this slot: positive velocity's signed whole-pixel component, loaded from the shared linear-speed record selected by parameter two and used for down/right movement.</summary>
    public ushort SpeedWhole
    {
        get => _words.SpeedWhole;
        internal set => _words.SpeedWhole = value;
    }

    /// <summary>Native $7E:7804 negativeSubSpeed word for this slot: the table-supplied low sixteen bits of the negative velocity, added with carry for up/left movement rather than negating the positive fraction alone.</summary>
    public ushort NegativeSpeedFraction
    {
        get => _words.NegativeSpeedFraction;
        internal set => _words.NegativeSpeedFraction = value;
    }

    /// <summary>Native $7E:7806 negativeSpeed word for this slot: the table-supplied signed whole-pixel component for up/left movement, exposed as its raw ushort bits and added before fractional carry.</summary>
    public ushort NegativeSpeedWhole
    {
        get => _words.NegativeSpeedWhole;
        internal set => _words.NegativeSpeedWhole = value;
    }
}
