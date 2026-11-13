using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One of the room's enemy places and the enemy occupying it: its identity, where it is, how it collides, its health and
/// timers, and the species state installed for it. A place is found by the slot object or its ordinal
/// (<see cref="SlotIndex"/>); <see cref="EnemySlotOrder"/> sets how places are allocated and processed.
/// </summary>
public sealed class RoomEnemySlot
{
    /// <summary>Creates the place at <paramref name="slotIndex"/> in <see cref="EnemySlotOrder"/>, before any occupant is installed.</summary>
    internal RoomEnemySlot(int slotIndex, EnemySpeciesRegistry registry)
    {
        _registry = Ensure.NotNull(registry);
        SlotIndex = Ensure.BetweenInclusive(0, EnemySlotOrder.Capacity - 1, slotIndex);
    }

    /// <summary>
    /// The slot's ordinal in <see cref="EnemySlotOrder"/>, 0 through <see cref="EnemySlotOrder.Capacity"/> minus one,
    /// fixed for the slot's lifetime: the identity by which the game names "which enemy", such as the enemy Samus last
    /// met solidly in each direction.
    /// </summary>
    [IntegerQuantity("position of the slot in the enemy table")]
    public int SlotIndex { get; }
    /// <summary>The occupant's enemy identity; <see cref="EnemyDefinitionId.None"/> denotes an empty slot.</summary>
    public EnemyDefinitionId DefinitionId { get; private set; }
    /// <summary>Immutable resolved definition for the current enemy identity, supplying initialization, collision, and AI definitions.</summary>
    public RoomEnemyDefinition Definition { get; private set; }

    /// <summary>The species registry that creates the entity for each installed occupant.</summary>
    private readonly EnemySpeciesRegistry _registry;

    /// <summary>The enemy occupying the slot, bound with its identity; null while the slot is empty.</summary>
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
        SpeciesState<T>() ?? throw new InvalidOperationException($"Enemy slot {SlotIndex} has no {typeof(T).Name} for its occupant.");

    /// <summary>The occupant's <typeparamref name="T"/> state, or null when the place holds no occupant with one.</summary>
    internal T? SpeciesState<T>() where T : class => OccupantStates.Find<T>();

    /// <summary>Installs or replaces the occupant's <typeparamref name="T"/> state.</summary>
    internal void SetSpeciesState<T>(T state) where T : class => OccupantStates.Install(state);

    /// <summary>
    /// Installs a new occupant's identity and binds the entity its species creates. Every identity change goes through
    /// here, at the statement that changes the definition, because the game re-reads the definition each dispatch.
    /// </summary>
    /// <param name="definitionId">The occupant's definition.</param>
    /// <param name="definition">The occupant's resolved definition.</param>
    internal void Bind(EnemyDefinitionId definitionId, RoomEnemyDefinition definition)
    {
        DefinitionId = definitionId;
        Definition = definition;
        OccupantStates = new();
        Entity = definitionId == EnemyDefinitionId.None ? null : _registry.Create(this);
    }

    /// <summary>
    /// Empties the slot's identity and drops its occupant's species states when the occupant is deleted without being
    /// killed. The place keeps the occupant's last position, health and timers: the enemies that read a deleted partner
    /// read what it left, through the rule that says so (<see cref="DeletedParentLeftovers"/>, <see cref="FollowUpAfterStruckPart"/>),
    /// and a partner keeps the species state it took when it was placed.
    /// </summary>
    internal void Unbind()
    {
        DefinitionId = EnemyDefinitionId.None;
        Entity = null;
        OccupantStates = new();
    }
    /// <summary>The population entry the occupant was spawned from, kept unchanged while its live state moves on.</summary>
    public RoomEnemySpawnSnapshot Spawn { get; internal set; }

