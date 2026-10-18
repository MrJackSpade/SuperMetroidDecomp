namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A pre-instruction routine: the port that runs it.</summary>
/// <param name="name">What the routine does, for diagnostics.</param>
/// <param name="run">The port.</param>
internal sealed class PlmPreInstruction(string name, Action<PlmSlot, PlmStepContext> run) : IPlmPreInstruction
{
    /// <inheritdoc/>
    public void Run(PlmSlot slot, PlmStepContext context) => run(slot, context);

    /// <summary>A routine that returns without changing anything.</summary>
    /// <param name="name">What the routine is, for diagnostics.</param>
    internal static PlmPreInstruction NoOp(string name) => new(name, static (_, _) => { });

    /// <inheritdoc/>
    public override string ToString() => name;
}
