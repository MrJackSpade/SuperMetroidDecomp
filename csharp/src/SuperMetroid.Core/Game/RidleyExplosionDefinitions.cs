namespace SuperMetroid.Core.Game;

/// <summary>Parameters that select one authored component of Ridley's death breakup.</summary>
internal static class RidleyExplosionParts
{
    /// <summary>First tail segment.</summary>
    public const ushort Tail0 = 0x0000;
    /// <summary>Second tail segment.</summary>
    public const ushort Tail1 = 0x0002;
    /// <summary>Third tail segment.</summary>
    public const ushort Tail2 = 0x0004;
    /// <summary>Fourth tail segment.</summary>
    public const ushort Tail3 = 0x0006;
    /// <summary>Fifth tail segment.</summary>
    public const ushort Tail4 = 0x0008;
    /// <summary>Sixth tail segment.</summary>
    public const ushort Tail5 = 0x000a;
    /// <summary>Seventh segment, whose image is selected from the combined tip angle.</summary>
    public const ushort TailTip = 0x000c;
    /// <summary>Wing fragment.</summary>
    public const ushort Wings = 0x000e;
    /// <summary>Leg fragment.</summary>
    public const ushort Legs = 0x0010;
    /// <summary>Open-head and neck fragment.</summary>
    public const ushort OpenHeadAndNeck = 0x0012;
    /// <summary>Torso fragment.</summary>
    public const ushort Torso = 0x0014;
    /// <summary>Claw fragment.</summary>
    public const ushort Claw = 0x0016;
}

/// <summary>
/// Compiled bank-$A6 mechanics and dispatch metadata for Lower Norfair Ridley's
/// twelve death-breakup actors. The selected instruction streams remain ROM-backed.
/// </summary>
internal static class RidleyExplosionDefinitions
{
    /// <summary><c>EnemyHeaders_RidleyExplosion</c> at <c>$A0:E1BF</c>.</summary>
    public const ushort EnemyDefinition = 0xe1bf;

    /// <summary>
    /// Native allocation order encoded by <c>SpawnEnemy_RidleyExplosion</c> at
    /// <c>$A6:C932-$A6:C986</c>. Enemy-slot/OAM order makes this sequence observable.
    /// </summary>
    public static ReadOnlySpan<ushort> SpawnOrder =>
    [
        RidleyExplosionParts.TailTip,
        RidleyExplosionParts.Tail5,
        RidleyExplosionParts.Tail4,
        RidleyExplosionParts.Tail3,
        RidleyExplosionParts.Tail2,
        RidleyExplosionParts.Tail1,
        RidleyExplosionParts.Tail0,
        RidleyExplosionParts.Wings,
        RidleyExplosionParts.Legs,
        RidleyExplosionParts.Torso,
        RidleyExplosionParts.OpenHeadAndNeck,
        RidleyExplosionParts.Claw,
    ];

    /// <summary>
    /// Parameters, lifetimes from <c>$A6:C6CE-$A6:C6E5</c>, and native initializer
    /// callbacks from <c>$A6:C6E6-$A6:C6FD</c>, in parameter-index order.
    /// </summary>
    private static readonly RidleyExplosionPartDefinition[] PartRecords =
    [
        new(0x0000, 0x0048, 0xc6fe),
        new(0x0002, 0x0050, 0xc716),
        new(0x0004, 0x0058, 0xc72e),
        new(0x0006, 0x0060, 0xc746),
        new(0x0008, 0x0068, 0xc75e),
        new(0x000a, 0x0070, 0xc776),
        new(0x000c, 0x0078, 0xc78e),
        new(0x000e, 0x0028, 0xc7da),
        new(0x0010, 0x0030, 0xc80c),
        new(0x0012, 0x0038, 0xc83e),
        new(0x0014, 0x0080, 0xc870),
        new(0x0016, 0x0040, 0xc8a2),
    ];

    /// <summary>
    /// Ten signed body-relative positions consumed cyclically by
    /// <c>SpawnSmallExplosionNearRidley</c> at <c>$A6:C623</c>. The interleaved
    /// X/Y words occupy <c>$A6:C66E-$A6:C695</c>.
    /// </summary>
    private static readonly RidleyDeathExplosionPlacement[] DeathExplosionPlacements =
    [
        new(-24, -24),
        new(-20, 20),
        new(16, -30),
        new(30, -3),
        new(14, -13),
        new(-2, 18),
        new(-2, -32),
        new(-31, 8),
        new(-4, -10),
        new(19, 19),
    ];

