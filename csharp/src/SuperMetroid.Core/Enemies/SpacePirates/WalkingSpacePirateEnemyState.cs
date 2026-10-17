using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Debugger-facing projection of one walking Space Pirate's native state. The patrol posts are room positions that
/// wrap at sixteen bits, because population parameter two is added and subtracted without clamping by
/// <c>$B2:FD02</c>; the walk compares the Pirate with them by the wrapped spatial difference.
/// </summary>
public sealed class WalkingSpacePirateEnemyState
{
    /// <summary>Native enemy slot backing the walking Pirate's function and patrol posts.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds the typed walking-Pirate view to its existing enemy slot.</summary>
    internal WalkingSpacePirateEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WalkingSpacePirateProgram> Program { get; set; }

    /// <summary>Bank-$B2 function address stored in native enemy variable A.</summary>
    public WalkingSpacePirateFunction Function
    {
        get => (WalkingSpacePirateFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Left patrol post in native variable E. Only the X axis is stored and meaningful: a leftward walk turns back once
    /// the Pirate passes it.
    /// </summary>
    public Position16<RoomPixels> LeftPost
    {
        get => new(_slot.VariableE, 0);
        internal set => _slot.VariableE = value.X;
    }

    /// <summary>
    /// Right patrol post in native variable F. Only the X axis is stored and meaningful: a rightward walk turns back
    /// once the Pirate reaches it.
    /// </summary>
    public Position16<RoomPixels> RightPost
    {
        get => new(_slot.VariableF, 0);
        internal set => _slot.VariableF = value.X;
    }

    /// <summary>Successful allocations from the actor's three-shot firing animations.</summary>
    public int SpawnedLaserCount { get; internal set; }
}
