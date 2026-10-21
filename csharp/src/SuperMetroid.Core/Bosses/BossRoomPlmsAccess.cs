using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses;

/// <summary>Reaches the boss PLM families from a step context's room PLM owner.</summary>
internal static class BossRoomPlmsAccess
{
    /// <summary>The boss PLM families <paramref name="plms"/> runs; every room's PLM owner runs them.</summary>
    /// <exception cref="InvalidOperationException">The owner runs no boss PLM families.</exception>
    internal static IBossRoomPlms Bosses(this IRoomPlms plms) =>
        plms as IBossRoomPlms ?? throw new InvalidOperationException($"{plms.GetType().Name} runs no boss PLM families.");
}
