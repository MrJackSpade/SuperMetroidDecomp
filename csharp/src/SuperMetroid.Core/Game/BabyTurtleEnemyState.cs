namespace SuperMetroid.Core.Game;

/// <summary>Typed view of a Baby Turtle's ordinary variables and required extension words.</summary>
public sealed class BabyTurtleEnemyState
{
    /// <summary>Physical child slot whose common words hold parent identity and shell-path state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches one Baby Turtle's typed view to its native child record.</summary>
    internal BabyTurtleEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Gets the current bank-$A2 Baby Turtle AI function pointer.</summary>
    public BabyTurtleAiFunction Function
    {
        get => (BabyTurtleAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Gets the parent Mama Turtle's native enemy-record byte index.</summary>
    public ushort ParentNativeIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Gets the horizontal spawn position used by the child's shell path.</summary>
    public ushort SpawnXPosition
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Gets the upper boundary of the child's spawn hitbox.</summary>
    public ushort SpawnTopBoundary
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
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