    /// <summary>The settings the occupant's population entry gives its species, as the <typeparamref name="T"/> it reads.</summary>
    /// <exception cref="InvalidDataException">The entry carries other settings.</exception>
    internal T SpawnSettings<T>() where T : EnemySpawnSettings => Spawn.Population.SpawnSettingsOf<T>();
    /// <summary>The enemy's 16.16 room position, each whole coordinate wrapping at sixteen bits.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Collision half-extents in pixels, initially the definition's.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>How the occupant meets Samus and her shots, initially the population entry's.</summary>
    public EnemyContact Contact { get; internal set; }
    /// <summary>What the room's enemy passes run for the occupant, initially the population entry's.</summary>
    public EnemyProcessing Processing { get; internal set; }
    /// <summary>Whether the occupant is drawn, still occupies the slot and returns after a kill, initially the population entry's.</summary>
    public EnemyPresence Presence { get; internal set; }
    /// <summary>
    /// Whether the occupant collides by its frames' hitboxes rather than by its radius, initially the population entry's.
    /// The enemies' draw pass also reaches such an occupant wherever it is, not only when its radius puts it on screen.
    /// </summary>
    public bool CollidesByFrameHitboxes { get; internal set; }
    /// <summary>Set when the occupant's animation program installs a new timed frame.</summary>
    public bool NewInstructionFrame { get; internal set; }
    /// <summary>Which of the ordinary, hurt, frozen and grappled behaviours the enemy's update dispatches to.</summary>
    public EnemyAiStates AiStates { get; internal set; }
    /// <summary>Remaining health, initially the definition's and reduced by weapon damage.</summary>
    public int Health { get; internal set; }
    /// <summary>The frame the enemy shows, an installed frame or the empty frame; null before any.</summary>
    public EnemyFrameId? Frame { get; internal set; }
    /// <summary>A loop or delay count the occupant's behaviour and animation keep; not an elapsed-frame counter.</summary>
    public WrappingCountdown Timer { get; internal set; }

    /// <summary>Updates left before the enemy's animation takes its next step; a new occupant starts at one.</summary>
    public WrappingCountdown FrameHold { get; internal set; }
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
    /// <summary>The hurt flash's remaining updates; the flash alternates the enemy's colours and its end ends hurt dispatch.</summary>
    [IntegerQuantity("updates of hurt flash left; Phantoon restores his colours on the eighth")]
    public int FlashTimer { get; internal set; }
    /// <summary>How long the enemy stays frozen; its frozen blinking and thaw read it.</summary>
    public int FrozenTimer { get; internal set; }
    /// <summary>Updates during which weapons and contact do not interact with the enemy, sampled before it counts down.</summary>
    public int InvincibilityTimer { get; internal set; }
    /// <summary>Draws left that shake the enemy one pixel sideways, counted down as it draws.</summary>
    public int ShakeTimer { get; internal set; }
    /// <summary>
    /// The enemy's own update count, 0 through 65535, wrapping; its behaviour and drawing read it, independently of the
    /// game's frame counter.
    /// </summary>
    public int FrameCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, value);
    }
    /// <summary>The definition's hurt duration, 0 through 255, copied into the slot when it is populated.</summary>
    public int HurtAiTime
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>The frames a hurt flash runs before any extension: the hurt duration, or four when it is zero.</summary>
    internal int HurtFlashFrames => HurtAiTime == 0 ? 4 : HurtAiTime;
    /// <summary>
    /// Where the enemy draws relative to its position: zero for every population entry; Ceres's steam sets it to
    /// place its puffs on the rotating station.
    /// </summary>
    public Offset16<RoomPixels> DrawOffset { get; internal set; }

    /// <summary>Empties the slot when its occupant is killed: its identity, its species states and the place's live values.</summary>
    internal void Clear()
    {
        Unbind();
        Definition = default;
        Spawn = default;
        Position = FixedPosition<RoomPixels>.Zero;
        Radius = Extent16<RoomPixels>.Zero;
        Contact = EnemyContact.None;
        Processing = EnemyProcessing.None;
        Presence = EnemyPresence.None;
        CollidesByFrameHitboxes = false;
        NewInstructionFrame = false;
        Health = 0;
        AiStates = EnemyAiStates.None;
        FrameHold = WrappingCountdown.Start(0);

        Timer = WrappingCountdown.Start(0);
        Frame = null;
        FrozenTimer = 0;
        FlashTimer = 0;
        Shading = PictureShading.Standard;
        FrameCounter = 0;
        ShakeTimer = 0;
        InvincibilityTimer = 0;
        HurtAiTime = 0;
        DrawOffset = Offset16<RoomPixels>.Zero;
    }
}
