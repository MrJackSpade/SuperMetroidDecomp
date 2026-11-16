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
public sealed class RoomEnemyPlace : ISolidEnemy
{
    /// <summary>Creates the place at <paramref name="ordinal"/> in <see cref="EnemyPlaceOrder"/>, before any occupant is installed.</summary>
    /// <param name="ordinal">The place's position in allocation and processing order.</param>
    /// <param name="registry">The species registry that creates the entity for each installed occupant.</param>
    /// <param name="leftovers">What the place's removed occupants leave for their neighbours, created with the room's places.</param>
    internal RoomEnemyPlace(int ordinal, EnemySpeciesRegistry registry, RemovedEnemyLeftovers<EnemyOccupantStates> leftovers)
    {
        _registry = Ensure.NotNull(registry);
        _leftovers = Ensure.NotNull(leftovers);
        Ordinal = Ensure.BetweenInclusive(0, EnemyPlaceOrder.Capacity - 1, ordinal);
    }

    /// <summary>
    /// What the place's last removed occupant left there for the enemies that read it: its species states as it left them
    /// after a deletion, and cleared after a kill (<see cref="RemovedEnemyLeftovers{TStates}"/>).
    /// </summary>
    private readonly RemovedEnemyLeftovers<EnemyOccupantStates> _leftovers;

    /// <summary>
    /// The place's ordinal in <see cref="EnemyPlaceOrder"/>, 0 through <see cref="EnemyPlaceOrder.Capacity"/> minus one,
    /// fixed for the place's lifetime: the place's position in allocation and processing order.
    /// </summary>
    [IntegerQuantity("position of the place in the enemy table")]
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

    /// <summary>The enemy occupying the place, bound with its identity; null while the place is empty.</summary>
    internal IEnemyEntity? Entity { get; private set; }

    /// <summary>
    /// The species states the current occupant owns (#1543). Its species installs them as it initializes, and the place
    /// starts an empty set whenever its occupant leaves: a new occupant is bound, or the occupant is deleted or killed. No
    /// state outlives its occupant; a reaction that runs on after a killing hit reaches the struck occupant's set through
    /// its hit context.
    /// </summary>
    internal EnemyOccupantStates OccupantStates { get; private set; } = new();

    /// <summary>The occupant's species state, which its species installs before reading it.</summary>
    /// <exception cref="InvalidOperationException">The occupant holds no <typeparamref name="T"/>.</exception>
    internal T RequireSpeciesState<T>() where T : class =>
        SpeciesState<T>() ?? throw new InvalidOperationException($"Enemy place {Ordinal} has no {typeof(T).Name} for its occupant.");

    /// <summary>The occupant's <typeparamref name="T"/> state, or null when the place holds no occupant with one.</summary>
    internal T? SpeciesState<T>() where T : class => OccupantStates.Find<T>();

    /// <summary>Installs or replaces the occupant's <typeparamref name="T"/> state.</summary>
    internal void SetSpeciesState<T>(T state) where T : class => OccupantStates.Install(state);

    /// <summary>
    /// The <typeparamref name="T"/> another enemy finds when it reads this place, which it does without testing what the
    /// place holds (<see cref="RemovedEnemyLeftovers{TStates}"/>): the occupant's own while an enemy holds the place, the
    /// state a deleted occupant left, or null when the place's last occupant was killed, whose cleared state reads as
    /// nothing unless a neighbour wrote it (<see cref="FoundOrClearedState{T}"/>), or held none.
    /// </summary>
    internal T? FoundState<T>() where T : class =>
        _leftovers.Found(Occupant)?.Find<T>();

    /// <summary>
    /// The <typeparamref name="T"/> a neighbour that writes to this place finds there, which it does without testing what
    /// the place holds: <see cref="FoundState{T}"/>'s, or, when the place's last occupant was killed, the
    /// <typeparamref name="T"/> among the states the kill cleared (<see cref="RemovedEnemyLeftovers{TStates}.ClearedByKill"/>),
    /// emptied when first reached and keeping what the neighbour writes; null when the place holds no
    /// <typeparamref name="T"/> otherwise.
    /// </summary>
    internal T? FoundOrClearedState<T>() where T : class, new()
    {
        if (FoundState<T>() is { } found)
            return found;
        if (_leftovers.ClearedByKill(Occupant) is not { } cleared)
            return null;
        var emptied = new T();
        cleared.Install(emptied);
        return emptied;
    }

