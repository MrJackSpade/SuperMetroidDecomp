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
    /// <summary>$A6:CA95 InstList_RidleyTailTip_PointingDown, first of sixteen orientation programs.</summary>
    private const ushort TailTipProgramStart = 0xca95;
    /// <summary>
    /// Native allocation order encoded by <c>SpawnEnemy_RidleyExplosion</c> at
    /// <c>$A6:C932-$A6:C986</c>. Enemy-slot/OAM order makes this sequence observable.
    /// </summary>
    internal static void SpawnInNativeOrder(Action<ushort> spawn)
    {
        Ensure.NotNull(spawn);
        for (int parameter = RidleyExplosionParts.TailTip; parameter >= RidleyExplosionParts.Tail0;
             parameter -= RidleyExplosionParts.Tail1 - RidleyExplosionParts.Tail0)
            spawn((ushort)parameter);
        spawn(RidleyExplosionParts.Wings);
        spawn(RidleyExplosionParts.Legs);
        spawn(RidleyExplosionParts.Torso);
        spawn(RidleyExplosionParts.OpenHeadAndNeck);
        spawn(RidleyExplosionParts.Claw);
    }

    /// <summary>
    /// $A6:C6CE lifetimes and $A6:C6E6 initializer pointers, selected by the even
    /// parameter at $0FB4. Tail segments expire eight frames apart and their
    /// equal-sized initializers advance by 24 bytes. Lifetime bases 72/40, the shared
    /// step 8 and torso lifetime 128 are authored breakup timing (see residualScalarInputsReview).
    /// </summary>
    public static RidleyExplosionPartDefinition GetPart(ushort parameter)
    {
        if ((parameter & 1) != 0 || parameter > RidleyExplosionParts.Claw)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        int part = parameter >> 1;
        if (parameter <= RidleyExplosionParts.TailTip)
            return new((ushort)(0x48 + 8 * part));

        // Body initializers have the same two-facing layout, so their native
        // addresses advance by 50 bytes. Torso is the last fragment to expire;
        // the other body parts follow the eight-frame lifetime progression.
        int body = (parameter - RidleyExplosionParts.Wings) / 2;
        ushort lifetime = parameter == RidleyExplosionParts.Torso ? (ushort)0x80
            : (ushort)(0x28 + 8 * (body - (parameter == RidleyExplosionParts.Claw ? 1 : 0)));
        return new(lifetime);
    }

    /// <summary>
    /// Returns one of the ten body-relative positions consumed cyclically by
    /// <c>SpawnSmallExplosionNearRidley</c> at <c>$A6:C623</c> (words $A6:C66E-$A6:C695,
    /// shared with the Baby Metroid death).
    /// </summary>
    public static RidleyDeathExplosionPlacement DeathExplosionPlacement(int index)
    {
        (short x, short y) = DeathExplosionScatterDefinitions.Offset(index);
        return new(x, y);
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
            (ushort)(TailTipProgramStart + 6 * tailTipOrientation),
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

/// <summary>Lifetime value returned for one Ridley breakup actor parameter.</summary>
/// <param name="Lifetime">Cartridge-authored frame lifetime copied into the fragment actor's timer state.</param>
internal readonly record struct RidleyExplosionPartDefinition(
    ushort Lifetime);

/// <summary>Facing-specific offsets and animation selection for one non-tail Ridley fragment.</summary>
/// <param name="XOffset">Signed horizontal displacement from the parent Ridley position for the fragment's body origin.</param>
/// <param name="YOffset">Signed vertical displacement from the parent Ridley position for the fragment's body origin.</param>
/// <param name="InstructionList">Bank-$A6 pointer to the fragment's facing-specific native animation instruction list.</param>
internal readonly record struct RidleyExplosionBodyPartDefinition(
    short XOffset,
    short YOffset,
    ushort InstructionList);

/// <summary>One signed scatter offset used to place a small explosion around Ridley's death breakup.</summary>
/// <param name="XOffset">Horizontal displacement from Ridley's death-explosion anchor, in game-coordinate units.</param>
/// <param name="YOffset">Vertical displacement from Ridley's death-explosion anchor, in game-coordinate units.</param>
internal readonly record struct RidleyDeathExplosionPlacement(
    short XOffset,
    short YOffset);
