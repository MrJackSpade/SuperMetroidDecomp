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
    /// $A6:C6CE lifetimes and $A6:C6E6 initializer pointers, selected by the even
    /// parameter at $0FB4. Tail segments expire eight frames apart and their
    /// equal-sized initializers advance by 24 bytes.
    /// </summary>
    public static RidleyExplosionPartDefinition GetPart(ushort parameter)
    {
        if ((parameter & 1) != 0 || parameter > RidleyExplosionParts.Claw)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        int part = parameter >> 1;
        if (parameter <= RidleyExplosionParts.TailTip)
            return new(parameter, (ushort)(0x48 + 8 * part), (ushort)(0xc6fe + 0x18 * part));

        // Body initializers have the same two-facing layout, so their native
        // addresses advance by 50 bytes. Torso is the last fragment to expire;
        // the other body parts follow the eight-frame lifetime progression.
        int body = (parameter - RidleyExplosionParts.Wings) / 2;
        ushort lifetime = parameter == RidleyExplosionParts.Torso ? (ushort)0x80
            : (ushort)(0x28 + 8 * (body - (parameter == RidleyExplosionParts.Claw ? 1 : 0)));
        return new(parameter, lifetime, (ushort)(0xc7da + 0x32 * body));
    }

    /// <summary>Returns one authored small-explosion position selected by its cyclic index.</summary>
    public static RidleyDeathExplosionPlacement DeathExplosionPlacement(int index)
    {
        if ((uint)index >= DeathExplosionPlacements.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return DeathExplosionPlacements[index];
    }

    /// <summary>
    /// Selects the fixed tail instruction list loaded by <c>$A6:C6FE-$A6:C7B9</c>. Each of the sixteen tip programs selected at $A6:C7BA occupies six bytes.
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
            (ushort)(0xca95 + 6 * tailTipOrientation),
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
