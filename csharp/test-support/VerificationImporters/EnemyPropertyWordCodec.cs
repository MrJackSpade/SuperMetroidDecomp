using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between the typed <see cref="EnemyProperties"/> and <see cref="EnemyExtraProperties"/> and the two
/// property words the cartridge stores them in (#1472), for the checks, fixtures and native-snapshot imports that
/// compare with native memory. Each flag is the word's bit of the same value. The property word's low byte is not an
/// engine property: a few species keep their own settings there, which the species' settings and state hold typed.
/// </summary>
[EncodingOwner("enemy property and extra-property words")]
internal static class EnemyPropertyWordCodec
{
    /// <summary>The property word's bits that hold engine properties: its high byte.</summary>
    private const int EngineBits = 0xff00;

    /// <summary>Every additional property the extra-property word can hold.</summary>
    private const EnemyExtraProperties NamedExtraProperties =
        EnemyExtraProperties.UsesExtendedSpritemap | EnemyExtraProperties.NewInstructionFrame;

    /// <summary>The engine properties as their stored word, with a clear low byte.</summary>
    public static int Encode(EnemyProperties properties) => (int)properties;

    /// <summary>The additional properties as their stored word.</summary>
    public static int Encode(EnemyExtraProperties properties) => (int)properties;

    /// <summary>A stored property word that holds only engine properties.</summary>
    /// <exception cref="InvalidDataException">The word's low byte holds species settings.</exception>
    public static EnemyProperties Decode(int word) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, word) & ~EngineBits) == 0
            ? (EnemyProperties)word
            : throw new InvalidDataException($"Enemy property word ${word:X4} holds species settings in its low byte.");

    /// <summary>The engine properties of a stored word, leaving the species settings of its low byte to the species' import.</summary>
    public static EnemyProperties EngineFlagsOf(int word) => (EnemyProperties)(Ensure.BetweenInclusive(0, ushort.MaxValue, word) & EngineBits);

    /// <summary>The species settings a stored property word keeps in its low byte.</summary>
    public static int SpeciesBitsOf(int word) => Ensure.BetweenInclusive(0, ushort.MaxValue, word) & ~EngineBits;

    /// <summary>A stored extra-property word that holds only named additional properties.</summary>
    /// <exception cref="InvalidDataException">The word holds a bit no additional property names.</exception>
    public static EnemyExtraProperties DecodeExtra(int word) =>
        ((EnemyExtraProperties)Ensure.BetweenInclusive(0, ushort.MaxValue, word) & ~NamedExtraProperties) == EnemyExtraProperties.None
            ? (EnemyExtraProperties)word
            : throw new InvalidDataException($"Enemy extra-property word ${word:X4} holds a bit no additional property names.");

    /// <summary>The named additional properties of a stored word, dropping bits no port behaviour reads.</summary>
    public static EnemyExtraProperties ExtraFlagsOf(int word) =>
        (EnemyExtraProperties)Ensure.BetweenInclusive(0, ushort.MaxValue, word) & NamedExtraProperties;
}
