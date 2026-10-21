using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas;

/// <summary>Reaches the area PLM families from a step context's room PLM owner.</summary>
internal static class AreaRoomPlmsAccess
{
    /// <summary>The area PLM families <paramref name="plms"/> runs; every room's PLM owner runs them.</summary>
    /// <exception cref="InvalidOperationException">The owner runs no area PLM families.</exception>
    internal static IAreaRoomPlms Areas(this IRoomPlms plms) =>
        plms as IAreaRoomPlms ?? throw new InvalidOperationException($"{plms.GetType().Name} runs no area PLM families.");
}
