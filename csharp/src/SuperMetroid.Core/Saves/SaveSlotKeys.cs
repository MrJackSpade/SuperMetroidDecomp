namespace SuperMetroid.Core.Saves;

/// <summary>
/// The installed key of each <see cref="SaveSlot"/> (#2520, #2565), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class SaveSlotKeys
{
    /// <summary>Each SaveSlot member's installed key.</summary>
    internal static InstalledKeys<SaveSlot> Installed { get; } = new(new Dictionary<SaveSlot, string>
    {
        [SaveSlot.A] = "A",
        [SaveSlot.B] = "B",
        [SaveSlot.C] = "C",
    });
}
