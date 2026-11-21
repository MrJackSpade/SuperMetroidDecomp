namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The installed key of each <see cref="FileSelectItem"/> (#2520, #2565), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class FileSelectItemKeys
{
    /// <summary>Each FileSelectItem member's installed key.</summary>
    internal static InstalledKeys<FileSelectItem> Installed { get; } = new(new Dictionary<FileSelectItem, string>
    {
        [FileSelectItem.SlotA] = "SlotA",
        [FileSelectItem.SlotB] = "SlotB",
        [FileSelectItem.SlotC] = "SlotC",
        [FileSelectItem.Copy] = "Copy",
        [FileSelectItem.Clear] = "Clear",
        [FileSelectItem.Exit] = "Exit",
    });
}
