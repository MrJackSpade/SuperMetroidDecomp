using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="ChozoStatuePlmDefinitions"/> members production does not use.</summary>
internal static class ChozoStatuePlmDefinitionsAccess
{
    /// <summary>Lists the authored Chozo statue PLM definitions used by room population.</summary>
    extension(ChozoStatuePlmDefinitions)
    {
        /// <summary>Enumerate the five semantic spawn cases in the original published order.</summary>
        internal static IEnumerable<ChozoStatuePlmDefinition> All
        {
            get
            {
                yield return ChozoStatuePlmDefinitions.LowerNorfairHand;
                yield return ChozoStatuePlmDefinitions.WreckedShipHand;
                yield return ChozoStatuePlmDefinitions.ClearSlopeAccess;
                yield return ChozoStatuePlmDefinitions.BlockSlopeAccess;
                yield return ChozoStatuePlmDefinitions.CrumblePlug;
            }
        }
    }
}
