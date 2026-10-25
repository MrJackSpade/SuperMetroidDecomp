using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads recorded enemy slots' identity words as species, for checks against native captures.</summary>
internal static class EnemyDefinitionSlotWords
{
    /// <summary>The species an enemy slot's identity word names; zero is the cleared slot.</summary>
    /// <exception cref="InvalidDataException">The word is neither zero nor a labelled enemy header.</exception>
    internal static EnemyDefinitionId FromSlotWord(ushort word) =>
        word == 0 ? EnemyDefinitionId.None : EnemyDefinitionIds.FromHeaderPointer(word);
}
