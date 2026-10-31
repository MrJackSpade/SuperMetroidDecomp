namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The n00b tube's wakes and its inactive return.</summary>
internal static class NoobTubeFrameBehaviors
{
    /// <summary><c>$84:BD26</c> on the tube: a power bomb wakes it; another shot plays the ineffective sound.</summary>
    internal static PlmFrameBehavior WakeOnPowerBomb { get; } = Wake(NoobTubePlmFrameBehavior.WakeOnPowerBomb);

    /// <summary><c>$84:D4BF</c>: an accepted controller edge wakes the tube.</summary>
    internal static PlmFrameBehavior WakeOnAcceptedInput { get; } = Wake(NoobTubePlmFrameBehavior.WakeOnAcceptedInput);

    /// <summary><c>$84:86D0</c>, the return the shattered tube installs: it does nothing.</summary>
    internal static PlmFrameBehavior Inactive { get; } = PlmFrameBehavior.NoOp("n00b tube inactive");

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static PlmFrameBehavior Wake(NoobTubePlmFrameBehavior routine) =>
        new($"n00b tube {routine}",
            (slot, context) => context.Plms.NoobTube.RunNoobTubeFrameBehavior(slot, routine, context.Frame.ControllerNewInput));
}
