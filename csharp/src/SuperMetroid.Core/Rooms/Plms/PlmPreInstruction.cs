namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A pre-instruction routine: its native address and the port that runs it.</summary>
/// <param name="name">What the routine does, for diagnostics.</param>
/// <param name="nativeRoutine">The bank-$84 routine address.</param>
/// <param name="run">The port.</param>
internal sealed class PlmPreInstruction(string name, ushort nativeRoutine, Action<PlmSlot, PlmStepContext> run) : IPlmPreInstruction
{
    /// <inheritdoc/>
    public ushort NativeRoutine { get; } = nativeRoutine;

    /// <inheritdoc/>
    public void Run(PlmSlot slot, PlmStepContext context) => run(slot, context);

    /// <summary>A routine that returns without changing anything.</summary>
    /// <param name="name">What the routine is, for diagnostics.</param>
    /// <param name="nativeRoutine">The bank-$84 routine address.</param>
    internal static PlmPreInstruction NoOp(string name, ushort nativeRoutine) => new(name, nativeRoutine, static (_, _) => { });

    /// <inheritdoc/>
    public override string ToString() => $"{name} ($84:{NativeRoutine:X4})";
}
