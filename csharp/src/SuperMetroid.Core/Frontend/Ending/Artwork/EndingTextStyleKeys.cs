namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The installed key of each <see cref="EndingTextStyle"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class EndingTextStyleKeys
{
    /// <summary>Each EndingTextStyle member's installed key.</summary>
    internal static InstalledKeys<EndingTextStyle> Installed { get; } = new(new Dictionary<EndingTextStyle, string>
    {
        [EndingTextStyle.ResultSmall] = "ResultSmall",
        [EndingTextStyle.CopyrightLarge] = "CopyrightLarge",
        [EndingTextStyle.PercentageSmall] = "PercentageSmall",
        [EndingTextStyle.FinalLarge] = "FinalLarge",
    });
}
