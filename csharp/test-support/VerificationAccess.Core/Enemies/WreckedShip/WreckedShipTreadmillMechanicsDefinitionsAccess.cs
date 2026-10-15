
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Verification access to <see cref="WreckedShipTreadmillMechanicsDefinitions"/> members production does not use.</summary>
internal static class WreckedShipTreadmillMechanicsDefinitionsAccess
{
    /// <summary>Enumerates rightward then leftward treadmill object definitions in native header order.</summary>
    extension(WreckedShipTreadmillMechanicsDefinitions)
    {
        /// <summary>Both named directions in native header order, without a stored roster.</summary>
        internal static IEnumerable<WreckedShipTreadmillObjectDefinition> All
        {
            get
            {
                yield return WreckedShipTreadmillMechanicsDefinitions.ForDirection(WreckedShipTreadmillDirection.Rightwards);
                yield return WreckedShipTreadmillMechanicsDefinitions.ForDirection(WreckedShipTreadmillDirection.Leftwards);
            }
        }
    }
}
