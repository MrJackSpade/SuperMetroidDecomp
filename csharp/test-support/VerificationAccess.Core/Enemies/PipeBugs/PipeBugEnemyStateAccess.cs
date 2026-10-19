using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Verification access to <see cref="PipeBugEnemyState"/> members production does not use.</summary>
internal static class PipeBugEnemyStateAccess
{
    /// <summary>Exposes <see cref="PipeBugEnemyState"/> state that only verification reads.</summary>
    extension(PipeBugEnemyState self)
    {
        /// <summary>Gets whether the initialized owner is either Brinstar Pipe Bug variant.</summary>
        public bool IsBrinstar =>
            self.DefinitionAtInitialization is
                EnemyDefinitionId.Zeb or
                EnemyDefinitionId.Zebbo;
    }
}
