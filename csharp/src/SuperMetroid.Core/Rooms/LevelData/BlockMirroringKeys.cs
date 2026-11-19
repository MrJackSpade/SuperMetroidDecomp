namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="BlockMirroring"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class BlockMirroringKeys
{
    /// <summary>Each BlockMirroring member's installed key.</summary>
    internal static InstalledKeys<BlockMirroring> Installed { get; } = new(new Dictionary<BlockMirroring, string>
    {
        [BlockMirroring.None] = "None",
        [BlockMirroring.Horizontal] = "Horizontal",
        [BlockMirroring.Vertical] = "Vertical",
        [BlockMirroring.Both] = "Both",
    });
}
