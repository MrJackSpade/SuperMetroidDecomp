using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a room block does beyond how it collides (#1551, #1722). The block's collision kind decides which reading applies: a
/// slope reads its <see cref="Slope"/>, an extension its <see cref="ExtensionOffset"/>, and each reacting kind its family's
/// reaction (<see cref="Shootable"/>, <see cref="Bombable"/>, <see cref="Special"/>,
/// <see cref="SpecialAir"/>, <see cref="Spike"/>, <see cref="Grapple"/>). Where a door block leads is its room's own
/// (<see cref="RoomLevelData.DoorExitAt"/>, #2090), not a reading of its behaviour.
/// </summary>
/// <remarks>
/// A block whose collision kind changes keeps its behaviour, and the new kind reads it through its own reading, so every
/// behaviour is one of the kept behaviours, each holding every reading. A behaviour is written as the reading it is meant
/// for, the catalog naming for each kind's every reading the behaviour the write keeps (<see cref="RoomBlockReadingDefinitions"/>),
/// and carries the others through <see cref="BlockParameterCarryOver{TBehavior, TReading}"/>. The default behaviour is
/// <see cref="None"/>.
/// <para>
/// A few behaviours mean what the room's area makes of them to special blocks and special air (#1805): the import resolves
/// each area's meaning into its own reaction, and such a behaviour is kept once unresolved, holding neither reading, and
/// once for each meaning the areas give it (<see cref="RoomBlockReadingDefinitions.InAreas"/>). A room holds its behaviours
/// as its area resolves them (<see cref="In"/>).
/// </para>
/// </remarks>
public readonly struct RoomBlockBehavior : IEquatable<RoomBlockBehavior>
{
    /// <summary>
    /// The kept behaviour each set of readings is, for <see cref="Holding"/>: every behaviour some write keeps and every
    /// area's resolution, each held once.
    /// </summary>
    private static readonly Dictionary<RoomBlockReadings, RoomBlockReadings> KeptByReadings = KeptBehaviors();

    /// <summary>The farthest an extension of any kept behaviour reaches backward, in blocks (or rows): a negative offset.</summary>
    private static int MostBackwardExtension { get; } = RoomBlockReadingDefinitions.Extension.Keys.Min();

    /// <summary>The farthest an extension of any kept behaviour reaches forward, in blocks (or rows).</summary>
    private static int MostForwardExtension { get; } = RoomBlockReadingDefinitions.Extension.Keys.Max();

    /// <summary>The unresolved behaviour each behaviour the areas resolve is, keyed by itself and by each of its resolutions.</summary>
    private static readonly Dictionary<RoomBlockReadings, RoomBlockReadings> UnresolvedOf = [];

    /// <summary>Each unresolved behaviour's resolution in each area.</summary>
    private static readonly Dictionary<AreaResolution, RoomBlockReadings> ResolvedIn = ResolutionsByArea();

    /// <summary>Writes behaviours as shootable reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, ShootableReaction> ShootableWrites =
        Writes(RoomBlockReadingDefinitions.Shootable);

    /// <summary>Writes behaviours as bombable reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, BombableReaction> BombableWrites =
        Writes(RoomBlockReadingDefinitions.Bombable);

    /// <summary>Writes behaviours as special-block reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, SpecialBlockReaction> SpecialWrites =
        Writes(RoomBlockReadingDefinitions.Special);

    /// <summary>Writes behaviours as special-air reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, SpecialAirReaction> SpecialAirWrites =
        Writes(RoomBlockReadingDefinitions.SpecialAir);

    /// <summary>Writes behaviours as spike reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, SpikeReaction> SpikeWrites =
        Writes(RoomBlockReadingDefinitions.Spike);

    /// <summary>Writes behaviours as grapple reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, GrappleReaction> GrappleWrites =
        Writes(RoomBlockReadingDefinitions.Grapple);

    /// <summary>Writes behaviours as slopes.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, SlopeGeometry> SlopeWrites =
        Writes(RoomBlockReadingDefinitions.Slope);

    /// <summary>Writes behaviours as extension offsets.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, int> ExtensionWrites =
        Writes(RoomBlockReadingDefinitions.Extension);

    /// <summary>
    /// Writes behaviours as one kind's readings: the behaviour the catalog names for each reading, held as the kept behaviour
    /// it is. A reading the catalog names no behaviour for is never written; a reacting kind's absent reaction is one.
    /// </summary>
    private static BlockParameterCarryOver<RoomBlockReadings, TReading> Writes<TReading>(IReadOnlyDictionary<TReading, RoomBlockReadings> written)
        where TReading : notnull =>
        new(written.ToDictionary(static write => write.Key, static write => KeptByReadings[write.Value]));

    /// <summary>Every kept behaviour, each once by its readings: every behaviour some write keeps and every area's resolution.</summary>
    private static Dictionary<RoomBlockReadings, RoomBlockReadings> KeptBehaviors()
    {
        IEnumerable<RoomBlockReadings> every =
        [
            .. RoomBlockReadingDefinitions.Shootable.Values, .. RoomBlockReadingDefinitions.Bombable.Values,
            .. RoomBlockReadingDefinitions.Special.Values, .. RoomBlockReadingDefinitions.SpecialAir.Values,
            .. RoomBlockReadingDefinitions.Spike.Values, .. RoomBlockReadingDefinitions.Grapple.Values,
            .. RoomBlockReadingDefinitions.Slope.Values, .. RoomBlockReadingDefinitions.Extension.Values,
            .. RoomBlockReadingDefinitions.InAreas.Select(static resolution => resolution.Readings),
        ];
        var kept = new Dictionary<RoomBlockReadings, RoomBlockReadings>();
        foreach (RoomBlockReadings readings in every)
            kept.TryAdd(readings, readings);
        return kept;
    }

    /// <summary>
    /// Indexes each resolution by its unresolved behaviour and area, and each resolution and unresolved behaviour by the
    /// unresolved one.
    /// </summary>
    /// <exception cref="InvalidOperationException">A resolution resolves no kept behaviour, or an area resolves one twice or not at all.</exception>
    private static Dictionary<AreaResolution, RoomBlockReadings> ResolutionsByArea()
    {
        var byArea = new Dictionary<AreaResolution, RoomBlockReadings>();
        foreach (AreaBlockReadings resolution in RoomBlockReadingDefinitions.InAreas)
        {
            RoomBlockReadings unresolved = KeptByReadings.TryGetValue(Unresolved(resolution.Readings), out RoomBlockReadings? kept)
                ? kept
                : throw new InvalidOperationException($"The area resolution {resolution.Readings} resolves no kept behaviour.");
            RoomBlockReadings resolved = KeptByReadings[resolution.Readings];
            UnresolvedOf[unresolved] = unresolved;
            UnresolvedOf.Add(resolved, unresolved);
            foreach (AreaId area in resolution.Areas)
                byArea.Add(new AreaResolution(unresolved, area), resolved);
        }
        foreach (RoomBlockReadings unresolved in UnresolvedOf.Values.Distinct())
        {
            if (AreaIds.All.Any(area => !byArea.ContainsKey(new AreaResolution(unresolved, area))))
                throw new InvalidOperationException($"An area leaves the behaviour {unresolved} unresolved.");
        }
        return byArea;
    }

    /// <summary>An unresolved behaviour in one area, which resolves it.</summary>
    /// <param name="Unresolved">The behaviour holding neither the special-block nor the special-air reading.</param>
    /// <param name="Area">The area resolving it.</param>
    private readonly record struct AreaResolution(RoomBlockReadings Unresolved, AreaId Area);

    /// <summary>A resolution's readings with the special-block and special-air readings its area gives left out.</summary>
    private static RoomBlockReadings Unresolved(RoomBlockReadings resolution) =>
        resolution with { Special = null, SpecialAir = null, ResolvedByArea = true };

    /// <summary>The empty behaviour's readings: the kept behaviour whose extension ends its chain.</summary>
    private static readonly RoomBlockReadings EmptyReadings = ExtensionWrites.Carrying(0);

    /// <summary>The kept behaviour; null holds <see cref="None"/>.</summary>
    private readonly RoomBlockReadings? kept;

    /// <summary>Holds one of the kept behaviours.</summary>
    private RoomBlockBehavior(RoomBlockReadings kept) => this.kept = kept;

    /// <summary>
    /// The empty behaviour: the one whose extension ends its chain, which every block past a room's grid starts with and
    /// which a block's cleared behaviour holds.
    /// </summary>
    public static RoomBlockBehavior None => new(EmptyReadings);

    /// <summary>The kept behaviour this one is.</summary>
    private RoomBlockReadings Readings => kept ?? EmptyReadings;

    /// <summary>The behaviour holding <paramref name="readings"/>, which must be one of the kept behaviours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No kept behaviour holds exactly these readings.</exception>
    public static RoomBlockBehavior Holding(RoomBlockReadings readings) =>
        KeptByReadings.TryGetValue(Ensure.NotNull(readings), out RoomBlockReadings? held)
            ? new RoomBlockBehavior(held)
            : throw new ArgumentOutOfRangeException(nameof(readings), readings, "No kept block behaviour holds these readings.");

    /// <summary>A shootable block's behaviour.</summary>
    /// <param name="reaction">The reaction.</param>
    public static RoomBlockBehavior Reacting(ShootableReaction reaction) => new(ShootableWrites.Carrying(reaction));

    /// <summary>A bombable block's behaviour.</summary>
    /// <param name="reaction">The reaction.</param>
    public static RoomBlockBehavior Reacting(BombableReaction reaction) => new(BombableWrites.Carrying(reaction));

    /// <summary>A special block's behaviour.</summary>
    /// <param name="reaction">The reaction.</param>
    public static RoomBlockBehavior Reacting(SpecialBlockReaction reaction) => new(SpecialWrites.Carrying(reaction));

    /// <summary>A special-air block's behaviour.</summary>
    /// <param name="reaction">The reaction.</param>
    public static RoomBlockBehavior Reacting(SpecialAirReaction reaction) => new(SpecialAirWrites.Carrying(reaction));

    /// <summary>A spike's behaviour.</summary>
    /// <param name="reaction">The reaction.</param>
    public static RoomBlockBehavior Reacting(SpikeReaction reaction) => new(SpikeWrites.Carrying(reaction));

    /// <summary>A grapple block's behaviour.</summary>
    /// <param name="reaction">The reaction.</param>
    public static RoomBlockBehavior Reacting(GrappleReaction reaction) => new(GrappleWrites.Carrying(reaction));

    /// <summary>An extension's behaviour.</summary>
    /// <param name="offset">The signed displacement, <see cref="MostBackwardExtension"/> through <see cref="MostForwardExtension"/>.</param>
    public static RoomBlockBehavior Extending(int offset) =>
        new(ExtensionWrites.Carrying(Ensure.BetweenInclusive(MostBackwardExtension, MostForwardExtension, offset)));

    /// <summary>A slope's behaviour.</summary>
    /// <param name="slope">The slope's shape and mirroring.</param>
    public static RoomBlockBehavior Sloping(SlopeGeometry slope) => new(SlopeWrites.Carrying(slope));

    /// <summary>
    /// This behaviour as a room of <paramref name="area"/> holds it (#1805): a behaviour with the special-block and
    /// special-air reactions it has in that area, and any other behaviour unchanged. The installation and the room
    /// objects' changes hold such a behaviour unresolved, and a room's load resolves it.
    /// </summary>
    /// <param name="area">The room's area, one of the game's areas.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The area is not one of the game's areas, or the behaviour holds what another area makes of it.
    /// </exception>
    public RoomBlockBehavior In(AreaId area)
    {
        _ = AreaIds.Require(area);
        RoomBlockReadings readings = Readings;
        if (!UnresolvedOf.TryGetValue(readings, out RoomBlockReadings? unresolved))
            return this;
        RoomBlockReadings resolved = ResolvedIn[new AreaResolution(unresolved, area)];
        return ReferenceEquals(readings, unresolved) || ReferenceEquals(readings, resolved)
            ? new RoomBlockBehavior(resolved)
            : throw new ArgumentOutOfRangeException(nameof(area), area, $"The behaviour ({this}) holds what another area makes of it.");
    }

    /// <summary>The reaction shootable blocks and shootable air read, or null when they have none for this behaviour.</summary>
    public ShootableReaction? Shootable => Readings.Shootable;

    /// <summary>The reaction bombable blocks and bombable air read, or null when they have none for this behaviour.</summary>
    public BombableReaction? Bombable => Readings.Bombable;

    /// <summary>The reaction special blocks read, or null when they have none for this behaviour.</summary>
    public SpecialBlockReaction? Special => Readings.Special;

    /// <summary>The reaction special-air blocks read, or null when they have none for this behaviour.</summary>
    public SpecialAirReaction? SpecialAir => Readings.SpecialAir;

    /// <summary>The reaction spike blocks and spike air read, or null when they have none for this behaviour.</summary>
    public SpikeReaction? Spike => Readings.Spike;

    /// <summary>The reaction grapple blocks read, or null when they have none for this behaviour.</summary>
    public GrappleReaction? Grapple => Readings.Grapple;

    /// <summary>A slope block's shape and mirroring.</summary>
    public SlopeGeometry Slope => Readings.Slope;

    /// <summary>
    /// The signed number of blocks (horizontal) or rows (vertical) from an extension to the block it collides as; zero
    /// ends the chain, and the extension then behaves as its kind's terminator.
    /// </summary>
    public int ExtensionOffset => Readings.ExtensionOffset;

    /// <summary>Whether an extension with this behaviour ends its chain instead of leading to another block.</summary>
    public bool EndsExtension => ExtensionOffset == 0;

    /// <summary>The reaction <paramref name="family"/>'s kinds read, or null when they have none for this behaviour.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="family"/> is undefined.</exception>
    public BlockReaction? ReactionIn(BlockReactionFamily family) => family switch
    {
        BlockReactionFamily.Shootable => Shootable is { } shootable ? BlockReaction.Of(shootable) : null,
        BlockReactionFamily.Bombable => Bombable is { } bombable ? BlockReaction.Of(bombable) : null,
        BlockReactionFamily.Special => Special is { } special ? BlockReaction.Of(special) : null,
        BlockReactionFamily.SpecialAir => SpecialAir is { } specialAir ? BlockReaction.Of(specialAir) : null,
        BlockReactionFamily.Spike => Spike is { } spike ? BlockReaction.Of(spike) : null,
        BlockReactionFamily.Grapple => Grapple is { } grapple ? BlockReaction.Of(grapple) : null,
        _ => throw new InvalidOperationException($"Undefined reaction family {family}."),
    };

    /// <summary>The reaction <paramref name="kind"/> reads, or null for a kind that reads no reaction or has none for this behaviour.</summary>
    public BlockReaction? ReactionUnder(RoomCollisionType kind) =>
        BlockReactionFamilies.Of(kind) is { } family ? ReactionIn(family) : null;

    /// <summary>The blue-door cap a shootable block's reaction selects, if it selects one.</summary>
    public bool TryGetBlueDoorOrientation(out ColoredDoorOrientation orientation) =>
        BlockReactionDefinitions.BlueDoorCaps.TryGetMeaning(Shootable, out orientation);

    /// <summary>
    /// The crumble block a special block's reaction selects, if it selects one. The same behaviours mean unrelated things to
    /// other collision kinds, so it is read only of a block established as special.
    /// </summary>
    public bool TryGetContactCrumble(out ContactCrumbleBlock crumble) =>
        BlockReactionDefinitions.ContactCrumbles.TryGetMeaning(Special, out crumble);

    /// <summary>The map or recharge station access a special block's reaction selects, if it selects one.</summary>
    public bool TryGetStationAccess(out StationAccess access) =>
        BlockReactionDefinitions.StationAccesses.TryGetMeaning(Special, out access);

    /// <summary>Whether a special block's reaction makes it a save station's floor.</summary>
    public bool IsSaveStationFloor => Special == SpecialBlockReaction.SaveStationFloor;

    /// <summary>
    /// Whether a special block's reaction makes it a station's access block or a save station's floor, whose touch room
    /// collision reports to the station.
    /// </summary>
    public bool IsStationTrigger => IsSaveStationFloor || TryGetStationAccess(out _);

    /// <summary>
    /// The downward-gate trigger a shootable block's reaction selects, if it selects one. The same behaviours mean
    /// unrelated things to other collision kinds, so it is read only of a block established as shootable.
    /// </summary>
    public bool TryGetDownwardGateTrigger(out DownwardGateTrigger trigger) =>
        BlockReactionDefinitions.DownwardGateTriggers.TryGetMeaning(Shootable, out trigger);

    /// <summary>Whether both are the same kept behaviour.</summary>
    public bool Equals(RoomBlockBehavior other) => ReferenceEquals(Readings, other.Readings);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RoomBlockBehavior other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Readings);

    /// <summary>Whether both are the same kept behaviour.</summary>
    public static bool operator ==(RoomBlockBehavior left, RoomBlockBehavior right) => left.Equals(right);

    /// <summary>Whether they are different kept behaviours.</summary>
    public static bool operator !=(RoomBlockBehavior left, RoomBlockBehavior right) => !left.Equals(right);

    /// <summary>A reaction's name, or "none" when the kind has no reaction for the behaviour.</summary>
    private static string Named<TReaction>(TReaction? reaction)
        where TReaction : struct, Enum => reaction?.ToString() ?? "none";

    /// <summary>Names every reading of the behaviour.</summary>
    public override string ToString()
    {
        RoomBlockReadings readings = Readings;
        return $"shootable {Named(readings.Shootable)}, bombable {Named(readings.Bombable)}, special {Named(readings.Special)}, " +
            $"special air {Named(readings.SpecialAir)}, spike {Named(readings.Spike)}, grapple {Named(readings.Grapple)}, " +
            $"slope {readings.Slope.Shape} {readings.Slope.Mirroring}, extension {readings.ExtensionOffset}";
    }
}
