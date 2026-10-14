
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Verification access to <see cref="SamusKinematicsState"/> members production does not use.</summary>
internal static class SamusKinematicsStateAccess
{
    /// <summary>Reads the four native solid-enemy collision indexes, including $FFFF for a clear direction.</summary>
    extension(SamusKinematicsState self)
    {
        /// <summary>
        /// Native left/right/up/down collision-index words retained for debugger inspection.
        /// <c>$FFFF</c> means that direction did not encounter a solid enemy.
        /// </summary>
        internal IReadOnlyList<ushort> SolidEnemyCollisionIndexes => self._solidEnemyCollisionIndexes;
    }
}
