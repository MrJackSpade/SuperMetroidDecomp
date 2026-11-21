using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One of the room's enemy places and the enemy occupying it: its identity, where it is, how it collides, its health and
/// timers, and the species state installed for it. A place is held and passed as itself; its position in the room's list
/// (<see cref="Ordinal"/>) is read only by the owner of the population's listing (<see cref="EnemyCompositeListing"/>) and
/// the compatibility services that reproduce a reach by placement. <see cref="EnemyPlaceOrder"/> sets how places are
/// allocated and processed.
/// </summary>
public sealed class RoomEnemyPlace : ISolidEnemy, IEnemyTerrainBody
{
    /// <summary>Creates the place at <paramref name="ordinal"/> in <see cref="EnemyPlaceOrder"/>, before any occupant is installed.</summary>
    /// <param name="ordinal">The place's position in allocation and processing order.</param>
    /// <param name="common">The shared participation every entity built in the place composes.</param>
    /// <param name="leftovers">What the place's removed occupants leave for their neighbours, created with the room's places.</param>
    internal RoomEnemyPlace(int ordinal, EnemyCommonParticipation common, RemovedEnemyLeftovers<IEnemyEntity> leftovers)
    {
        _common = Ensure.NotNull(common);
        _leftovers = Ensure.NotNull(leftovers);
        Ordinal = Ensure.BetweenInclusive(0, EnemyPlaceOrder.Capacity - 1, ordinal);
    }

    /// <summary>
    /// What the place's last removed occupant left there for the enemies that read it: the enemy as it left after a
    /// deletion, and nothing after a kill (<see cref="RemovedEnemyLeftovers{TEnemy}"/>).
    /// </summary>
    private readonly RemovedEnemyLeftovers<IEnemyEntity> _leftovers;

    /// <summary>
    /// The place's ordinal in <see cref="EnemyPlaceOrder"/>, 0 through <see cref="EnemyPlaceOrder.Capacity"/> minus one,
    /// fixed for the place's lifetime: the place's position in allocation and processing order.
    /// </summary>
    [IntegerQuantity("place among the room's enemy places")]
    public int Ordinal { get; }
    /// <summary>
    /// What the place has held since its room loaded (#2558): nothing yet, or the history of a populated place, which
    /// keeps the population entry its latest occupant was spawned from and whether an enemy holds it now.
    /// </summary>
    public EnemyPlaceHistory History { get; private set; } = NeverPopulatedEnemyPlace.Instance;

    /// <summary>
    /// The enemy holding the place (#2558), which carries its identity and the population entry it was spawned from; null
    /// while the place holds no enemy, whether it was never populated, was vacated or <see cref="AwaitsRespawn"/>. Each
    /// binding installs a new occupant.
    /// </summary>
    public RoomEnemyOccupant? Occupant => History is PopulatedEnemyPlace { Occupancy: OccupiedEnemyPlace occupied } ? occupied.Occupant : null;

    /// <summary>Whether the place holds an enemy of <paramref name="definition"/>; false while it holds none.</summary>
    /// <param name="definition">The enemy asked about.</param>
    public bool Holds(EnemyDefinitionId definition) => Occupant is { } occupant && occupant.DefinitionId == definition;

    /// <summary>
    /// Whether the place holds no enemy but stays taken until the death explosion of the respawning enemy killed in it
    /// rebuilds that enemy from its population entry (#1851), as its history records. No spawn takes the place meanwhile,
    /// and the enemy passes that reach every place pass it over as they pass a free one; the enemy pass still counts its
    /// updates on the update its enemy died.
    /// </summary>
    public bool AwaitsRespawn => History is PopulatedEnemyPlace { AwaitsRespawn: true };

    /// <summary>
    /// Whether the place is free for a spawn: it was never populated, or its last occupant left it with no respawn pending.
    /// A place's earlier occupant's partners read it as gone only once it is free.
    /// </summary>
    internal bool IsFree => History is NeverPopulatedEnemyPlace or PopulatedEnemyPlace { Occupancy: VacatedEnemyPlace };

    /// <summary>The shared participation every entity built in the place composes.</summary>
    private readonly EnemyCommonParticipation _common;

    /// <summary>The enemy occupying the place, bound with its identity; null while the place is empty.</summary>
    internal IEnemyEntity? Entity => BoundEntity?.Entity;

    /// <summary>
    /// The occupant's entity with the setup and main AI it runs, as its factory named them (#2571); null while the place is
    /// empty.
    /// </summary>
    internal EnemyOccupant? BoundEntity { get; private set; }

    /// <summary>
    /// The enemy another enemy finds when it reads this place, which it does without testing what the place holds
    /// (<see cref="RemovedEnemyLeftovers{TEnemy}"/>): the enemy holding it, the one a deletion left there as it left, or null once
    /// the place's last occupant was killed or while no enemy has held it.
    /// </summary>
    internal IEnemyEntity? FoundEntity => _leftovers.Found(Occupant is null ? null : Entity);

