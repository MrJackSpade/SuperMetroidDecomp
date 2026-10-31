using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between the typed <see cref="EnemyAiStates"/> and the AI-handler word the cartridge stores (#1472), for
/// the checks that compare enemy slots with native memory. Each state is the word's bit of the same value.
/// </summary>
[EncodingOwner("enemy AI-state word")]
internal static class EnemyAiStatesWordCodec
{
    /// <summary>The stored AI-handler word for a set of states.</summary>
    public static int Encode(EnemyAiStates states) => (int)states;
}
