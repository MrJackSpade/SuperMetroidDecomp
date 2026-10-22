namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access: a loaded graphics set's records, as checks read them.</summary>
internal static class EnemyGraphicsSetAccess
{
    extension(EnemyGraphicsSet self)
    {
        /// <summary>The loaded set's record for <paramref name="enemy"/>.</summary>
        /// <exception cref="InvalidDataException">The room's graphics set does not load that enemy.</exception>
        internal RoomEnemyGraphicsSetEntry Entry(EnemyDefinitionId enemy) =>
            self.Entries.Where(entry => entry.DefinitionId == enemy).Cast<RoomEnemyGraphicsSetEntry?>().FirstOrDefault()
                ?? throw new InvalidDataException($"Enemy graphics set {self.LoadedSet.Name} does not load {enemy}.");
    }
}
