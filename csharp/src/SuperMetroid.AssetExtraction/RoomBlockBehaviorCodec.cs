using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The codec of the cartridge's block behaviour byte (#1551, #1722): the one place its layout and the dispatch tables'
/// numbering are known. The byte stored beside each level word means what the block's collision kind makes of it, so the
/// import decodes every reading at once into one of the kept <see cref="RoomBlockBehavior"/>s:
/// <list type="bullet">
/// <item>a reaction: bit seven selects the area's table over the common one, and the low seven bits are the entry, which
/// each reacting kind's dispatch tables give their own meaning; an entry past a kind's tables is no reaction of that kind,
/// which no retail block of the kind holds (<see cref="HoldsReaction"/>, #1742). The special-block and special-air area
/// tables give an entry what the room's area makes of it, which the codec resolves into that meaning's own reaction
/// (#1805): <see cref="ReadingsOf(AreaId, int)"/> reads a byte in an area, and <see cref="ReadingsOf(int)"/> leaves both
/// readings out of a byte whose meaning the areas give differently;</item>
/// <item>a slope: the low five bits number the shape (<see cref="SlopeShapeOf"/>), bit six mirrors it left to right and bit
/// seven top to bottom;</item>
/// <item>an extension: the whole byte, two's complement, is the signed offset;</item>
/// <item>a door: the low seven bits are the door-list entry.</item>
/// </list>
/// Only the import, the generator of the kept behaviours and checks that compare the game's own bytes convert through it:
/// the installation holds each block's behaviour as the reading its collision kind takes, and the X-ray reveals' rules by
/// their reactions' names (#1709, #1722), so nothing the game loads holds the byte.
/// </summary>
[EncodingOwner("room block behaviour byte")]
public static class RoomBlockBehaviorCodec
{
    /// <summary>The number of behaviour bytes.</summary>
    public const int ByteCount = 256;

    /// <summary>The bit selecting the area's reactions, a slope's vertical mirroring, and an extension's sign.</summary>
    private const int HighBit = 0x80;

    /// <summary>The bit mirroring a slope left to right.</summary>
    private const int SlopeHorizontalBit = 0x40;

    /// <summary>The bits naming a reaction's entry or a door's link.</summary>
    private const int EntryMask = 0x7F;

    /// <summary>The bits naming a slope's shape.</summary>
    private const int SlopeShapeMask = 0x1F;

    /// <summary>The slope shape each value of a slope's shape bits numbers, in that order.</summary>
    private static readonly IReadOnlyList<SlopeShape> SlopeShapes =
    [
        SlopeShapeDefinitions.SquareBottomHalf, SlopeShapeDefinitions.SquareRightHalf, SlopeShapeDefinitions.SquareBottomRightQuarter,
        SlopeShapeDefinitions.SquareAllButTopLeft, SlopeShapeDefinitions.SquareFull, SlopeShapeDefinitions.LowPeak,
        SlopeShapeDefinitions.HighPeak, SlopeShapeDefinitions.HalfHeight, SlopeShapeDefinitions.UnusedSolid1,
        SlopeShapeDefinitions.UnusedSolid2, SlopeShapeDefinitions.UnusedSolid3, SlopeShapeDefinitions.UnusedSolid4,
        SlopeShapeDefinitions.UnusedSolid5, SlopeShapeDefinitions.UnusedSolid6, SlopeShapeDefinitions.FourPixelStairs,
        SlopeShapeDefinitions.TwoPixelStairs, SlopeShapeDefinitions.Open, SlopeShapeDefinitions.Overhang,
        SlopeShapeDefinitions.UnitRise, SlopeShapeDefinitions.Solid, SlopeShapeDefinitions.UnitRiseLower,
        SlopeShapeDefinitions.UnitRiseUpper, SlopeShapeDefinitions.HalfRiseLower, SlopeShapeDefinitions.HalfRiseUpper,
        SlopeShapeDefinitions.ThirdRiseLower, SlopeShapeDefinitions.ThirdRiseMiddle, SlopeShapeDefinitions.ThirdRiseUpper,
        SlopeShapeDefinitions.DoubleRiseLower, SlopeShapeDefinitions.DoubleRiseUpper, SlopeShapeDefinitions.TripleRiseLower,
        SlopeShapeDefinitions.TripleRiseMiddle, SlopeShapeDefinitions.TripleRiseUpper,
    ];

