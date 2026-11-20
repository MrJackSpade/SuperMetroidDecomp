using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One of the room's enemy places and the enemy occupying it: its identity, where it is, how it collides, its health and
/// timers, and the species state installed for it. A place is found by the place object or its ordinal
/// (<see cref="Ordinal"/>); <see cref="EnemyPlaceOrder"/> sets how places are allocated and processed.
/// </summary>
public sealed class RoomEnemyPlace : ISolidEnemy, IEnemyTerrainBody
{
    /// <summary>Creates the place at <paramref name="ordinal"/> in <see cref="EnemyPlaceOrder"/>, before any occupant is installed.</summary>
    /// <param name="ordinal">The place's position in allocation and processing order.</param>
    /// <param name="registry">The species registry that creates the entity for each installed occupant.</param>
    /// <param name="leftovers">What the place's removed occupants leave for their neighbours, created with the room's places.</param>
    internal RoomEnemyPlace(int ordinal, EnemySpeciesRegistry registry, RemovedEnemyLeftovers leftovers)
    {
        _registry = Ensure.NotNull(registry);
        _leftovers = Ensure.NotNull(leftovers);
        Ordinal = Ensure.BetweenInclusive(0, EnemyPlaceOrder.Capacity - 1, ordinal);
    }

    /// <summary>
    /// What the place's last removed occupant left there for the enemies that read it: the enemy with its state as it left
    /// after a deletion, and the remains a kill emptied (<see cref="RemovedEnemyLeftovers"/>).
    /// </summary>
    private readonly RemovedEnemyLeftovers _leftovers;

    /// <summary>
    /// The place's ordinal in <see cref="EnemyPlaceOrder"/>, 0 through <see cref="EnemyPlaceOrder.Capacity"/> minus one,
    /// fixed for the place's lifetime: the place's position in allocation and processing order.
    /// </summary>
    [IntegerQuantity("place among the room's enemy places")]
    public int Ordinal { get; }
    /// <summary>
    /// The occupant's enemy identity; <see cref="EnemyDefinitionId.None"/> when the place holds no enemy, whether it is
    /// free or <see cref="AwaitsRespawn"/>.
    /// </summary>
    public EnemyDefinitionId DefinitionId { get; private set; }

    /// <summary>
    /// Whether the place holds no enemy but stays taken until the death explosion of the respawning enemy killed in it
    /// rebuilds that enemy from its population entry (#1851). No spawn takes the place meanwhile, and the enemy passes
    /// that reach every place pass it over as they pass a free one; the enemy pass still counts its updates on the update
    /// its enemy died.
    /// </summary>
    public bool AwaitsRespawn { get; private set; }

    /// <summary>
    /// Whether the place is free for a spawn: it holds no enemy and does not await one's respawn. A place's earlier
    /// occupant's partners read it as gone only once it is free.
    /// </summary>
    internal bool IsFree => DefinitionId == EnemyDefinitionId.None && !AwaitsRespawn;
    /// <summary>Immutable resolved definition for the current enemy identity, supplying initialization, collision, and AI definitions.</summary>
    public RoomEnemyDefinition Definition { get; private set; }

    /// <summary>The species registry that creates the entity for each installed occupant.</summary>
    private readonly EnemySpeciesRegistry _registry;

    /// <summary>
    /// The enemy occupying the place, holding its state: its species' factory builds it as the place is set up
    /// (<see cref="SetUp"/>), once the place holds its identity and population entry; null while the place is empty.
    /// </summary>
    internal IEnemyEntity? Entity { get; private set; }

    /// <summary>
    /// What another enemy finds when it reads this place, which it does without testing what the place holds
    /// (<see cref="RemovedEnemyLeftovers"/>): the enemy holding it, the enemy a deletion left there as it left, the record
    /// the last kill cleared, or nothing while no enemy has held it.
    /// </summary>
    internal EnemyPlaceFinding Found => _leftovers.Found(DefinitionId == EnemyDefinitionId.None ? null : Entity);

