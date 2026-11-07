namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A pre-instruction routine: the port that runs it.</summary>
/// <param name="name">What the routine does, for diagnostics.</param>
/// <param name="run">The port.</param>
internal sealed class RoomObjectFrameBehavior(string name, Action<RoomObjectSlot, RoomObjectStepContext> run) : IRoomObjectFrameBehavior
{
    /// <inheritdoc/>
    public void Run(RoomObjectSlot slot, RoomObjectStepContext context) => run(slot, context);

    /// <summary>A routine that returns without changing anything.</summary>
    /// <param name="name">What the routine is, for diagnostics.</param>
    internal static RoomObjectFrameBehavior NoOp(string name) => new(name, static (_, _) => { });

    /// <inheritdoc/>
    public override string ToString() => name;
}