    /// <summary>Each byte's behaviour in any area, decoded once.</summary>
    private static readonly Lazy<RoomBlockBehavior[]> AnyArea = new(() =>
        [.. Enumerable.Range(0, ByteCount).Select(value => RoomBlockBehavior.Holding(ReadingsOf(value)))]);

    /// <summary>Each byte's behaviour in each area, decoded once.</summary>
    private static readonly Lazy<Dictionary<AreaId, RoomBlockBehavior[]>> InAreas = new(() => AreaIds.All.ToDictionary(
        area => area, area => (RoomBlockBehavior[])[.. Enumerable.Range(0, ByteCount).Select(value => RoomBlockBehavior.Holding(ReadingsOf(area, value)))]));

    /// <summary>
    /// The byte each kept behaviour's first holder is, for <see cref="Encode"/>: each byte's unresolved behaviour and its
    /// resolution in every area.
    /// </summary>
    private static readonly Lazy<Dictionary<RoomBlockBehavior, int>> Bytes = new(() =>
    {
        var bytes = new Dictionary<RoomBlockBehavior, int>();
        for (int value = 0; value < ByteCount; value++)
        {
            bytes.Add(Decode(value), value);
            foreach (AreaId area in AreaIds.All)
                bytes.TryAdd(Decode(area, value), value);
        }
        return bytes;
    });

    /// <summary>
    /// Decodes a behaviour byte into the kept behaviour holding every reading the block's collision kinds take of it in any
    /// area: a byte whose meaning to special blocks and special air the areas give differently holds neither reading
    /// (<see cref="RoomBlockBehavior.In"/> resolves it in a room).
    /// </summary>
    /// <param name="value">The byte, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a byte, or no kept behaviour holds its readings.</exception>
    public static RoomBlockBehavior Decode(int value) => AnyArea.Value[Ensure.BetweenInclusive(0, byte.MaxValue, value)];

    /// <summary>Decodes a behaviour byte into the kept behaviour holding every reading the block's collision kinds take of it in <paramref name="area"/>.</summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <param name="value">The byte, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not a retail area, <paramref name="value"/> is not a byte, or no kept behaviour holds its readings.</exception>
    public static RoomBlockBehavior Decode(AreaId area, int value) =>
        InAreas.Value[AreaIds.Require(area)][Ensure.BetweenInclusive(0, byte.MaxValue, value)];

    /// <summary>The slope shape a slope's shape bits number.</summary>
    /// <param name="number">The shape bits, 0 through 31.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> numbers no slope shape.</exception>
    public static SlopeShape SlopeShapeOf(int number) => SlopeShapes[Ensure.BetweenInclusive(0, SlopeShapeMask, number)];

    /// <summary>Encodes a behaviour as its byte.</summary>
    /// <param name="behavior">A decoded behaviour, or one a room object wrote.</param>
    public static int Encode(RoomBlockBehavior behavior) => Bytes.Value[behavior];

    /// <summary>
    /// Whether a block of collision kind <paramref name="kind"/> with <paramref name="behavior"/> has a reaction its kind
    /// reads: true for a kind that reads none. The import refuses a level block without one (#1742): its kind's dispatch
    /// would read past the end of its tables, and no retail block does.
    /// </summary>
    public static bool HoldsReaction(RoomCollisionType kind, RoomBlockBehavior behavior) =>
        BlockReactionFamilies.Of(kind) is not { } family || behavior.ReactionIn(family) is not null;

    /// <summary>
    /// Decodes a behaviour byte into every reading the block's collision kinds take of it in any area: when the areas make
    /// different things of it to special blocks and special air, it holds neither of those readings.
    /// </summary>
    /// <param name="value">The byte, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a byte.</exception>
    public static RoomBlockReadings ReadingsOf(int value)
    {
        List<RoomBlockReadings> inAreas = [.. AreaIds.All.Select(area => ReadingsOf(area, value)).Distinct()];
        return inAreas.Count == 1 ? inAreas[0] : inAreas[0] with { Special = null, SpecialAir = null };
    }

