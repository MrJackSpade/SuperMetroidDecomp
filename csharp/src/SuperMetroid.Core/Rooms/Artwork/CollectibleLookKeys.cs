namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed key of each <see cref="CollectibleLook"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class CollectibleLookKeys
{
    /// <summary>Each CollectibleLook member's installed key.</summary>
    internal static InstalledKeys<CollectibleLook> Installed { get; } = new(new Dictionary<CollectibleLook, string>
    {
        [CollectibleLook.Primary] = "Primary",
        [CollectibleLook.Alternate] = "Alternate",
    });
}
