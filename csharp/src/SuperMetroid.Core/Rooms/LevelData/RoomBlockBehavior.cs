using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a room block does beyond how it collides (#1551, #1722). The block's collision kind decides which reading applies: a
/// slope reads its <see cref="Slope"/>, an extension its <see cref="ExtensionOffset"/>, a door its <see cref="DoorLink"/>,
/// and each reacting kind its family's reaction (<see cref="Shootable"/>, <see cref="Bombable"/>, <see cref="Special"/>,
/// <see cref="SpecialAir"/>, <see cref="Spike"/>, <see cref="Grapple"/>).
/// </summary>
/// <remarks>
/// A block whose collision kind changes keeps its behaviour, and the new kind reads it through its own reading, so every
/// behaviour is one of the kept behaviours, each holding every reading (<see cref="RoomBlockReadingDefinitions"/>). A
/// behaviour is written as the reading it is meant for and carries the others through
/// <see cref="BlockParameterCarryOver{TBehavior, TReading}"/>. The default behaviour is <see cref="None"/>.
/// <para>
/// A few behaviours mean what the room's area makes of them to special blocks and special air (#1805): the import resolves
/// each area's meaning into its own reaction, and such a behaviour is kept once unresolved, holding neither reading, and
/// once for each meaning the areas give it (<see cref="RoomBlockReadingDefinitions.InAreas"/>). A room holds its behaviours
/// as its area resolves them (<see cref="In"/>).
/// </para>
/// </remarks>
public readonly struct RoomBlockBehavior : IEquatable<RoomBlockBehavior>
{
    /// <summary>The farthest an extension reaches backward: 128 blocks (or rows).</summary>
    public const int MostBackwardExtension = -128;

    /// <summary>The farthest an extension reaches forward: 127 blocks (or rows).</summary>
    public const int MostForwardExtension = 127;

    /// <summary>The last entry of a room's door list a door block selects.</summary>
    public const int LastDoorLink = 127;

    /// <summary>
    /// Every kept behaviour in the order the game numbers them, each one the areas resolve followed by its resolutions in the
    /// game's area order, so a write of a reading keeps the unresolved behaviour before any area's.
    /// </summary>
    private static readonly IReadOnlyList<RoomBlockReadings> Kept = KeptInOrder();

    /// <summary>The kept behaviour each set of readings is, for <see cref="Holding"/>.</summary>
    private static readonly Dictionary<RoomBlockReadings, RoomBlockReadings> KeptByReadings =
        Kept.ToDictionary(readings => readings);

    /// <summary>The unresolved behaviour each behaviour the areas resolve is, keyed by itself and by each of its resolutions.</summary>
    private static readonly Dictionary<RoomBlockReadings, RoomBlockReadings> UnresolvedOf = [];

    /// <summary>Each unresolved behaviour's resolution in each area.</summary>
    private static readonly Dictionary<AreaResolution, RoomBlockReadings> ResolvedIn = ResolutionsByArea();

    /// <summary>Writes behaviours as shootable reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, ShootableReaction> ShootableWrites =
        ReactionWrites(static readings => readings.Shootable);

    /// <summary>Writes behaviours as bombable reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, BombableReaction> BombableWrites =
        ReactionWrites(static readings => readings.Bombable);

    /// <summary>Writes behaviours as special-block reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, SpecialBlockReaction> SpecialWrites =
        ReactionWrites(static readings => readings.Special);

    /// <summary>Writes behaviours as special-air reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, SpecialAirReaction> SpecialAirWrites =
        ReactionWrites(static readings => readings.SpecialAir);

    /// <summary>Writes behaviours as spike reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, SpikeReaction> SpikeWrites =
        ReactionWrites(static readings => readings.Spike);

    /// <summary>Writes behaviours as grapple reactions.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, GrappleReaction> GrappleWrites =
        ReactionWrites(static readings => readings.Grapple);

    /// <summary>Writes behaviours as slopes.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, SlopeGeometry> SlopeWrites =
        new(Kept, static readings => readings.Slope);

    /// <summary>Writes behaviours as extension offsets.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, int> ExtensionWrites =
        new(Kept, static readings => readings.ExtensionOffset);

    /// <summary>Writes behaviours as door links.</summary>
    private static readonly BlockParameterCarryOver<RoomBlockReadings, int> DoorWrites =
        new(Kept, static readings => readings.DoorLink);

    /// <summary>
    /// Writes behaviours as one reacting kind's reactions: the kept behaviours that give the kind a reaction, read as it. A
    /// behaviour giving the kind none is never written as its reaction.
    /// </summary>
    private static BlockParameterCarryOver<RoomBlockReadings, TReaction> ReactionWrites<TReaction>(Func<RoomBlockReadings, TReaction?> read)
        where TReaction : struct, Enum =>
        new(Kept.Where(readings => read(readings) is not null),
            readings => read(readings) ?? throw new InvalidOperationException("A kept behaviour lost its reaction."));

    /// <summary>The kept behaviours in order: each of the catalog's, followed by its resolutions in the areas.</summary>
    private static List<RoomBlockReadings> KeptInOrder()
    {
        ILookup<RoomBlockReadings, RoomBlockReadings> resolutions = RoomBlockReadingDefinitions.InAreas.ToLookup(
            static resolution => Unresolved(resolution.Readings), static resolution => resolution.Readings);
        return [.. RoomBlockReadingDefinitions.Kept.SelectMany(kept => resolutions[kept].Prepend(kept))];
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
            UnresolvedOf[unresolved] = unresolved;
            UnresolvedOf.Add(resolution.Readings, unresolved);
            foreach (AreaId area in resolution.Areas)
                byArea.Add(new AreaResolution(unresolved, area), resolution.Readings);
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
        resolution with { Special = null, SpecialAir = null };

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

    /// <summary>The behaviour of a block whose family's reaction is <paramref name="reaction"/>, of any family.</summary>
    /// <param name="reaction">A reaction of one of the families (<see cref="BlockReactionFamily"/>).</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is no family's reaction.</exception>
    public static RoomBlockBehavior ReactingAs(Enum reaction) => Ensure.NotNull(reaction) switch
    {
        ShootableReaction shootable => Reacting(shootable),
        BombableReaction bombable => Reacting(bombable),
        SpecialBlockReaction special => Reacting(special),
        SpecialAirReaction specialAir => Reacting(specialAir),
        SpikeReaction spike => Reacting(spike),
        GrappleReaction grapple => Reacting(grapple),
        _ => throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "Not a block reaction."),
    };

    /// <summary>An extension's behaviour.</summary>
    /// <param name="offset">The signed displacement, <see cref="MostBackwardExtension"/> through <see cref="MostForwardExtension"/>.</param>
    public static RoomBlockBehavior Extending(int offset) =>
        new(ExtensionWrites.Carrying(Ensure.BetweenInclusive(MostBackwardExtension, MostForwardExtension, offset)));

    /// <summary>A slope's behaviour.</summary>
    /// <param name="slope">The slope's shape and mirroring.</param>
    public static RoomBlockBehavior Sloping(SlopeGeometry slope) => new(SlopeWrites.Carrying(slope));

    /// <summary>A door block's behaviour.</summary>
    /// <param name="doorLink">The entry of the room's door list the door selects, 0 through <see cref="LastDoorLink"/>.</param>
    public static RoomBlockBehavior LinkingDoor(int doorLink) =>
        new(DoorWrites.Carrying(Ensure.BetweenInclusive(0, LastDoorLink, doorLink)));

    /// <summary>
    /// This behaviour as a room of <paramref name="area"/> holds it (#1805): a behaviour whose special-block and special-air
    /// readings the area's own tables give, as the area resolves it, and any other behaviour unchanged. The installation and
    /// the room objects' writes hold such a behaviour unresolved, and a room's load resolves it.
    /// </summary>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The area is not a retail area, or the behaviour holds what another area makes of it.
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

    /// <summary>
    /// The entry of the room's door list a door block selects. Elevator and special-door blocks select through the same
    /// list.
    /// </summary>
    public int DoorLink => Readings.DoorLink;

    /// <summary>Every reading of this behaviour.</summary>
    public RoomBlockReadings AllReadings => Readings;

    /// <summary>The reaction <paramref name="family"/>'s kinds read, or null when they have none for this behaviour.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="family"/> is undefined.</exception>
    public Enum? ReactionIn(BlockReactionFamily family) => family switch
    {
        BlockReactionFamily.Shootable => Shootable,
        BlockReactionFamily.Bombable => Bombable,
        BlockReactionFamily.Special => Special,
        BlockReactionFamily.SpecialAir => SpecialAir,
        BlockReactionFamily.Spike => Spike,
        BlockReactionFamily.Grapple => Grapple,
        _ => throw new InvalidOperationException($"Undefined reaction family {family}."),
    };

    /// <summary>The reaction <paramref name="kind"/> reads, or null for a kind that reads no reaction or has none for this behaviour.</summary>
    public Enum? ReactionUnder(RoomCollisionType kind) =>
        BlockReactionFamilies.Of(kind) is { } family ? ReactionIn(family) : null;

    /// <summary>The blue-door cap a shootable block's reaction selects, if it selects one.</summary>
    public bool TryGetBlueDoorOrientation(out ColoredDoorOrientation orientation) =>
        BlockReactionDefinitions.BlueDoorCaps.TryGetMeaning(Shootable, out orientation);

    /// <summary>
    /// The crumble block a special block's reaction selects, if it selects one. The same behaviours mean unrelated things to
    /// other collision kinds, so the caller first establishes that the block is special.
    /// </summary>
    public bool TryGetContactCrumble(out ContactCrumbleBlock crumble) =>
        BlockReactionDefinitions.ContactCrumbles.TryGetMeaning(Special, out crumble);

    /// <summary>The map, recharge or save-station access a special block's reaction selects, if it selects one.</summary>
    public bool TryGetStationAccess(out StationAccessBehavior access) =>
        BlockReactionDefinitions.StationAccesses.TryGetMeaning(Special, out access);

    /// <summary>
    /// The downward-gate trigger a shootable block's reaction selects, if it selects one. The same behaviours mean
    /// unrelated things to other collision kinds, so the caller first establishes that the block is shootable.
    /// </summary>
    public bool TryGetDownwardGateTrigger(out DownwardGateTriggerBehavior trigger) =>
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
            $"slope {readings.Slope.Shape} {readings.Slope.Mirroring}, extension {readings.ExtensionOffset}, door {readings.DoorLink}";
    }
}
