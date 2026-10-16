namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The n00b tube's wakes and its inactive return.</summary>
internal static class NoobTubePreInstructions
{
    /// <summary><c>$84:BD26</c> on the tube: a power bomb wakes it; another shot plays the ineffective sound.</summary>
    internal static PlmPreInstruction WakeOnPowerBomb { get; } = Wake(NoobTubePlmPreInstruction.WakeOnPowerBomb);

    /// <summary><c>$84:D4BF</c>: an accepted controller edge wakes the tube.</summary>
    internal static PlmPreInstruction WakeOnAcceptedInput { get; } = Wake(NoobTubePlmPreInstruction.WakeOnAcceptedInput);

    /// <summary><c>$84:86D0</c>, the return the shattered tube installs: it does nothing.</summary>
    internal static PlmPreInstruction Inactive { get; } = PlmPreInstruction.NoOp("n00b tube inactive", (ushort)NoobTubePlmPreInstruction.Inactive);

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static PlmPreInstruction Wake(NoobTubePlmPreInstruction routine) =>
        new($"n00b tube {routine}", (ushort)routine,
            (slot, context) => context.Plms.NoobTube.RunNoobTubePreInstruction(slot, routine, context.Frame.ControllerNewInput));
}
