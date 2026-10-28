using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>How a retail scroll program writes its scroll states.</summary>
internal static class RoomPlmScrollPrograms
{
    /// <summary>Applies <paramref name="program"/>'s ordered scroll writes.</summary>
    internal static void Apply(RoomScrollProgram program, Action<int, RoomScrollState> write)
    {
        Ensure.NotNull(write);
        foreach (RoomScrollWrite step in RoomPlmScrollProgramDefinitions.Programs[program])
            write(step.Index, step.State);
    }
}
