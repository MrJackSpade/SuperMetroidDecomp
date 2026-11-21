namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The installed key of each <see cref="YesNoChoice"/> (#2520, #2565), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class YesNoChoiceKeys
{
    /// <summary>Each YesNoChoice member's installed key.</summary>
    internal static InstalledKeys<YesNoChoice> Installed { get; } = new(new Dictionary<YesNoChoice, string>
    {
        [YesNoChoice.Yes] = "Yes",
        [YesNoChoice.No] = "No",
    });
}
