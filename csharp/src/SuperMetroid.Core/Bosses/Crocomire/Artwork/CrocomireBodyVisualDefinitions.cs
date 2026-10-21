namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The fifty distinct fight-body extended-frame roots selected by Crocomire's
/// compiled bank-$A4 instruction operands. Forty-two have both ordinary OAM
/// components and BG2 streams; the final eight death-transition roots have
/// only ordinary OAM components. Skeleton poses use a separate later range.
/// </summary>
internal static class CrocomireBodyVisualDefinitions
{
    /// <summary>Fifty selected fight-body roots exclude the later skeleton range.</summary>
    internal const int BodyFrameCount = 50;
}
