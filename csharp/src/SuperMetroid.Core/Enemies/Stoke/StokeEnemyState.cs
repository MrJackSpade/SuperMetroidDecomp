using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Typed view over Stoke's six generic enemy-variable words. These properties remain backed
/// by <see cref="RoomEnemySlot"/> rather than duplicating state in host-only fields: inspecting
/// variables A-F consequently shows the same values as <c>$0FA8-$0FB2</c> in the original.
/// </summary>
public sealed class StokeEnemyState
{
    /// <summary>The live enemy slot whose generic variables hold Stoke's speed and AI state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds Stoke's typed state view to its existing room enemy slot.</summary>
    internal StokeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<StokeProgram> Program { get; set; }

    /// <summary>Fractional half of the positive 16.16 walking displacement.</summary>
    public ushort RightSubvelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Signed whole half of the positive 16.16 walking displacement.</summary>
    public ushort RightVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Fractional half of the negative 16.16 walking displacement.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Signed whole half of the negative 16.16 walking displacement.</summary>
    public ushort LeftVelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Facing latch changed by animation commands, one frame after a turn request.</summary>
    public StokeDirection Direction
    {
        get => (StokeDirection)_slot.VariableE;
        internal set => _slot.VariableE = (ushort)value;
    }

    /// <summary>Current indirect main-AI routine.</summary>
    public StokeAiFunction Function
    {
        get => (StokeAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
