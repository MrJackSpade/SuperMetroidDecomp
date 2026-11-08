using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between the typed <see cref="EnemyAiStates"/> and the AI-handler word the cartridge stores (#1472,
/// #1549), for the native-snapshot import and the checks that compare enemy slots with native memory. The states'
/// values are arbitrary: each one's word bit is named here, and every conversion walks this table bit by bit.
/// </summary>
[EncodingOwner("enemy AI-state word")]
internal static class EnemyAiStatesWordCodec
{
    /// <summary>Each state's bit in the AI-handler word.</summary>
    private static readonly Dictionary<EnemyAiStates, int> StateBits = new()
    {
        [EnemyAiStates.Grappled] = 1 << 0,
        [EnemyAiStates.Hurt] = 1 << 1,
        [EnemyAiStates.Frozen] = 1 << 2,
    };

    /// <summary>The stored AI-handler word for a set of states.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="states"/> holds a value no state names.</exception>
    public static int Encode(EnemyAiStates states)
    {
        int word = 0;
        EnemyAiStates encoded = EnemyAiStates.None;
        foreach ((EnemyAiStates state, int bit) in StateBits)
        {
            if ((states & state) == EnemyAiStates.None)
                continue;
            word |= bit;
            encoded |= state;
        }
        return encoded == states
            ? word
            : throw new ArgumentOutOfRangeException(nameof(states), states, "The value holds a bit no AI state names.");
    }

    /// <summary>The states a stored AI-handler word holds.</summary>
    /// <exception cref="InvalidDataException">The word holds a bit no AI state names.</exception>
    public static EnemyAiStates Decode(int word)
    {
        Ensure.BetweenInclusive(0, ushort.MaxValue, word);
        EnemyAiStates states = EnemyAiStates.None;
        foreach ((EnemyAiStates state, int bit) in StateBits)
            if ((word & bit) != 0)
                states |= state;
        return Encode(states) == word
            ? states
            : throw new InvalidDataException($"Enemy AI-handler word ${word:X4} holds a bit no AI state names.");
    }
}
