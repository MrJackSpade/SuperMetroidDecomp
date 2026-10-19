namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access to the species state of a slot's current occupant.</summary>
internal static class RoomEnemySlotEntityStateAccess
{
    /// <summary>Reads occupant state through the slot that holds it.</summary>
    extension(RoomEnemySlot self)
    {
        /// <summary>The slot's <typeparamref name="TState"/> while a <typeparamref name="TEntity"/> occupies it, else null.</summary>
        internal TState? EntityState<TEntity, TState>()
            where TEntity : class, IEnemyEntity
            where TState : class =>
            self.Entity is TEntity ? self.SpeciesState<TState>() : null;
    }
}
