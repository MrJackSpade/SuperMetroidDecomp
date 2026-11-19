namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="CollectibleBlockPairs.Pair"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class CollectibleBlockPairKeys
{
    /// <summary>Each block pair's installed key.</summary>
    internal static InstalledKeys<CollectibleBlockPairs.Pair> Installed { get; } = new(new Dictionary<CollectibleBlockPairs.Pair, string>
    {
        [CollectibleBlockPairs.Pair.First] = "First",
        [CollectibleBlockPairs.Pair.Second] = "Second",
        [CollectibleBlockPairs.Pair.Third] = "Third",
        [CollectibleBlockPairs.Pair.Fourth] = "Fourth",
    });
}
