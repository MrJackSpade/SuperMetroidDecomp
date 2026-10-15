using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Typed view of Cacatac's six native ordinary words plus its two extended patrol bounds.
/// The native record uniquely uses <c>$0FA8,x</c> for right subspeed; because the generic
/// slot projection begins at <c>$0FAA,x</c>, that one word remains explicit host storage.
/// </summary>
public sealed class CacatacEnemyState
{
    /// <summary>Common enemy slot backing Cacatac's native patrol velocity and function words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects a live Cacatac slot into typed patrol and spike-facing state.</summary>
    internal CacatacEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<CacatacProgram> Program { get; set; }

    /// <summary>Native <c>$0FA8,x</c>, the fractional half of rightward velocity.</summary>
    public ushort RightSubvelocity { get; internal set; }

    /// <summary>Native <c>$0FAA,x</c>, the whole half of rightward velocity.</summary>
    public ushort RightVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native <c>$0FAC,x</c>, the fractional half of leftward velocity.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native <c>$0FAE,x</c>, the whole half of leftward velocity.</summary>
    public ushort LeftVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native <c>$0FB0,x</c>; zero is left, one is right, two initially stops.</summary>
    public CacatacDirection Direction
    {
        get => (CacatacDirection)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Native indirect function pointer at <c>$0FB2,x</c>.</summary>
    public CacatacEnemyFunction Function
    {
        get => (CacatacEnemyFunction)_slot.VariableE;
        internal set => _slot.VariableE = (ushort)value;
    }

    /// <summary>Left patrol limit in whole room pixels, stored at native <c>$7E:7800,x</c>.</summary>
    public ushort MinimumXPosition { get; internal set; }

    /// <summary>Right patrol limit in whole room pixels, stored at native <c>$7E:7802,x</c>.</summary>
    public ushort MaximumXPosition { get; internal set; }

    /// <summary>
    /// Parameter-one's high-byte presentation selector. The assembly tests only zero versus
    /// nonzero, so this is a Boolean discriminator rather than a guessed flags enum.
    /// </summary>
    public bool UpsideUp { get; internal set; }
}
