namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The n00b tube's wakes and its inactive return.</summary>
internal static class NoobTubeFrameBehaviors
{
    /// <summary>The power-bomb wake frame behaviour on the tube: a power bomb wakes it; another shot plays the ineffective sound.</summary>
    internal static RoomObjectFrameBehavior WakeOnPowerBomb { get; } = Wake(NoobTubeObjectFrameBehavior.WakeOnPowerBomb);

    /// <summary>An accepted controller edge wakes the tube.</summary>
    internal static RoomObjectFrameBehavior WakeOnAcceptedInput { get; } = Wake(NoobTubeObjectFrameBehavior.WakeOnAcceptedInput);

    /// <summary>The return the shattered tube installs: it does nothing.</summary>
    internal static RoomObjectFrameBehavior Inactive { get; } = RoomObjectFrameBehavior.NoOp("n00b tube inactive");

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static RoomObjectFrameBehavior Wake(NoobTubeObjectFrameBehavior routine) =>
        new($"n00b tube {routine}",
            (slot, context) => context.RoomObjects.NoobTube.RunNoobTubeFrameBehavior(slot, routine, context.Frame.ControllerNewInput));
}
