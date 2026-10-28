using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Runs the translated straight-line bank-$8F door callbacks that write the room scroll grid.</summary>
internal static class DoorScrollPrograms
{
    /// <summary>True when the setup code selects a translated pure scroll callback.</summary>
    public static bool Contains(DoorSetupCode setupCode) => Writes(setupCode) is not null;

    /// <summary>Executes ordered scroll writes; setup routines without scroll writes leave the grid unchanged.</summary>
    public static bool TryApply(DoorSetupCode setupCode, RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        if (Writes(setupCode) is not { } writes)
            return false;
        foreach (DoorScrollWrite write in writes)
            scrolls.SetStorage(write.ScrollIndex, write.State);
        return true;
    }

    /// <summary>
    /// The ordered scroll writes of <paramref name="code"/>, or <see langword="null"/> for a setup routine that
    /// writes no scroll cells. An undefined setup code throws.
    /// </summary>
    private static IReadOnlyList<DoorScrollWrite>? Writes(DoorSetupCode code)
    {
        if (DoorScrollProgramDefinitions.Writes.TryGetValue(code, out IReadOnlyList<DoorScrollWrite>? writes))
            return writes;
        if (DoorScrollProgramDefinitions.WithoutScrollWrites.Contains(code))
            return null;
        throw new InvalidOperationException($"Undefined {nameof(DoorSetupCode)} {code}.");
    }
}