    /// <summary>Decodes a behaviour byte into every reading the block's collision kinds take of it in <paramref name="area"/>.</summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <param name="value">The byte, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not a retail area, or <paramref name="value"/> is not a byte.</exception>
    public static RoomBlockReadings ReadingsOf(AreaId area, int value)
    {
        _ = AreaIds.Require(area);
        _ = Ensure.BetweenInclusive(0, byte.MaxValue, value);
        bool areaTable = (value & HighBit) != 0;
        int entry = value & EntryMask;
        BlockMirroring mirroring = ((value & SlopeHorizontalBit) != 0 ? BlockMirroring.Horizontal : BlockMirroring.None) |
            (areaTable ? BlockMirroring.Vertical : BlockMirroring.None);
        return new RoomBlockReadings(
            ShootableOf(areaTable, entry), BombableOf(areaTable, entry),
            areaTable ? AreaSpecialOf(area, entry) : SpecialOf(entry),
            areaTable ? AreaSpecialAirOf(area, entry) : SpecialAirOf(entry),
            SpikeOf(areaTable, entry), GrappleOf(areaTable, entry), new SlopeGeometry(SlopeShapeOf(value & SlopeShapeMask), mirroring),
            unchecked((sbyte)value), entry);
    }

    /// <summary>The shot, bomb and grapple dispatchers' table entry for a shootable block, or none past their tables.</summary>
    private static ShootableReaction? ShootableOf(bool area, int entry) => new TableEntry(area, entry) switch
    {
        (true, < 8) => ShootableReaction.NothingWhenSolid,
        (true, _) => null,
        (false, 0) => ShootableReaction.RespawningBreak1x1,
        (false, 1) => ShootableReaction.RespawningBreak2x1,
        (false, 2) => ShootableReaction.RespawningBreak1x2,
        (false, 3) => ShootableReaction.RespawningBreak2x2,
        (false, 4) => ShootableReaction.PermanentBreak1x1,
        (false, 5) => ShootableReaction.PermanentBreak2x1,
        (false, 6) => ShootableReaction.PermanentBreak1x2,
        (false, 7) => ShootableReaction.PermanentBreak2x2,
        (false, 8) => ShootableReaction.RespawningPowerBombBlock,
        (false, 9) => ShootableReaction.PermanentPowerBombBlock,
        (false, 10) => ShootableReaction.RespawningSuperMissileBlock,
        (false, 11) => ShootableReaction.PermanentSuperMissileBlock,
        (false, < 16) => ShootableReaction.Nothing,
        (false, 16) => ShootableReaction.CollisionProbe,
        (false, < 64) => ShootableReaction.IgnoresShots,
        (false, 64) => ShootableReaction.BlueDoorCapLeft,
        (false, 65) => ShootableReaction.BlueDoorCapRight,
        (false, 66) => ShootableReaction.BlueDoorCapUp,
        (false, 67) => ShootableReaction.BlueDoorCapDown,
        (false, 68) => ShootableReaction.ResidentObjectTrigger,
        (false, 69) => ShootableReaction.CollectibleTrigger,
        (false, 70) => ShootableReaction.GateTriggerBlueLeft,
        (false, 71) => ShootableReaction.GateTriggerBlueRight,
        (false, 72) => ShootableReaction.GateTriggerRedLeft,
        (false, 73) => ShootableReaction.GateTriggerRedRight,
        (false, 74) => ShootableReaction.GateTriggerGreenLeft,
        (false, 75) => ShootableReaction.GateTriggerGreenRight,
        (false, 76) => ShootableReaction.GateTriggerYellowLeft,
        (false, 77) => ShootableReaction.GateTriggerYellowRight,
        (false, 78) => ShootableReaction.IgnoresShots,
        (false, 79) => ShootableReaction.AnimalEscape,
        (false, _) => null,
    };

