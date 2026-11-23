using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Verification access to <see cref="SamusKinematicsState"/> members production does not use.</summary>
internal static class SamusKinematicsStateAccess2
{
    /// <summary>Exposes <see cref="SamusKinematicsState"/> state that only verification reads.</summary>
    extension(SamusKinematicsState self)
    {
        /// <summary>
        /// The solid enemy Samus met moving in <paramref name="direction"/>, or null where she met none since the enemy pass
        /// last cleared them.
        /// </summary>
        public ISolidEnemy? SolidEnemyContact(SamusCollisionDirection direction) =>
            self.SolidEnemyContactsByDirection
                .GetValueOrDefault(direction);
    }
}
