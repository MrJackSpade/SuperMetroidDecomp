namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy.properties (+$0E) as a typed value (#1323): the engine-owned high byte is named <see cref="EnemyProperties"/>
/// flags, and the low byte is species-owned data the common engine never interprets (crawler and Yard facing, Metaree
/// descent, Skree setup bits).
/// </summary>
public readonly record struct EnemyPropertyWord
{
    /// <summary>Creates a property word from named flags and the species-owned low byte.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="flags"/> contains low-byte or undefined bits, or <paramref name="familyData"/> is not a byte.
    /// </exception>
    public EnemyPropertyWord(EnemyProperties flags, int familyData = 0)
    {
        EnemyPropertyFlagExtensions.ValidateKnown(flags);
        Flags = flags;
        FamilyData = Ensure.BetweenInclusive(0, byte.MaxValue, familyData);
    }

    /// <summary>The named engine flags of the high byte.</summary>
    public EnemyProperties Flags { get; }

    /// <summary>
    /// The species-owned family bits, 0 through 255. Every producer supplies a byte (the constructor and
    /// <see cref="WithFamilyData"/> reject anything else, and decoding takes the word's low byte), so the value cannot
    /// leave that range, and the native word places it unchanged beside the flags.
    /// </summary>
    public int FamilyData { get; }

    /// <summary>A native word as a property value; an unnamed high-byte flag is corrupt.</summary>
    public static EnemyPropertyWord FromNativeWord(int word) => EnemyPropertyWordCodec.Decode(checked((ushort)word));

    /// <summary>The native word.</summary>
    public ushort ToNativeWord() => EnemyPropertyWordCodec.Encode(this);

    /// <summary>Whether any of <paramref name="flags"/> is set.</summary>
    public bool HasAny(EnemyProperties flags) => (Flags & flags) != EnemyProperties.None;

    /// <summary>This value with <paramref name="flags"/> set.</summary>
    public EnemyPropertyWord With(EnemyProperties flags) => new(Flags | flags, checked((byte)FamilyData));

    /// <summary>This value with <paramref name="flags"/> cleared.</summary>
    public EnemyPropertyWord Without(EnemyProperties flags) => new(Flags & ~flags, checked((byte)FamilyData));

    /// <summary>This value with <paramref name="clear"/> cleared, then <paramref name="set"/> set.</summary>
    public EnemyPropertyWord Replace(EnemyProperties clear, EnemyProperties set) => Without(clear).With(set);

    /// <summary>This value with a new species-owned low byte.</summary>
    public EnemyPropertyWord WithFamilyData(int familyData) => new(Flags, familyData);
}