    /// <summary>
    /// The enemy another enemy finds when it reads this place (<see cref="Found"/>): the one holding it or the one a
    /// deletion left as it left; null when the place's last occupant was killed or no enemy has held it.
    /// </summary>
    internal IEnemyEntity? FoundEnemy => Found is EnemyPlaceFinding.Enemy found ? found.Entity : null;

    /// <summary>
    /// Builds the occupant's enemy with its state through its species' factory, at the point its initialization runs: after
    /// its identity, population entry and live values are installed (#2554).
    /// </summary>
    /// <param name="setup">How the occupant comes to be set up.</param>
    internal void SetUp(EnemySetup setup) => Entity = _registry.Create(this, Ensure.NotNull(setup));

    /// <summary>
    /// Installs a new occupant's identity, releasing the enemy that held the place; its species builds the new occupant's
    /// enemy as it is set up (<see cref="SetUp"/>). Every identity change goes through here, at the statement that changes
    /// the definition, because the game re-reads the definition each time the enemy runs.
    /// </summary>
    /// <param name="definitionId">The occupant's definition.</param>
    /// <param name="definition">The occupant's resolved definition.</param>
    internal void Bind(EnemyDefinitionId definitionId, RoomEnemyDefinition definition)
    {
        ReleaseEntity();
        DefinitionId = definitionId;
        Definition = definition;
        AwaitsRespawn = false;
        AnimationClock = new();
    }

    /// <summary>
    /// Stops holding the place's enemy, telling it that it has left the place (<see cref="IEnemyDeparture"/>) before any
    /// new occupant is bound.
    /// </summary>
    private void ReleaseEntity()
    {
        IEnemyEntity? leaving = Entity;
        Entity = null;
        if (leaving is IEnemyDeparture departure)
            departure.Leave();
    }

    /// <summary>
    /// Keeps the place of a respawning enemy just killed in it taken until its death explosion rebuilds it
    /// (<see cref="AwaitsRespawn"/>). The killed enemy's population entry stays in <see cref="Spawn"/> for the rebuild.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place still holds an enemy.</exception>
    internal void AwaitRespawn()
    {
        if (DefinitionId != EnemyDefinitionId.None)
            throw new InvalidOperationException($"Enemy place {Ordinal} still holds {DefinitionId}; only a killed enemy's place awaits its respawn.");
        AwaitsRespawn = true;
    }

    /// <summary>
    /// Empties the place's identity and enemy when the occupant is deleted without being killed. The place keeps the
    /// occupant's last position, health and timers, and the enemy with its state stays for the enemies that read it
    /// (<see cref="Found"/>, <see cref="RemovedEnemyLeftovers"/>, <see cref="FollowUpAfterStruckPart"/>).
    /// </summary>
    internal void Unbind()
    {
        _leftovers.Deleted(Entity);
        Vacate();
    }

    /// <summary>Empties the place's identity and enemy.</summary>
    private void Vacate()
    {
        DefinitionId = EnemyDefinitionId.None;
        ReleaseEntity();
    }
    /// <summary>
    /// The population entry the occupant was spawned from, kept unchanged while its live state moves on and after it is
    /// killed; only populating the place replaces it.
    /// </summary>
    public RoomEnemySpawnSnapshot Spawn { get; internal set; }

    /// <summary>The settings the occupant's population entry gives its species, as the <typeparamref name="T"/> it reads.</summary>
    /// <exception cref="InvalidDataException">The entry carries other settings.</exception>
    internal T SpawnSettings<T>() where T : EnemySpawnSettings => Spawn.Population.SpawnSettingsOf<T>();
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
    /// Empties the place when its occupant is killed: its identity, its enemy and the place's live values. The kill clears
    /// what the occupant leaves, so the enemies that read the place find no species state there. The
    /// population entry the occupant was spawned from survives the kill (<see cref="Spawn"/>): a respawn rebuilds the
    /// enemy from it (#2256).
    /// </summary>
    internal void Clear()
    {
        _leftovers.Killed();
        Vacate();
        Definition = default;
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
        AwaitsRespawn = false;
        DrawOffset = Offset16<RoomPixels>.Zero;
    }
}
