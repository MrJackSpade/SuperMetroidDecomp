using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// The codec of the cartridge's block behaviour byte (#1551, #1722): the one place its layout and the dispatch tables'
/// numbering are known. The byte stored beside each level word means what the block's collision kind makes of it, so the
/// import decodes every reading at once into one of the kept <see cref="RoomBlockBehavior"/>s:
/// <list type="bullet">
/// <item>a reaction: bit seven selects the area's table over the common one, and the low seven bits are the entry, which
/// each reacting kind's dispatch tables give their own meaning; an entry past a kind's tables is no reaction of that kind,
/// which no retail block of the kind holds (<see cref="HoldsReaction"/>, #1742);</item>
/// <item>a slope: the low five bits are the shape, bit six mirrors it left to right and bit seven top to bottom;</item>
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

    /// <summary>The byte each kept behaviour's first holder is, for <see cref="Encode"/>.</summary>
    private static readonly Lazy<Dictionary<RoomBlockBehavior, int>> Bytes = new(() =>
        Enumerable.Range(0, ByteCount).ToDictionary(Decode, value => value));

    /// <summary>Decodes a behaviour byte into the kept behaviour holding every reading the block's collision kinds take of it.</summary>
    /// <param name="value">The byte, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a byte, or no kept behaviour holds its readings.</exception>
    public static RoomBlockBehavior Decode(int value) => RoomBlockBehavior.Holding(ReadingsOf(value));

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

    /// <summary>Decodes a behaviour byte into every reading the block's collision kinds take of it.</summary>
    /// <param name="value">The byte, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a byte.</exception>
    public static RoomBlockReadings ReadingsOf(int value)
    {
        Ensure.BetweenInclusive(0, byte.MaxValue, value);
        bool area = (value & HighBit) != 0;
        int entry = value & EntryMask;
        BlockMirroring mirroring = ((value & SlopeHorizontalBit) != 0 ? BlockMirroring.Horizontal : BlockMirroring.None) |
            (area ? BlockMirroring.Vertical : BlockMirroring.None);
        return new RoomBlockReadings(
            ShootableOf(area, entry), BombableOf(area, entry), SpecialOf(area, entry), SpecialAirOf(area, entry),
            SpikeOf(area, entry), GrappleOf(area, entry), new SlopeGeometry(value & SlopeShapeMask, mirroring),
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
    /// The special-block dispatchers' entry for a special block, or none past their tables. The bombed-special table's
    /// common entries run on past the reactions every area shares into the area tables, so entries 26 through 29 read
    /// Brinstar's Speed Booster blocks (<see cref="SpecialBlockReaction.BombRevealsSpeedBlock"/>).
    /// </summary>
    private static SpecialBlockReaction? SpecialOf(bool area, int entry) => new TableEntry(area, entry) switch
    {
        (true, 0) => SpecialBlockReaction.WreckedShipChozoHand,
        (true, 2) => SpecialBlockReaction.BrinstarSlowRespawningSpeedBlock,
        (true, 3) => SpecialBlockReaction.BrinstarSlowPermanentSpeedBlockOrNorfairChozoHand,
        (true, 4) => SpecialBlockReaction.BrinstarRespawningSpeedBlock,
        (true, 5) => SpecialBlockReaction.BrinstarPermanentSpeedBlock,
        (true, < 8) => SpecialBlockReaction.SolidInEveryArea,
        (true, _) => null,
        (false, 0) => SpecialBlockReaction.RespawningCrumble1x1,
        (false, 1) => SpecialBlockReaction.RespawningCrumble2x1,
        (false, 2) => SpecialBlockReaction.RespawningCrumble1x2,
        (false, 3) => SpecialBlockReaction.RespawningCrumble2x2,
        (false, 4) => SpecialBlockReaction.PermanentCrumble1x1,
        (false, 5) => SpecialBlockReaction.PermanentCrumble2x1,
        (false, 6) => SpecialBlockReaction.PermanentCrumble1x2,
        (false, 7) => SpecialBlockReaction.PermanentCrumble2x2,
        (false, < 14) => SpecialBlockReaction.XrayRevealedSolid,
        (false, 14) => SpecialBlockReaction.RespawningSpeedBlock,
        (false, 15) => SpecialBlockReaction.PermanentSpeedBlock,
        (false, >= 26 and <= 29) => SpecialBlockReaction.BombRevealsSpeedBlock,
        (false, 69) => SpecialBlockReaction.CollectibleTrigger,
        (false, 71) => SpecialBlockReaction.MapStationRight,
        (false, 72) => SpecialBlockReaction.MapStationLeft,
        (false, 73) => SpecialBlockReaction.EnergyStationRight,
        (false, 74) => SpecialBlockReaction.EnergyStationLeft,
        (false, 75) => SpecialBlockReaction.MissileStationRight,
        (false, 76) => SpecialBlockReaction.MissileStationLeft,
        (false, 77) => SpecialBlockReaction.SaveStationFloor,
        (false, < 80) => SpecialBlockReaction.Solid,
        (false, _) => null,
    };

    /// <summary>The special-air dispatchers' entry for a special-air block, or none past the area tables.</summary>
    private static SpecialAirReaction? SpecialAirOf(bool area, int entry) => new TableEntry(area, entry) switch
    {
        (true, 0) => SpecialAirReaction.IceFloorPlantChozoHandOrQuicksandSurface,
        (true, 1) => SpecialAirReaction.CeilingPlantOrQuicksandSurface,
        (true, 2) => SpecialAirReaction.SlowRespawningSpeedBlockOrQuicksandSurface,
        (true, 3) => SpecialAirReaction.SlowPermanentSpeedBlockChozoHandOrSubmergingQuicksand,
        (true, 4) => SpecialAirReaction.RespawningSpeedBlockOrSlowSandfall,
        (true, 5) => SpecialAirReaction.PermanentSpeedBlockOrFastSandfall,
        (true, < 16) => SpecialAirReaction.PassesThroughInEveryArea,
        (true, _) => null,
        (false, 8) => SpecialAirReaction.RightwardConveyorOnFoot,
        (false, 9) => SpecialAirReaction.LeftwardConveyorOnFoot,
        (false, 10) => SpecialAirReaction.RightwardConveyor,
        (false, 11) => SpecialAirReaction.LeftwardConveyor,
        (false, 70) => SpecialAirReaction.ScrollTrigger,
        (false, _) => SpecialAirReaction.PassesThrough,
    };

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