    /// <summary>
    /// The <typeparamref name="TRecord"/> a neighbour that writes to this place finds there once its last occupant was
    /// killed (<see cref="RemovedEnemyLeftovers{TEnemy}.ClearedByKill{TRecord}"/>): emptied when first reached, keeping what the
    /// neighbour writes; null while an enemy holds the place or its last removal was no kill.
    /// </summary>
    /// <typeparam name="TRecord">The record the neighbour reads and writes.</typeparam>
    internal TRecord? ClearedByKill<TRecord>() where TRecord : class, new() => _leftovers.ClearedByKill<TRecord>(Occupant is not null);

    /// <summary>
    /// Installs a new occupant spawned from <paramref name="spawn"/> and binds the entity its species creates. Every
    /// identity change goes through here, at the statement that changes the definition, because the game re-reads the
    /// definition each time the enemy runs.
    /// </summary>
    /// <param name="spawn">The bound population entry the occupant is spawned from, which names its enemy.</param>
    /// <param name="definition">The occupant's resolved definition, which the occupant holds.</param>
    /// <returns>The new occupant, whose entity its species builds as it is set up (<see cref="Build"/>).</returns>
    internal RoomEnemyOccupant Bind(EnemySpawn spawn, RoomEnemyDefinition definition)
    {
        ReleaseEntity();
        RoomEnemyOccupant occupant;
        if (History is PopulatedEnemyPlace populated)
            occupant = populated.Occupy(spawn, definition);
        else
        {
            var first = new PopulatedEnemyPlace(this, spawn, definition);
            History = first;
            occupant = first.LatestOccupant;
        }
        AnimationClock = new();
        return occupant;
    }

    /// <summary>
    /// Builds <paramref name="occupant"/>'s entity with the state it holds from its setup (#2554), through
    /// <paramref name="build"/>, its species' creation bound to its settings, once the place holds its identity and live
    /// values.
    /// </summary>
    /// <param name="occupant">The occupant the place was just bound to.</param>
    /// <param name="build">How the occupant's species builds it where it is set up.</param>
    /// <returns>The entity with the setup and main AI it runs.</returns>
    internal EnemyOccupant Build(RoomEnemyOccupant occupant, IEnemyBuild build) => Hold(Ensure.NotNull(build).Create(occupant, this, _common));

    /// <summary>Holds <paramref name="built"/> as the place's entity.</summary>
    private EnemyOccupant Hold(EnemyOccupant built)
    {
        BoundEntity = built;
        return built;
    }

    /// <summary>
    /// Stops holding the place's enemy, telling it that it has left the place (<see cref="IEnemyDeparture"/>) before any
    /// new occupant is bound.
    /// </summary>
    private void ReleaseEntity()
    {
        IEnemyEntity? leaving = Entity;
        BoundEntity = null;
        if (leaving is IEnemyDeparture departure)
            departure.Leave();
    }

    /// <summary>
    /// Empties the place's identity and its occupant's species states when the occupant is deleted without being killed.
    /// The place keeps the occupant's last position, health and timers, and the enemy with its state stays for the enemies
    /// that read it (<see cref="FoundEntity"/>, <see cref="RemovedEnemyLeftovers{TEnemy}"/>, <see cref="FollowUpAfterStruckPart"/>).
    /// </summary>
    internal void Unbind()
    {
        if (Entity is { } leaving)
            _leftovers.Deleted(leaving);
        Vacate();
    }

    /// <summary>
    /// Empties the place of its occupant and its entity, leaving its other values and what earlier occupants left: a
    /// deletion and a kill go on from here, and a recorded state's free place is imported by it.
    /// </summary>
    private void Vacate()
    {
        if (History is PopulatedEnemyPlace populated)
            populated.Vacate();
        ReleaseEntity();
    }
    /// <summary>The enemy's room position with its fraction of a pixel, each whole coordinate wrapping around the room.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Collision half-extents in pixels, initially the definition's.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }

    /// <inheritdoc/>
    FixedPosition<RoomPixels> IEnemyTerrainBody.Position
    {
        get => Position;
        set => Position = value;
    }

    /// <inheritdoc/>
    Extent16<RoomPixels> IEnemyTerrainBody.Radius => Radius;
    /// <summary>How the occupant meets Samus and her shots, initially the population entry's.</summary>
    public EnemyContact Contact { get; internal set; }
    /// <summary>What the room's enemy passes run for the occupant, initially the population entry's.</summary>
    public EnemyProcessing Processing { get; internal set; }
    /// <summary>Whether the occupant is drawn, still occupies the place and returns after a kill, initially the population entry's.</summary>
    public EnemyPresence Presence { get; internal set; }
    /// <summary>
    /// Whether the occupant collides by its frames' hitboxes rather than by its radius, initially the population entry's.
    /// The enemies' draw pass also reaches such an occupant wherever it is, not only when its radius puts it on screen.
    /// </summary>
    public bool CollidesByFrameHitboxes { get; internal set; }
    /// <summary>Which of the ordinary, hurt, frozen and grappled behaviours the enemy's update runs.</summary>
    public EnemyAiStates AiStates { get; internal set; }
    /// <summary>Remaining health, initially the definition's and reduced by weapon damage.</summary>
    public int Health { get; internal set; }
    /// <summary>The frame the enemy shows, an installed frame or the empty frame; null before any.</summary>
    public EnemyFrameId? Frame { get; internal set; }

