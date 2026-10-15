using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Compiled bank-$A6 mechanics and dispatch metadata for Lower Norfair Ridley's
/// twelve death-breakup actors. The selected instruction streams remain ROM-backed.
/// </summary>
internal static class RidleyExplosionDefinitions
{
    /// <summary>The breakup initializer ($A6:C696) masks a random word to bits 4, 5 and 8 for a fragment's horizontal 8.8 speed.</summary>
    internal const ushort FragmentSpeedMask = 0x0130;
    /// <summary>The tail tip's sixteen orientations.</summary>
    private const int TailTipOrientationCount = 16;
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
    /// Selects the tail pose loaded by <c>$A6:C6FE-$A6:C7B9</c>; the tip's comes from its orientation, as the sixteen
    /// tip lists selected at $A6:C7BA do. <paramref name="tailTipOrientation"/> is ignored for the first six segments.
    /// </summary>
    public static RidleyBreakupFrame SelectTailFrame(
        RidleyExplosionPart parameter,
        int tailTipOrientation) => parameter switch
    {
        RidleyExplosionPart.Tail0 or RidleyExplosionPart.Tail1 => RidleyBreakupFrame.TailSegmentSmall,
        RidleyExplosionPart.Tail2 or RidleyExplosionPart.Tail3 => RidleyBreakupFrame.TailSegmentMedium,
        RidleyExplosionPart.Tail4 or RidleyExplosionPart.Tail5 => RidleyBreakupFrame.TailSegmentLarge,
        RidleyExplosionPart.TailTip when (uint)tailTipOrientation < TailTipOrientationCount =>
            RidleyBreakupFrame.TailTip0 + tailTipOrientation,
        RidleyExplosionPart.TailTip =>
            throw new ArgumentOutOfRangeException(nameof(tailTipOrientation)),
        RidleyExplosionPart.Wings or RidleyExplosionPart.Legs or RidleyExplosionPart.OpenHeadAndNeck or
            RidleyExplosionPart.Torso or RidleyExplosionPart.Claw =>
            throw new ArgumentOutOfRangeException(nameof(parameter), parameter, "Body fragments have no tail program."),
        _ => throw new InvalidOperationException($"Undefined Ridley explosion part {parameter}."),
    };

    /// <summary>
    /// Selects the body-relative position and pose loaded by
    /// <c>$A6:C7DA-$A6:C8D3</c>. Facing zero selects the native left-facing record.
    /// </summary>
    public static RidleyExplosionBodyPartDefinition SelectBodyPart(
        RidleyExplosionPart parameter,
        bool facingRight) => (parameter, facingRight) switch
    {
        (RidleyExplosionPart.Wings, false) => new(new(0, 0), RidleyBreakupFrame.WingsFacingLeft),
        (RidleyExplosionPart.Wings, true) => new(new(0, 0), RidleyBreakupFrame.WingsFacingRight),
        (RidleyExplosionPart.Legs, false) => new(new(15, 22), RidleyBreakupFrame.LegsFacingLeft),
        (RidleyExplosionPart.Legs, true) => new(new(-15, 22), RidleyBreakupFrame.LegsFacingRight),
        (RidleyExplosionPart.OpenHeadAndNeck, false) => new(new(-3, -24), RidleyBreakupFrame.OpenHeadAndNeckFacingLeft),
        (RidleyExplosionPart.OpenHeadAndNeck, true) => new(new(3, -24), RidleyBreakupFrame.OpenHeadAndNeckFacingRight),
        (RidleyExplosionPart.Torso, false) => new(new(16, 0), RidleyBreakupFrame.TorsoFacingLeft),
        (RidleyExplosionPart.Torso, true) => new(new(-16, 0), RidleyBreakupFrame.TorsoFacingRight),
        (RidleyExplosionPart.Claw, false) => new(new(8, 7), RidleyBreakupFrame.ClawFacingLeft),
        (RidleyExplosionPart.Claw, true) => new(new(-8, 7), RidleyBreakupFrame.ClawFacingRight),
        _ => throw new ArgumentOutOfRangeException(nameof(parameter)),
    };
}
