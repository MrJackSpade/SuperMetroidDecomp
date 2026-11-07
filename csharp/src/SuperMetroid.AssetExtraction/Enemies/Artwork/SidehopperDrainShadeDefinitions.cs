namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Exact RGB5 shade relationships in the final Sidehopper drain image.</summary>
internal static class SidehopperDrainShadeDefinitions
{
    /// <summary>Both upper intermediate reds are seven below their own light endpoint (31→24 and27→20). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    internal const int SelectedLightRedDrop = 7;
    /// <summary>Both lower intermediate reds are four above their own dark endpoint (5→9 and12→16). Reviewed shared material-shade offset for the shifted Sidehopper drain only.</summary>
    internal const int SelectedDarkRedLift = 4;
}
