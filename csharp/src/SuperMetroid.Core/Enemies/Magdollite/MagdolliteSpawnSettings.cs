namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Which part of a Magdollite a placement is (#1472, #1519, #2023): its head, its rising body or its tracking overlay,
/// each its own settings (#2474), so only the head's carry how it rises.
/// </summary>
internal abstract partial record MagdolliteSpawnSettings : EnemySpawnSettings
{
    /// <summary>The occupant of <paramref name="place"/> the part these settings place makes, for <paramref name="species"/>.</summary>
    /// <param name="species">The species the part belongs to.</param>
    /// <param name="occupant">The occupant the part is created for, which carries its identity.</param>
    /// <param name="place">The place the part occupies.</param>
    /// <param name="common">The participation every enemy shares.</param>
    internal abstract EnemyOccupant Create(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common);
}
