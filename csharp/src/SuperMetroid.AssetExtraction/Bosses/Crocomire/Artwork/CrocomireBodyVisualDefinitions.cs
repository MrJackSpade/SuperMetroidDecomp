namespace SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;

/// <summary>
/// The fifty distinct fight-body extended-frame roots selected by Crocomire's
/// compiled animation operands. Forty-two have both ordinary sprite
/// components and body-layer streams; the final eight death-transition roots have
/// only ordinary sprite components. Skeleton poses use a separate later range.
/// </summary>
internal static class CrocomireBodyVisualDefinitions
{
    /// <summary>Fifty selected fight-body roots exclude the later skeleton range.</summary>
    internal const int BodyFrameCount = 50;
}
