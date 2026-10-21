using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Decoding for <see cref="EnemyDefinitionId"/>.</summary>
internal static class EnemyDefinitionIds
{
    /// <summary>The species a stored bank-$A0 header pointer names.</summary>
    /// <exception cref="InvalidDataException">The pointer is not a labelled enemy header.</exception>
    internal static EnemyDefinitionId FromHeaderPointer(ushort pointer)
    {
        var id = (EnemyDefinitionId)pointer;
        return id != EnemyDefinitionId.None && Enum.IsDefined(id)
            ? id
            : throw new InvalidDataException($"${pointer:X4} is not a bank-$A0 enemy header.");
    }
}
