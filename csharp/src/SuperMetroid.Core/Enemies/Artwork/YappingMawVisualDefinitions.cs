using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Presentation identities shown by the Yapping Maw's typed programs.</summary>
internal static class YappingMawVisualDefinitions
{
    /// <summary>$A8, the bank containing Yapping Maw's OAM data.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>
    /// The 24 distinct $A8:A8EE-$AAED spritemaps shown by the Yapping Maw's attack and cooldown programs, in address
    /// order. The native address remains the stable identity while the extracted composition is editable.
    /// </summary>
    internal static EnemySpritemapDefinition[] Frames() =>
        [.. new SortedSet<ushort>(YappingMawAnimations.Spritemaps(Enum.GetValues<YappingMawProgram>()))
            .Select(pointer => new EnemySpritemapDefinition(Bank, pointer, $"yapping_maw_a8_{pointer:x4}"))];
}
