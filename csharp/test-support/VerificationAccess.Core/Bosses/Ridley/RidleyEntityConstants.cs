using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Cartridge values and helpers of <see cref="RidleyEntity"/> that only verification reads.</summary>
internal static class RidleyEntityConstants
{
    /// <summary>The encounter <paramref name="body"/> holds, from its initialization.</summary>
    /// <param name="body">A place holding one of Ridley's bodies.</param>
    /// <exception cref="InvalidOperationException">The place holds no Ridley body that has begun an encounter.</exception>
    internal static RidleyEncounterState EncounterOf(RoomEnemyPlace body) =>
        Ensure.NotNull(body).Entity is RidleyEntity entity
            ? entity.Encounter
            : throw new InvalidOperationException($"Enemy place {body.Ordinal} holds no Ridley body.");
}
