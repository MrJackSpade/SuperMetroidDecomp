namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Exact RGB5 shade relationships in the final Sidehopper drain image at $A9:EC6C.</summary>
internal static class SidehopperDrainShadeDefinitions
{
    /// <summary>$A9:EC70/EC78: both upper intermediate reds are seven below their own light endpoint (31→24 and27→20). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    internal const int SelectedLightRedDrop = 7;
    /// <summary>$A9:EC72/EC7A: both lower intermediate reds are four above their own dark endpoint (5→9 and12→16). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    internal const int SelectedDarkRedLift = 4;
}
