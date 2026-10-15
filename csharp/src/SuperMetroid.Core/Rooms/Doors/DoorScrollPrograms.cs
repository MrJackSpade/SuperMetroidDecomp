using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Runs the translated straight-line bank-$8F door callbacks that write the room scroll grid.</summary>
internal static class DoorScrollPrograms
{
    /// <summary>True when the setup code selects a translated pure scroll callback.</summary>
    public static bool Contains(DoorSetupCode pointer) => DoorScrollProgramDefinitions.Writes(pointer) is not null;

    /// <summary>Executes ordered scroll writes; setup routines without scroll writes leave the grid unchanged.</summary>
    public static bool TryApply(DoorSetupCode pointer, RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        if (DoorScrollProgramDefinitions.Writes(pointer) is not { } writes)
            return false;
        foreach (DoorScrollWrite write in writes)
            scrolls.SetStorage(write.ScrollIndex, write.State);
        return true;
    }
}