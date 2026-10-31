namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The n00b tube's wakes and its inactive return.</summary>
internal static class NoobTubeFrameBehaviors
{
    /// <summary>The power-bomb wake frame behaviour on the tube: a power bomb wakes it; another shot plays the ineffective sound.</summary>
    internal static PlmFrameBehavior WakeOnPowerBomb { get; } = Wake(NoobTubePlmFrameBehavior.WakeOnPowerBomb);

    /// <summary>An accepted controller edge wakes the tube.</summary>
    internal static PlmFrameBehavior WakeOnAcceptedInput { get; } = Wake(NoobTubePlmFrameBehavior.WakeOnAcceptedInput);

    /// <summary>The return the shattered tube installs: it does nothing.</summary>
    internal static PlmFrameBehavior Inactive { get; } = PlmFrameBehavior.NoOp("n00b tube inactive");

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static PlmFrameBehavior Wake(NoobTubePlmFrameBehavior routine) =>
        new($"n00b tube {routine}",
            (slot, context) => context.Plms.NoobTube.RunNoobTubeFrameBehavior(slot, routine, context.Frame.ControllerNewInput));
}