    /// <summary>The bomb-block table's entry for a bombable block, or none past the table.</summary>
    private static BombableReaction? BombableOf(bool area, int entry) => new TableEntry(area, entry) switch
    {
        (true, _) => BombableReaction.Inert,
        (false, 0) => BombableReaction.RespawningBreak1x1,
        (false, 1) => BombableReaction.RespawningBreak2x1,
        (false, 2) => BombableReaction.RespawningBreak1x2,
        (false, 3) => BombableReaction.RespawningBreak2x2,
        (false, 4) => BombableReaction.PermanentBreak1x1,
        (false, 5) => BombableReaction.PermanentBreak2x1,
        (false, 6) => BombableReaction.PermanentBreak1x2,
        (false, 7) => BombableReaction.PermanentBreak2x2,
        (false, < 16) => BombableReaction.Nothing,
        (false, _) => null,
    };

    /// <summary>
    /// The special-block dispatchers' common entry for a special block, or none past their tables. The bombed-special
    /// table's common entries run on past the reactions every area shares into the area tables, so entries 26 through 29
    /// read Brinstar's Speed Booster blocks (<see cref="SpecialBlockReaction.BombRevealsSpeedBlock"/>).
    /// </summary>
    private static SpecialBlockReaction? SpecialOf(int entry) => entry switch
    {
        0 => SpecialBlockReaction.RespawningCrumble1x1,
        1 => SpecialBlockReaction.RespawningCrumble2x1,
        2 => SpecialBlockReaction.RespawningCrumble1x2,
        3 => SpecialBlockReaction.RespawningCrumble2x2,
        4 => SpecialBlockReaction.PermanentCrumble1x1,
        5 => SpecialBlockReaction.PermanentCrumble2x1,
        6 => SpecialBlockReaction.PermanentCrumble1x2,
        7 => SpecialBlockReaction.PermanentCrumble2x2,
        < 14 => SpecialBlockReaction.XrayRevealedSolid,
        14 => SpecialBlockReaction.RespawningSpeedBlock,
        15 => SpecialBlockReaction.PermanentSpeedBlock,
        >= 26 and <= 29 => SpecialBlockReaction.BombRevealsSpeedBlock,
        69 => SpecialBlockReaction.CollectibleTrigger,
        71 => SpecialBlockReaction.MapStationRight,
        72 => SpecialBlockReaction.MapStationLeft,
        73 => SpecialBlockReaction.EnergyStationRight,
        74 => SpecialBlockReaction.EnergyStationLeft,
        75 => SpecialBlockReaction.MissileStationRight,
        76 => SpecialBlockReaction.MissileStationLeft,
        77 => SpecialBlockReaction.SaveStationFloor,
        < 80 => SpecialBlockReaction.Solid,
        _ => null,
    };

    /// <summary>
    /// What <paramref name="area"/>'s special-block table makes of its entry, or none past the table: Wrecked Ship's and Lower
    /// Norfair's Chozo hands, Brinstar's own Speed Booster blocks, and a plain solid block for every other entry.
    /// </summary>
    private static SpecialBlockReaction? AreaSpecialOf(AreaId area, int entry) => new AreaEntry(area, entry) switch
    {
        (AreaId.WreckedShip, 0) => SpecialBlockReaction.WreckedShipChozoHand,
        (AreaId.Brinstar, 2) => SpecialBlockReaction.SlowRespawningSpeedBlock,
        (AreaId.Brinstar, 3) => SpecialBlockReaction.SlowPermanentSpeedBlock,
        (AreaId.Norfair, 3) => SpecialBlockReaction.LowerNorfairChozoHand,
        (AreaId.Brinstar, 4) => SpecialBlockReaction.BrinstarRespawningSpeedBlock,
        (AreaId.Brinstar, 5) => SpecialBlockReaction.BrinstarPermanentSpeedBlock,
        (_, < 8) => SpecialBlockReaction.Solid,
        _ => null,
    };

    /// <summary>The special-air dispatchers' common entry for a special-air block.</summary>
    private static SpecialAirReaction SpecialAirOf(int entry) => entry switch
    {
        8 => SpecialAirReaction.RightwardConveyorOnFoot,
        9 => SpecialAirReaction.LeftwardConveyorOnFoot,
        10 => SpecialAirReaction.RightwardConveyor,
        11 => SpecialAirReaction.LeftwardConveyor,
        70 => SpecialAirReaction.ScrollTrigger,
        _ => SpecialAirReaction.PassesThrough,
    };

