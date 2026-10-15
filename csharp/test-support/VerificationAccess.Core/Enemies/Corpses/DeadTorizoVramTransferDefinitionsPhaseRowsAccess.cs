
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Verification access to <see cref="DeadTorizoVramTransferDefinitions.PhaseRows"/> members production does not use.</summary>
internal static class DeadTorizoVramTransferDefinitionsPhaseRowsAccess
{
    /// <summary>Exposes the count of VRAM transfer rows for a Dead Torizo phase.</summary>
    extension(DeadTorizoVramTransferDefinitions.PhaseRows self)
    {
        /// <summary>Number of VRAM transfers scheduled in this Dead Torizo phase.</summary>
        internal int Length => self.Count;
    }
}
