using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Runs the straight-line door behaviours that write the room scroll grid.</summary>
internal static class DoorScrollChanges
{
    /// <summary>True when the setup code selects a pure scroll behaviour.</summary>
    public static bool Contains(DoorSetupCode setupCode) => Writes(setupCode) is not null;

    /// <summary>Executes ordered scroll writes; setups without scroll writes leave the grid unchanged.</summary>
    public static bool TryApply(DoorSetupCode setupCode, RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        if (Writes(setupCode) is not { } writes)
            return false;
        foreach (DoorScrollWrite write in writes)
            scrolls.SetCell(write.ScrollIndex, write.State);
        return true;
    }

    /// <summary>
    /// The ordered scroll writes of <paramref name="code"/>, or <see langword="null"/> for a setup that
    /// writes no scroll cells. An undefined setup code throws.
    /// </summary>
    private static IReadOnlyList<DoorScrollWrite>? Writes(DoorSetupCode code)
    {
        if (DoorScrollChangeDefinitions.Writes.TryGetValue(code, out IReadOnlyList<DoorScrollWrite>? writes))
            return writes;
        if (DoorScrollChangeDefinitions.WithoutScrollWrites.Contains(code))
            return null;
        throw new InvalidOperationException($"Undefined {nameof(DoorSetupCode)} {code}.");
    }
}