    /// <summary>The species states of the enemy holding the place, or null while it holds none.</summary>
    private EnemyOccupantStates? Occupant => DefinitionId == EnemyDefinitionId.None ? null : OccupantStates;

    /// <summary>
    /// Installs a new occupant's identity and binds the entity its species creates. Every identity change goes through
    /// here, at the statement that changes the definition, because the game re-reads the definition each time the enemy runs.
    /// </summary>
    /// <param name="definitionId">The occupant's definition.</param>
    /// <param name="definition">The occupant's resolved definition.</param>
    internal void Bind(EnemyDefinitionId definitionId, RoomEnemyDefinition definition)
    {
        DefinitionId = definitionId;
        Definition = definition;
        AwaitsRespawn = false;
        OccupantStates = new();
        AnimationClock = new();
        Entity = definitionId == EnemyDefinitionId.None ? null : _registry.Create(this);
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
    /// Empties the place's identity and its occupant's species states when the occupant is deleted without being killed.
    /// The place keeps the occupant's last position, health and timers, and its species states stay for the enemies that
    /// read it (<see cref="FoundState{T}"/>, <see cref="RemovedEnemyLeftovers{TStates}"/>, <see cref="FollowUpAfterStruckPart"/>).
    /// </summary>
    internal void Unbind()
    {
        _leftovers.Deleted(OccupantStates);
        Vacate();
    }

    /// <summary>Empties the place's identity, entity and species states.</summary>
    private void Vacate()
    {
        DefinitionId = EnemyDefinitionId.None;
        Entity = null;
        OccupantStates = new();
    }
    /// <summary>
    /// The population entry the occupant was spawned from, kept unchanged while its live state moves on and after it is
    /// killed; only populating the place replaces it.
    /// </summary>
    public RoomEnemySpawnSnapshot Spawn { get; internal set; }

    /// <summary>The settings the occupant's population entry gives its species, as the <typeparamref name="T"/> it reads.</summary>
    /// <exception cref="InvalidDataException">The entry carries other settings.</exception>
    internal T SpawnSettings<T>() where T : EnemySpawnSettings => Spawn.Population.SpawnSettingsOf<T>();
    /// <summary>The enemy's 1/65536-pixel room position, each whole coordinate wrapping at sixteen bits.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Collision half-extents in pixels, initially the definition's.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
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
    /// <summary>The hurt flash's remaining updates; the flash alternates the enemy's colours and its end ends the hurt behaviour.</summary>
    [IntegerQuantity("updates of hurt flash left; Phantoon restores his colours on the eighth")]
    public int FlashTimer { get; internal set; }
    /// <summary>How long the enemy stays frozen; its frozen blinking and thaw read it.</summary>
    public int FrozenTimer { get; internal set; }
    /// <summary>Updates during which weapons and contact do not interact with the enemy, sampled before it counts down.</summary>
    public int InvincibilityTimer { get; internal set; }
    /// <summary>Draws left that shake the enemy one pixel sideways, counted down as it draws.</summary>
    public int ShakeTimer { get; internal set; }

    /// <inheritdoc/>
    void ISolidEnemy.ShakeFromWallJumpLaunch() => ShakeTimer = EnemyDrawPhaseDefinitions.WallJumpLaunchShakeDraws;

    /// <summary>
    /// The enemy's own update count, 0 through 65535, wrapping; its behaviour and drawing read it, independently of the
    /// game's frame counter.
    /// </summary>
    public int FrameCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, value);
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
    /// Empties the place when its occupant is killed: its identity, its species states and the place's live values. The
    /// kill clears what the occupant leaves, so the enemies that read the place find no species state there. The
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
        FrozenTimer = 0;
        FlashTimer = 0;
        Shading = PictureShading.Standard;
        FrameCounter = 0;
        ShakeTimer = 0;
        InvincibilityTimer = 0;
        HurtFlashFrames = 0;
        AwaitsRespawn = false;
        DrawOffset = Offset16<RoomPixels>.Zero;
    }
}
