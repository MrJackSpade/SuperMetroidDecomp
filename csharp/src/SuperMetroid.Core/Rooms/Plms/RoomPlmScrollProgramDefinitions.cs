using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The retail scroll programs: ordered room-scroll writes by program.</summary>
internal static partial class RoomPlmScrollProgramDefinitions
{
    /// <summary>Applies <paramref name="program"/>'s ordered scroll writes.</summary>
    internal static void Apply(RoomScrollProgram program, Action<int, RoomScrollState> write)
    {
        Ensure.NotNull(write);
        foreach (RoomScrollWrite step in Programs[program])
            write(step.Index, step.State);
    }
}
