using System.Collections.Frozen;
using SuperMetroid.Core.Enemies;
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
    /// <summary>The part each authored spawn parameter of the breakup's embedded population records selects.</summary>
    internal static readonly ParameterVariants<RidleyExplosionPart> PartParameters = new(new Dictionary<int, RidleyExplosionPart>
    {
        [0] = RidleyExplosionPart.Tail0,
        [2] = RidleyExplosionPart.Tail1,
        [4] = RidleyExplosionPart.Tail2,
        [6] = RidleyExplosionPart.Tail3,
        [8] = RidleyExplosionPart.Tail4,
        [10] = RidleyExplosionPart.Tail5,
        [12] = RidleyExplosionPart.TailTip,
        [14] = RidleyExplosionPart.Wings,
        [16] = RidleyExplosionPart.Legs,
        [18] = RidleyExplosionPart.OpenHeadAndNeck,
        [20] = RidleyExplosionPart.Torso,
        [22] = RidleyExplosionPart.Claw,
    });

    /// <summary>
    /// The breakup's allocation order: the tail from tip to base, then wings, legs, torso, head and claw. Enemy-slot
    /// and draw order make this sequence observable.
    /// </summary>
    private static readonly RidleyExplosionPart[] BreakupOrder =
    [
        RidleyExplosionPart.TailTip, RidleyExplosionPart.Tail5, RidleyExplosionPart.Tail4, RidleyExplosionPart.Tail3,
        RidleyExplosionPart.Tail2, RidleyExplosionPart.Tail1, RidleyExplosionPart.Tail0, RidleyExplosionPart.Wings,
        RidleyExplosionPart.Legs, RidleyExplosionPart.Torso, RidleyExplosionPart.OpenHeadAndNeck, RidleyExplosionPart.Claw,
    ];

    /// <summary>
    /// Each part's lifetime in frames: tail segments expire eight frames apart from the base, the body parts follow
    /// the same eight-frame progression, and the torso expires last.
    /// </summary>
    private static readonly FrozenDictionary<RidleyExplosionPart, ushort> Lifetimes = new Dictionary<RidleyExplosionPart, ushort>
    {
        [RidleyExplosionPart.Tail0] = 72,
        [RidleyExplosionPart.Tail1] = 80,
        [RidleyExplosionPart.Tail2] = 88,
        [RidleyExplosionPart.Tail3] = 96,
        [RidleyExplosionPart.Tail4] = 104,
        [RidleyExplosionPart.Tail5] = 112,
        [RidleyExplosionPart.TailTip] = 120,
        [RidleyExplosionPart.Wings] = 40,
        [RidleyExplosionPart.Legs] = 48,
        [RidleyExplosionPart.OpenHeadAndNeck] = 56,
        [RidleyExplosionPart.Claw] = 64,
        [RidleyExplosionPart.Torso] = 128,
    }.ToFrozenDictionary();

    /// <summary>The tail segment each tail fragment starts from.</summary>
    internal static readonly FrozenDictionary<RidleyExplosionPart, int> TailSegment = new Dictionary<RidleyExplosionPart, int>
    {
        [RidleyExplosionPart.Tail0] = 0,
        [RidleyExplosionPart.Tail1] = 1,
        [RidleyExplosionPart.Tail2] = 2,
        [RidleyExplosionPart.Tail3] = 3,
        [RidleyExplosionPart.Tail4] = 4,
        [RidleyExplosionPart.Tail5] = 5,
        [RidleyExplosionPart.TailTip] = 6,
    }.ToFrozenDictionary();

    /// <summary>Spawns every part's authored parameter in the breakup's allocation order.</summary>
    internal static void SpawnInBreakupOrder(Action<ushort> spawn)
    {
        Ensure.NotNull(spawn);
        foreach (RidleyExplosionPart part in BreakupOrder)
            spawn(checked((ushort)PartParameters.Parameter(part)));
    }

    /// <summary>The definition of the part an authored spawn parameter selects.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The parameter selects no part.</exception>
    public static RidleyExplosionPartDefinition GetPart(ushort parameter) => new(Lifetimes[PartParameters.Decode(parameter)]);

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
        bool facingRight) => new RidleyExplosionPartFacing(parameter, facingRight) switch
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
