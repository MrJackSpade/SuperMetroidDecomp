namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy.properties (+$0E) as a typed value (#1323): the engine-owned high byte is named <see cref="EnemyProperties"/>
/// flags, and the low byte is species-owned data the common engine never interprets (crawler and Yard facing, Metaree
/// descent, Skree setup bits).
/// </summary>
public readonly record struct EnemyPropertyWord
{
    /// <summary>Creates a property word from named flags and the species-owned low byte.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains low-byte or undefined bits.</exception>
    public EnemyPropertyWord(EnemyProperties flags, byte familyData = 0)
    {
        EnemyPropertyFlagExtensions.ValidateKnown(flags);
        Flags = flags;
        FamilyData = familyData;
    }

    /// <summary>The named engine flags of the high byte.</summary>
    public EnemyProperties Flags { get; }

    /// <summary>The species-owned low byte.</summary>
    public byte FamilyData { get; }

    /// <summary>A native word as a property value; an unnamed high-byte flag is corrupt.</summary>
    public static EnemyPropertyWord FromNativeWord(ushort word) =>
        new(word.ReadFlagsChecked(), SnesLittleEndian.LowByte(word));

    /// <summary>The native word.</summary>
    public ushort ToNativeWord() => (ushort)((ushort)Flags | FamilyData);

    /// <summary>Whether any of <paramref name="flags"/> is set.</summary>
    public bool HasAny(EnemyProperties flags) => (Flags & flags) != 0;

    /// <summary>This value with <paramref name="flags"/> set.</summary>
    public EnemyPropertyWord With(EnemyProperties flags) => new(Flags | flags, FamilyData);

    /// <summary>This value with <paramref name="flags"/> cleared.</summary>
    public EnemyPropertyWord Without(EnemyProperties flags) => new(Flags & ~flags, FamilyData);

    /// <summary>This value with <paramref name="clear"/> cleared, then <paramref name="set"/> set.</summary>
    public EnemyPropertyWord Replace(EnemyProperties clear, EnemyProperties set) => Without(clear).With(set);

    /// <summary>This value with a new species-owned low byte.</summary>
    public EnemyPropertyWord WithFamilyData(byte familyData) => new(Flags, familyData);
}
