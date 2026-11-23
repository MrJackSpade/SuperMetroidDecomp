namespace SuperMetroid.Core.Saves;

/// <summary>
/// The installed key of each <see cref="SaveFileHistory"/> (#2560), written out so renaming a member never changes a name a
/// save spells it by.
/// </summary>
internal static partial class SaveFileHistoryKeys
{
    /// <summary>Each SaveFileHistory member's installed key.</summary>
    internal static InstalledKeys<SaveFileHistory> Installed { get; } = new(new Dictionary<SaveFileHistory, string>
    {
        [SaveFileHistory.None] = "None",
        [SaveFileHistory.GameCompleted] = "GameCompleted",
        [SaveFileHistory.PowerOnFoundNoValidSave] = "PowerOnFoundNoValidSave",
    });
}