    /// <summary>
    /// Tail-tip instruction-list selectors at <c>$A6:C7BA-$A6:C7D9</c>, ordered by
    /// the high nibble of the rounded combined segment-five/tip angle.
    /// </summary>
    private static ReadOnlySpan<ushort> TailTipInstructionLists =>
    [
        0xca95, 0xca9b, 0xcaa1, 0xcaa7,
        0xcaad, 0xcab3, 0xcab9, 0xcabf,
        0xcac5, 0xcacb, 0xcad1, 0xcad7,
        0xcadd, 0xcae3, 0xcae9, 0xcaef,
    ];

    /// <summary>Returns the definition selected by the even parameter at <c>$0FB4</c>.</summary>
    public static RidleyExplosionPartDefinition GetPart(ushort parameter)
    {
        if ((parameter & 1) != 0 || parameter > RidleyExplosionParts.Claw)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        return PartRecords[parameter >> 1];
    }

    /// <summary>Returns one authored small-explosion position selected by its cyclic index.</summary>
    public static RidleyDeathExplosionPlacement DeathExplosionPlacement(int index)
    {
        if ((uint)index >= DeathExplosionPlacements.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return DeathExplosionPlacements[index];
    }

    /// <summary>
    /// Selects the fixed tail instruction list loaded by <c>$A6:C6FE-$A6:C7B9</c>.
    /// <paramref name="tailTipOrientation"/> is ignored for the first six segments.
    /// </summary>
    public static ushort SelectTailInstructionList(
        ushort parameter,
        int tailTipOrientation) => parameter switch
    {
        RidleyExplosionParts.Tail0 or RidleyExplosionParts.Tail1 => 0xca47,
        RidleyExplosionParts.Tail2 or RidleyExplosionParts.Tail3 => 0xca4d,
        RidleyExplosionParts.Tail4 or RidleyExplosionParts.Tail5 => 0xca53,
        RidleyExplosionParts.TailTip when (uint)tailTipOrientation < 16 =>
            TailTipInstructionLists[tailTipOrientation],
        RidleyExplosionParts.TailTip =>
            throw new ArgumentOutOfRangeException(nameof(tailTipOrientation)),
        _ => throw new ArgumentOutOfRangeException(nameof(parameter)),
    };

    /// <summary>
    /// Selects the body-relative position and instruction-list pointer loaded by
    /// <c>$A6:C7DA-$A6:C8D3</c>. Facing zero selects the native left-facing record.
    /// </summary>
    public static RidleyExplosionBodyPartDefinition SelectBodyPart(
        ushort parameter,
        bool facingRight) => (parameter, facingRight) switch
    {
        (RidleyExplosionParts.Wings, false) => new(0, 0, 0xca59),
        (RidleyExplosionParts.Wings, true) => new(0, 0, 0xca5f),
        (RidleyExplosionParts.Legs, false) => new(15, 22, 0xca65),
        (RidleyExplosionParts.Legs, true) => new(-15, 22, 0xca6b),
        (RidleyExplosionParts.OpenHeadAndNeck, false) => new(-3, -24, 0xca71),
        (RidleyExplosionParts.OpenHeadAndNeck, true) => new(3, -24, 0xca77),
        (RidleyExplosionParts.Torso, false) => new(16, 0, 0xca7d),
        (RidleyExplosionParts.Torso, true) => new(-16, 0, 0xca83),
        (RidleyExplosionParts.Claw, false) => new(8, 7, 0xca89),
        (RidleyExplosionParts.Claw, true) => new(-8, 7, 0xca8f),
        _ => throw new ArgumentOutOfRangeException(nameof(parameter)),
    };
}

/// <summary>One native Ridley-breakup lifetime and initializer-dispatch record.</summary>
internal readonly record struct RidleyExplosionPartDefinition(
    ushort Parameter,
    ushort Lifetime,
    ushort InitializationRoutine);

/// <summary>One facing-specific body fragment placement and animation selector.</summary>
internal readonly record struct RidleyExplosionBodyPartDefinition(
    short XOffset,
    short YOffset,
    ushort InstructionList);

/// <summary>One signed body-relative position in Ridley's death-explosion cycle.</summary>
internal readonly record struct RidleyDeathExplosionPlacement(
    short XOffset,
    short YOffset);
