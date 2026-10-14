using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Compiled bank-$A6 mechanics and dispatch metadata for Lower Norfair Ridley's
/// twelve death-breakup actors. The selected instruction streams remain ROM-backed.
/// </summary>
internal static class RidleyExplosionDefinitions
{
    /// <summary>$A6:CA95 InstList_RidleyTailTip_PointingDown, first of sixteen orientation programs.</summary>
    private const ushort TailTipProgramStart = 0xca95;
    /// <summary>
    /// Native allocation order encoded by <c>SpawnEnemy_RidleyExplosion</c> at
    /// <c>$A6:C932-$A6:C986</c>. Enemy-slot/OAM order makes this sequence observable.
    /// </summary>
    internal static void SpawnInNativeOrder(Action<ushort> spawn)
    {
        Ensure.NotNull(spawn);
        for (int parameter = (int)RidleyExplosionPart.TailTip; parameter >= (int)RidleyExplosionPart.Tail0;
             parameter -= RidleyExplosionPart.Tail1 - RidleyExplosionPart.Tail0)
            spawn((ushort)parameter);
        spawn((ushort)RidleyExplosionPart.Wings);
        spawn((ushort)RidleyExplosionPart.Legs);
        spawn((ushort)RidleyExplosionPart.Torso);
        spawn((ushort)RidleyExplosionPart.OpenHeadAndNeck);
        spawn((ushort)RidleyExplosionPart.Claw);
    }

    /// <summary>
    /// $A6:C6CE lifetimes and $A6:C6E6 initializer pointers, selected by the even
    /// parameter at $0FB4. Tail segments expire eight frames apart and their
    /// equal-sized initializers advance by 24 bytes. Lifetime bases 72/40, the shared
    /// step 8 and torso lifetime 128 are authored breakup timing (see residualScalarInputsReview).
    /// </summary>
    public static RidleyExplosionPartDefinition GetPart(ushort parameter)
    {
        if ((parameter & 1) != 0 || parameter > (ushort)RidleyExplosionPart.Claw)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        int part = parameter >> 1;
        if (parameter <= (ushort)RidleyExplosionPart.TailTip)
            return new((ushort)(0x48 + 8 * part));

        // Body initializers have the same two-facing layout, so their native
        // addresses advance by 50 bytes. Torso is the last fragment to expire;
        // the other body parts follow the eight-frame lifetime progression.
        int body = (parameter - (ushort)RidleyExplosionPart.Wings) / 2;
        ushort lifetime = parameter == (ushort)RidleyExplosionPart.Torso ? (ushort)0x80
            : (ushort)(0x28 + 8 * (body - (parameter == (ushort)RidleyExplosionPart.Claw ? 1 : 0)));
        return new(lifetime);
    }

    /// <summary>
    /// Returns one of the ten body-relative positions consumed cyclically by
    /// <c>SpawnSmallExplosionNearRidley</c> at <c>$A6:C623</c> (words $A6:C66E-$A6:C695,
    /// shared with the Baby Metroid death).
    /// </summary>
    public static Offset16<RoomPixels> DeathExplosionPlacement(int index) =>
        DeathExplosionScatterDefinitions.Offset(index);

    /// <summary>
    /// Selects the fixed tail instruction list loaded by <c>$A6:C6FE-$A6:C7B9</c>. Each of the sixteen tip programs selected at $A6:C7BA occupies six bytes.
    /// <paramref name="tailTipOrientation"/> is ignored for the first six segments.
    /// </summary>
    public static ushort SelectTailInstructionList(
        RidleyExplosionPart parameter,
        int tailTipOrientation) => parameter switch
    {
        RidleyExplosionPart.Tail0 or RidleyExplosionPart.Tail1 => 0xca47,
        RidleyExplosionPart.Tail2 or RidleyExplosionPart.Tail3 => 0xca4d,
        RidleyExplosionPart.Tail4 or RidleyExplosionPart.Tail5 => 0xca53,
        RidleyExplosionPart.TailTip when (uint)tailTipOrientation < 16 =>
            (ushort)(TailTipProgramStart + 6 * tailTipOrientation),
        RidleyExplosionPart.TailTip =>
            throw new ArgumentOutOfRangeException(nameof(tailTipOrientation)),
        RidleyExplosionPart.Wings or RidleyExplosionPart.Legs or RidleyExplosionPart.OpenHeadAndNeck or
            RidleyExplosionPart.Torso or RidleyExplosionPart.Claw =>
            throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Body fragments have no tail program."),
        _ => throw new InvalidOperationException($"Undefined Ridley explosion part {parameter}."),
    };

    /// <summary>
    /// Selects the body-relative position and instruction-list pointer loaded by
    /// <c>$A6:C7DA-$A6:C8D3</c>. Facing zero selects the native left-facing record.
    /// </summary>
    public static RidleyExplosionBodyPartDefinition SelectBodyPart(
        RidleyExplosionPart parameter,
        bool facingRight) => (parameter, facingRight) switch
    {
        (RidleyExplosionPart.Wings, false) => new(new(0, 0), 0xca59),
        (RidleyExplosionPart.Wings, true) => new(new(0, 0), 0xca5f),
        (RidleyExplosionPart.Legs, false) => new(new(15, 22), 0xca65),
        (RidleyExplosionPart.Legs, true) => new(new(-15, 22), 0xca6b),
        (RidleyExplosionPart.OpenHeadAndNeck, false) => new(new(-3, -24), 0xca71),
        (RidleyExplosionPart.OpenHeadAndNeck, true) => new(new(3, -24), 0xca77),
        (RidleyExplosionPart.Torso, false) => new(new(16, 0), 0xca7d),
        (RidleyExplosionPart.Torso, true) => new(new(-16, 0), 0xca83),
        (RidleyExplosionPart.Claw, false) => new(new(8, 7), 0xca89),
        (RidleyExplosionPart.Claw, true) => new(new(-8, 7), 0xca8f),
        _ => throw new ArgumentOutOfRangeException(nameof(parameter)),
    };
}
