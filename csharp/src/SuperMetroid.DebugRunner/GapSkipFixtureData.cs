/// <summary>Native support boundaries for the deterministic running gap-skip fixture.</summary>
internal static class GapSkipFixtureData
{
    /// <summary>Radius-21 center on the row-32 upper platforms after a downward grounding scan.</summary>
    public const uint PlatformLandingCenter = 0x01ebffff;

    /// <summary>Radius-19 center after expansion catches the far edge, one frame before landing.</summary>
    public const uint ExpandedEdgeCenter = 0x01edffff;

    /// <summary>Top boundary of the lower row-48 floor which catches failed gap skips.</summary>
    public const int LowerFloorTop = 768;
}