    /// <summary>
    /// What <paramref name="area"/>'s special-air table makes of its entry, or none past the table: Crateria's ice,
    /// Brinstar's plants and Speed Booster blocks, the Chozo hands' triggers, Maridia's quicksand and sandfalls, and passing
    /// through for every other entry.
    /// </summary>
    private static SpecialAirReaction? AreaSpecialAirOf(AreaId area, int entry) => new AreaEntry(area, entry) switch
    {
        (AreaId.Crateria, 0) => SpecialAirReaction.Ice,
        (AreaId.Brinstar, 0) => SpecialAirReaction.FloorPlant,
        (AreaId.Brinstar, 1) => SpecialAirReaction.CeilingPlant,
        (AreaId.Brinstar, 2) => SpecialAirReaction.SlowRespawningSpeedBlock,
        (AreaId.Brinstar, 3) => SpecialAirReaction.SlowPermanentSpeedBlock,
        (AreaId.Brinstar, 4) => SpecialAirReaction.DachoraRespawningSpeedBlock,
        (AreaId.Brinstar, 5) => SpecialAirReaction.PermanentSpeedBlock,
        (AreaId.Norfair, 3) => SpecialAirReaction.LowerNorfairChozoHand,
        (AreaId.WreckedShip, 0) => SpecialAirReaction.WreckedShipChozoHand,
        (AreaId.Maridia, 0 or 1 or 2) => SpecialAirReaction.QuicksandSurface,
        (AreaId.Maridia, 3) => SpecialAirReaction.SubmergingQuicksand,
        (AreaId.Maridia, 4) => SpecialAirReaction.SlowSandfall,
        (AreaId.Maridia, 5) => SpecialAirReaction.FastSandfall,
        (_, < 16) => SpecialAirReaction.PassesThrough,
        _ => null,
    };

    /// <summary>One entry of an area's own dispatch table.</summary>
    /// <param name="Area">The room's area.</param>
    /// <param name="Index">The entry, 0 through 127.</param>
    private readonly record struct AreaEntry(AreaId Area, int Index);

    /// <summary>
    /// The spike dispatchers' entry for a spike block or spike air, or none past the enemy's spike table, which reads both
    /// lists alike.
    /// </summary>
    private static SpikeReaction? SpikeOf(bool area, int entry) => new TableEntry(area, entry) switch
    {
        (true, < 15) => SpikeReaction.HarmlessLetsGrappleThrough,
        (true, 15) => SpikeReaction.EnemyBreakableLetsGrappleThrough,
        (true, _) => null,
        (false, 0) => SpikeReaction.HeavySpike,
        (false, 1) => SpikeReaction.LightSpike,
        (false, 2) => SpikeReaction.DamagingSpikeAir,
        (false, 3) => SpikeReaction.TurretSpike,
        (false, 14) => SpikeReaction.XrayRevealed,
        (false, 15) => SpikeReaction.EnemyBreakable,
        (false, < 16) => SpikeReaction.Harmless,
        (false, _) => null,
    };

    /// <summary>One entry of a reacting kind's dispatch tables: the common or the area's table, and the entry in it.</summary>
    /// <param name="Area">Whether the entry is of the area's table.</param>
    /// <param name="Index">The entry, 0 through 127.</param>
    private readonly record struct TableEntry(bool Area, int Index);

    /// <summary>The grapple dispatcher's entry for a grapple block, or none past its table.</summary>
    private static GrappleReaction? GrappleOf(bool area, int entry) => new TableEntry(area, entry) switch
    {
        (true, _) => GrappleReaction.Rejected,
        (false, 0) => GrappleReaction.Anchor,
        (false, 1) => GrappleReaction.RespawningBreakableAnchor,
        (false, 2) => GrappleReaction.PermanentBreakableAnchor,
        (false, 3) => GrappleReaction.UnrevealedAnchor,
        (false, _) => null,
    };
}
