using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between the typed <see cref="EnemyProperties"/> and <see cref="EnemyExtraProperties"/> and the two
/// property words the cartridge stores them in (#1472, #1549), for the checks, fixtures and native-snapshot imports
/// that compare with native memory. The members' values are arbitrary: each one's word bit is named here, and every
/// conversion walks these tables bit by bit. The property word's low byte is not an engine property: a few species keep
/// their own settings there, which the species' settings and state hold typed.
/// </summary>
[EncodingOwner("enemy property and extra-property words")]
internal static class EnemyPropertyWordCodec
{
    /// <summary>The property word's bits that hold engine properties: its high byte.</summary>
    private const int EngineBits = 0xff00;

    /// <summary>Each engine property's bit in the property word.</summary>
    private static readonly Dictionary<EnemyProperties, int> PropertyBits = new()
    {
        [EnemyProperties.Invisible] = 1 << 8,
        [EnemyProperties.Deleted] = 1 << 9,
        [EnemyProperties.IgnoreSamusCollision] = 1 << 10,
        [EnemyProperties.ProcessOffScreen] = 1 << 11,
        [EnemyProperties.BlocksPlasmaBeam] = 1 << 12,
        [EnemyProperties.RunsAnimationScript] = 1 << 13,
        [EnemyProperties.RespawnIfKilled] = 1 << 14,
        [EnemyProperties.SolidToSamus] = 1 << 15,
    };

    /// <summary>Each additional property's bit in the extra-property word.</summary>
    private static readonly Dictionary<EnemyExtraProperties, int> ExtraPropertyBits = new()
    {
        [EnemyExtraProperties.UsesExtendedSpritemap] = 1 << 2,
        [EnemyExtraProperties.NewInstructionFrame] = 1 << 15,
    };

    /// <summary>The engine properties as their stored word, with a clear low byte.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="properties"/> holds a value no property names.</exception>
    public static int Encode(EnemyProperties properties)
    {
        int word = 0;
        EnemyProperties encoded = EnemyProperties.None;
        foreach ((EnemyProperties flag, int bit) in PropertyBits)
        {
            if ((properties & flag) == EnemyProperties.None)
                continue;
            word |= bit;
            encoded |= flag;
        }
        return encoded == properties
            ? word
            : throw new ArgumentOutOfRangeException(nameof(properties), properties, "The value holds a bit no engine property names.");
    }

    /// <summary>The additional properties as their stored word.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="properties"/> holds a value no property names.</exception>
    public static int Encode(EnemyExtraProperties properties)
    {
        int word = 0;
        EnemyExtraProperties encoded = EnemyExtraProperties.None;
        foreach ((EnemyExtraProperties flag, int bit) in ExtraPropertyBits)
        {
            if ((properties & flag) == EnemyExtraProperties.None)
                continue;
            word |= bit;
            encoded |= flag;
        }
        return encoded == properties
            ? word
            : throw new ArgumentOutOfRangeException(nameof(properties), properties, "The value holds a bit no additional property names.");
    }

    /// <summary>A stored property word that holds only engine properties.</summary>
    /// <exception cref="InvalidDataException">The word's low byte holds species settings.</exception>
    public static EnemyProperties Decode(int word) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, word) & ~EngineBits) == 0
            ? EngineFlagsOf(word)
            : throw new InvalidDataException($"Enemy property word ${word:X4} holds species settings in its low byte.");

    /// <summary>The engine properties of a stored word, leaving the species settings of its low byte to the species' import.</summary>
    public static EnemyProperties EngineFlagsOf(int word)
    {
        Ensure.BetweenInclusive(0, ushort.MaxValue, word);
        EnemyProperties properties = EnemyProperties.None;
        foreach ((EnemyProperties flag, int bit) in PropertyBits)
            if ((word & bit) != 0)
                properties |= flag;
        return properties;
    }

    /// <summary>The species settings a stored property word keeps in its low byte.</summary>
    public static int SpeciesBitsOf(int word) => Ensure.BetweenInclusive(0, ushort.MaxValue, word) & ~EngineBits;

    /// <summary>A stored extra-property word that holds only named additional properties.</summary>
    /// <exception cref="InvalidDataException">The word holds a bit no additional property names.</exception>
    public static EnemyExtraProperties DecodeExtra(int word)
    {
        EnemyExtraProperties properties = ExtraFlagsOf(word);
        return Encode(properties) == word
            ? properties
            : throw new InvalidDataException($"Enemy extra-property word ${word:X4} holds a bit no additional property names.");
    }

    /// <summary>The named additional properties of a stored word, dropping bits no port behaviour reads.</summary>
    public static EnemyExtraProperties ExtraFlagsOf(int word)
    {
        Ensure.BetweenInclusive(0, ushort.MaxValue, word);
        EnemyExtraProperties properties = EnemyExtraProperties.None;
        foreach ((EnemyExtraProperties flag, int bit) in ExtraPropertyBits)
            if ((word & bit) != 0)
                properties |= flag;
        return properties;
    }
}
