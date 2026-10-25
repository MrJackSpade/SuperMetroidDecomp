using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Translation between <see cref="EnemyDefinitionId"/> and the bank-$A0 header pointers the cartridge names enemy
/// definitions by (#1412). The identity's values are arbitrary; <see cref="EnemyHeaderWords.Headers"/> is the mapping.
/// </summary>
internal static class EnemyDefinitionIds
{
    /// <summary>The species a stored bank-$A0 header pointer names.</summary>
    /// <exception cref="InvalidDataException">The pointer is not a labelled enemy header.</exception>
    internal static EnemyDefinitionId FromHeaderPointer(ushort pointer) =>
        TryFromHeaderPointer(pointer, out EnemyDefinitionId id)
            ? id
            : throw new InvalidDataException($"${pointer:X4} is not a bank-$A0 enemy header.");

    /// <summary>The species a stored bank-$A0 header pointer names, when it names one.</summary>
    internal static bool TryFromHeaderPointer(ushort pointer, out EnemyDefinitionId id) =>
        EnemyHeaderWords.Headers.TryDecode(pointer, out id) && id != EnemyDefinitionId.None;

    /// <summary>The bank-$A0 header pointer of <paramref name="id"/>; zero for the cleared slot.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a member.</exception>
    internal static ushort HeaderPointer(EnemyDefinitionId id) => EnemyHeaderWords.Headers.Word(id);
}
