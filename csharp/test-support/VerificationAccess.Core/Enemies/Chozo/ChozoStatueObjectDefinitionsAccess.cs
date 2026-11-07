namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Verification access to <see cref="ChozoStatueObjectDefinitions"/> members production does not use.</summary>
internal static class ChozoStatueObjectDefinitionsAccess
{
    /// <summary>Lists the authored Chozo statue PLM definitions used by room population.</summary>
    extension(ChozoStatueObjectDefinitions)
    {
        /// <summary>Enumerate the five semantic spawn cases in the original published order.</summary>
        internal static IEnumerable<ChozoStatueObjectDefinition> All
        {
            get
            {
                yield return ChozoStatueObjectDefinitions.LowerNorfairHand;
                yield return ChozoStatueObjectDefinitions.WreckedShipHand;
                yield return ChozoStatueObjectDefinitions.ClearSlopeAccess;
                yield return ChozoStatueObjectDefinitions.BlockSlopeAccess;
                yield return ChozoStatueObjectDefinitions.CrumblePlug;
            }
        }
    }
}
