using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>Verification placing of an enemy as a room load places it.</summary>
internal static class RoomEnemyPlaceInitializationAccess
{
    /// <summary>Places the slot's enemy through its production initialization.</summary>
    extension(RoomEnemyPlace self)
    {
        /// <summary>
        /// Binds the slot's occupant again from the spawn settings recorded for it, as a room load records them before it
        /// binds the entity (#2342: a species whose placement names its part creates that part's entity), then runs the
        /// entity's initialization with <paramref name="samus"/> as the load hands her.
        /// </summary>
        internal void InitializeAsPlaced(SamusState samus)
        {
            self.Bind(self.DefinitionId, self.Definition);
            EnemyLifecycle.RunInitializationAi(self, new EnemySpawnSurroundings(null, samus, GameplayAction.None, default));
        }
    }
}