    /// <summary>
    /// The occupant's animation clock (#1624): the hold of the frame its animation shows and
    /// whether this update showed a new frame. A newly bound occupant gets a stopped clock, which its
    /// initialization starts; a killed occupant's place keeps a stopped one, and a deleted occupant's place keeps its clock
    /// with the rest of what it left.
    /// </summary>
    public EnemyAnimationClock AnimationClock { get; private set; } = new();
    /// <summary>
    /// The shades and colour effects the enemy shows its pictures in (#1512) while no hit or freeze flash overrides
    /// them: its sheet's standard colours, the colours of the variant its room places it in, unless its behaviour shows a look of
    /// its own, such as a glow's frame or a fade's step.
    /// </summary>
    public PictureShading Shading
    {
        get;
        internal set => field = Ensure.NotNull(value);
    } = PictureShading.Standard;
    /// <summary>
    /// The hurt flash, counted by the enemy pass after the enemy joins its draw queue: the flash alternates the enemy's
    /// colours by its updates left, and too few left end the hurt behaviour.
    /// </summary>
    public UpdateCountdown HurtFlash { get; } = UpdateCountdown.Ended();
    /// <summary>
    /// The freeze, counted where the enemy pass runs the frozen behaviour: its frozen blinking reads its updates left, and
    /// a thaw leaves the leftover <see cref="ThawedFreezeLeftover"/> names.
    /// </summary>
    public UpdateCountdown Freeze { get; } = UpdateCountdown.Ended();
    /// <summary>The updates during which weapons and contact do not interact with the enemy, sampled before the enemy pass counts it.</summary>
    public UpdateCountdown Invincibility { get; } = UpdateCountdown.Ended();
    /// <summary>The draws left that shake the enemy one pixel sideways, counted by the draw pass as it forms the enemy's origin.</summary>
    public UpdateCountdown Shake { get; } = UpdateCountdown.Ended();

    /// <inheritdoc/>
    void ISolidEnemy.ShakeFromWallJumpLaunch() => Shake.Restart(EnemyDrawPhaseDefinitions.WallJumpLaunchShakeDraws);

    /// <summary>
    /// The enemy's own update count, independent of the game's frame counter: its behaviour and drawing read it only
    /// through power-of-two cadences (an <see cref="UpdateCadence"/>, an <see cref="UpdateCounterField"/> or a random field
    /// advanced by it), so it is a plain count (#2454).
    /// </summary>
    public int FrameCounter
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
    /// <summary>
    /// The updates a hurt flash runs before any weapon's extension: the definition's, which the import resolves for every
    /// definition (a definition that names none takes the standard flash), copied into the place when it is populated;
    /// zero while the place holds no enemy.
    /// </summary>
    public int HurtFlashFrames
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
    /// <summary>
    /// Where the enemy draws relative to its position: zero for every population entry; Ceres's steam sets it to
    /// place its puffs on the rotating station, and Crocomire's tongue to keep its picture against his body's picture
    /// when he steps after placing it.
    /// </summary>
    public Offset16<RoomPixels> DrawOffset { get; internal set; }

    /// <summary>
    /// Empties the place when its occupant is killed: its occupant, its entity and the place's live values. The kill clears
    /// what the occupant leaves, so the enemies that read the place find no enemy there. The
    /// population entry the occupant was spawned from survives the kill in the place's <see cref="History"/>: a respawn
    /// rebuilds the enemy from it (#2256). Only a kill of its occupant clears a place (<see cref="RoomEnemyOccupant.Kill"/>).
    /// </summary>
    internal void Clear()
    {
        _leftovers.Killed();
        Vacate();
        Position = FixedPosition<RoomPixels>.Zero;
        Radius = Extent16<RoomPixels>.Zero;
        Contact = EnemyContact.None;
        Processing = EnemyProcessing.None;
        Presence = EnemyPresence.None;
        CollidesByFrameHitboxes = false;
        Health = 0;
        AiStates = EnemyAiStates.None;
        AnimationClock = new();
        Frame = null;
        Freeze.End();
        HurtFlash.End();
        Shading = PictureShading.Standard;
        FrameCounter = 0;
        Shake.End();
        Invincibility.End();
        HurtFlashFrames = 0;
        DrawOffset = Offset16<RoomPixels>.Zero;
    }
}
