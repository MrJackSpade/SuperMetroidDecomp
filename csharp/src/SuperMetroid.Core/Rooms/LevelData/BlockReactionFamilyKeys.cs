namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="BlockReactionFamily"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class BlockReactionFamilyKeys
{
    /// <summary>Each BlockReactionFamily member's installed key.</summary>
    internal static InstalledKeys<BlockReactionFamily> Installed { get; } = new(new Dictionary<BlockReactionFamily, string>
    {
        [BlockReactionFamily.Shootable] = "Shootable",
        [BlockReactionFamily.Bombable] = "Bombable",
        [BlockReactionFamily.Special] = "Special",
        [BlockReactionFamily.SpecialAir] = "SpecialAir",
        [BlockReactionFamily.Spike] = "Spike",
        [BlockReactionFamily.Grapple] = "Grapple",
    });
}
