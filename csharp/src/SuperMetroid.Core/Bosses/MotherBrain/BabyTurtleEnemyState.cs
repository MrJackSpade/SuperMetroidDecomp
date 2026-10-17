using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Typed view of a Baby Turtle's ordinary variables and required extension words.</summary>
public sealed class BabyTurtleEnemyState
{
    /// <summary>Physical child slot whose common words hold parent identity and shell-path state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches one Baby Turtle's typed view to its native child record.</summary>
    internal BabyTurtleEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MamaTurtleProgram> Program { get; set; }

    /// <summary>Gets the current bank-$A2 Baby Turtle AI function pointer.</summary>
    public BabyTurtleAiFunction Function
    {
        get => (BabyTurtleAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// The parent Mama Turtle's physical slot index. Variable B keeps the slot's record offset so code that reads
    /// the slot by offset still sees it; this view converts in both directions and rejects a misaligned offset.
    /// </summary>
    public int ParentSlotIndex
    {
        get => _slot.VariableB % RoomEnemySystem.NativeSlotSize == 0
            ? _slot.VariableB / RoomEnemySystem.NativeSlotSize
            : throw new InvalidDataException($"Baby Turtle parent offset {_slot.VariableB} is not a slot boundary.");
        internal set => _slot.VariableB = checked((ushort)(value * RoomEnemySystem.NativeSlotSize));
    }

    /// <summary>
    /// The child's spawn point: its spawn X, which the crawl turns around 48 pixels from, and the top of its spawn
    /// hitbox, which the crawl holds it to. Variables C and D keep the two coordinates.
    /// </summary>
    public Position16<RoomPixels> SpawnTopCenter
    {
        get => new(_slot.VariableC, _slot.VariableD);
        internal set
        {
            _slot.VariableC = value.X;
            _slot.VariableD = value.Y;
        }
    }

    /// <summary>
    /// Signed whole-pixel velocity per update. The horizontal word is ordinary variable E; the
    /// vertical word is an extension word.
    /// </summary>
    public Offset16<RoomPixels> Velocity
    {
        get => new(unchecked((short)_slot.VariableE), _verticalVelocity);
        internal set
        {
            _slot.VariableE = unchecked((ushort)value.X);
            _verticalVelocity = value.Y;
        }
    }

    /// <summary>Extra-WRAM signed vertical velocity paired with common-slot horizontal speed.</summary>
    private short _verticalVelocity;

    /// <summary>Gets the phase-specific Baby Turtle countdown.</summary>
    public NativeCountdown FunctionTimer { get; internal set; }
    /// <summary>Gets the reaction countdown used while the child is not carrying Samus.</summary>
    public NativeCountdown NotCarryingSamusReactionTimer { get; internal set; }
}
