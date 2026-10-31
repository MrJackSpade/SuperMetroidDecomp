using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One of the room's enemy places and the enemy occupying it: its identity, where it is, how it collides, its health and
/// timers, and the species state installed for it. A place is addressed by the slot object or its ordinal
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
    /// <summary>Immutable resolved header for the current enemy identity, supplying initialization, collision, and AI definitions.</summary>
    public RoomEnemyDefinition Definition { get; private set; }

    /// <summary>The species registry that creates the entity for each installed occupant.</summary>
    private readonly EnemySpeciesRegistry _registry;

    /// <summary>The enemy occupying the slot, bound with its identity; null while the slot is empty.</summary>
    internal IEnemyEntity? Entity { get; private set; }

    /// <summary>
    /// Each species' state installed in this slot, keyed by state type. Like the slot's other state it outlives the
    /// occupant: a reaction tail that runs after common damage killed the enemy still reads it, until the next occupant of
    /// that species replaces it or the room loads.
    /// </summary>
    private readonly Dictionary<string, object> _speciesStates = new(StringComparer.Ordinal);

    /// <summary>
    /// For each installed species state that keeps values the cartridge stores in this record's common words, the
    /// operation that empties those values, keyed like the states. <see cref="Clear"/> runs them: emptying the record
    /// empties those values, while the state itself outlives the occupant.
    /// </summary>
    private readonly Dictionary<string, Action> _clearWithRecord = new(StringComparer.Ordinal);

    /// <summary>The slot's species state, which its species installs before reading it.</summary>
    /// <exception cref="InvalidOperationException">The slot holds no <typeparamref name="T"/>.</exception>
    internal T RequireSpeciesState<T>() where T : class =>
        SpeciesState<T>() ?? throw new InvalidOperationException($"Enemy slot {SlotIndex} has no initialized {typeof(T).Name}.");

    /// <summary>The <typeparamref name="T"/> state last installed in this slot this room, or null.</summary>
    internal T? SpeciesState<T>() where T : class =>
        _speciesStates.TryGetValue(StateKey<T>(), out object? state) ? (T)state : null;

    /// <summary>Installs or, with null, removes this slot's <typeparamref name="T"/> state.</summary>
    internal void SetSpeciesState<T>(T? state) where T : class
    {
        _clearWithRecord.Remove(StateKey<T>());
        if (state is null)
            _speciesStates.Remove(StateKey<T>());
        else
            _speciesStates[StateKey<T>()] = state;
    }

    /// <summary>
    /// Installs this slot's <typeparamref name="T"/> state together with <paramref name="clearWithRecord"/>, which
    /// empties the values the state keeps in place of this record's common words whenever <see cref="Clear"/> empties
    /// the record.
    /// </summary>
    internal void SetSpeciesState<T>(T state, Action clearWithRecord) where T : class
    {
        _speciesStates[StateKey<T>()] = Ensure.NotNull(state);
        _clearWithRecord[StateKey<T>()] = Ensure.NotNull(clearWithRecord);
    }


    /// <summary>The storage key of a state type.</summary>
    private static string StateKey<T>() => typeof(T).FullName ?? throw new InvalidOperationException("A state type has no name.");

    /// <summary>
    /// Installs a new occupant's identity and binds the entity its species creates. Every identity change goes through
    /// here, at the statement that changes the header pointer, because the cartridge re-reads the header each dispatch.
    /// </summary>
    /// <param name="definitionId">The occupant's header pointer.</param>
    /// <param name="definition">The occupant's header.</param>
    internal void Bind(EnemyDefinitionId definitionId, RoomEnemyDefinition definition)
    {
        DefinitionId = definitionId;
        Definition = definition;
        Entity = definitionId == EnemyDefinitionId.None ? null : _registry.Create(this);
    }

    /// <summary>Empties the slot's identity, as the deleted-property path does, leaving its other state.</summary>
    internal void Unbind()
    {
        DefinitionId = 0;
        Entity = null;
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
    /// <summary>The occupant's properties controlling collision, drawing and animation, initially the population entry's.</summary>
    public EnemyProperties Properties { get; internal set; }
    /// <summary>The occupant's additional properties, including whether it draws extended frames.</summary>
    public EnemyExtraProperties ExtraProperties { get; internal set; }
    /// <summary>Which of the ordinary, hurt, frozen and grappled behaviours the enemy's update dispatches to.</summary>
    public EnemyAiStates AiStates { get; internal set; }
    /// <summary>Remaining health, initially the definition's and reduced by weapon damage.</summary>
    public int Health { get; internal set; }
    /// <summary>The frame the enemy shows, an installed spritemap or extended frame; null before any.</summary>
    public EnemyFrameId? Frame { get; internal set; }
    /// <summary>A loop or delay count the occupant's behaviour and animation keep; not an elapsed-frame counter.</summary>
    public WrappingCountdown Timer { get; internal set; }

    /// <summary>Updates left before the enemy's animation takes its next step; a new occupant starts at one.</summary>
    public WrappingCountdown InstructionTimer { get; internal set; }
    /// <summary>
    /// What colours the enemy draws in (#1464), normally the colours its graphics set loads for it; an emptied slot
    /// draws in the common sprite colours.
    /// </summary>
    public SpriteColouring Colouring
    {
        get;
        internal set => field = Ensure.NotNull(value);
    } = SharedSpriteColouring.Common;
    /// <summary>
    /// The sheet character the occupant's graphics start at, as its graphics set places them; the projectiles and sprite
    /// objects it spawns draw from the spawned-object sheet composed at that character (#1471 keeps it for that reader).
    /// </summary>
    public int TileIndexBase
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>The sheet characters and colours the enemy draws with, as the projectiles and sprite objects it spawns take them.</summary>
    public SpriteGraphics Graphics => new(TileIndexBase, Colouring);
    /// <summary>
    /// The enemy's draw layer, initially the definition's: enemies draw layer by layer, and the draw queue takes the
    /// layer's lowest three bits, so it orders the enemy among the eight queues.
    /// </summary>
    public int Layer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>The hurt flash's remaining updates; the flash alternates the enemy's colours and its end ends hurt dispatch.</summary>
    public WrappingCountdown FlashTimer { get; internal set; }
    /// <summary>How long the enemy stays frozen; its frozen blinking and thaw read it.</summary>
    public WrappingCountdown FrozenTimer { get; internal set; }
    /// <summary>Updates during which weapons and contact do not interact with the enemy, sampled before it counts down.</summary>
    public WrappingCountdown InvincibilityTimer { get; internal set; }
    /// <summary>Draws left that shake the enemy one pixel sideways, counted down as it draws.</summary>
    public WrappingCountdown ShakeTimer { get; internal set; }
    /// <summary>
    /// The enemy's own update count, 0 through 65535, wrapping; its behaviour and drawing read it, independently of the
    /// game's frame counter.
    /// </summary>
    public int FrameCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>The definition's hurt duration, 0 through 255, copied into the slot when it is populated.</summary>
    public int HurtAiTime
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>The frames a hurt flash runs before any extension: the hurt duration, or four when it is zero.</summary>
    internal int HurtFlashFrames => HurtAiTime == 0 ? 4 : HurtAiTime;
    /// <summary>
    /// Where the enemy draws relative to its position: zero for every population entry; Ceres's steam sets it to
    /// place its puffs on the rotating station.
    /// </summary>
    public Offset16<RoomPixels> DrawOffset { get; internal set; }

    /// <summary>Empties the slot when its occupant is removed: identity, live state and the species values kept with it.</summary>
    internal void Clear()
    {
        Unbind();
        Definition = default;
        Spawn = default;
        Position = FixedPosition<RoomPixels>.Zero;
        Radius = Extent16<RoomPixels>.Zero;
        Properties = default;
        ExtraProperties = default;
        Health = 0;
        AiStates = EnemyAiStates.None;
        InstructionTimer = WrappingCountdown.Start(0);

        Timer = WrappingCountdown.Start(0);
        Frame = null;
        FrozenTimer = WrappingCountdown.Start(0);
        FlashTimer = WrappingCountdown.Start(0);
        Layer = 0;
        TileIndexBase = 0;
        Colouring = SharedSpriteColouring.Common;
        FrameCounter = 0;
        ShakeTimer = WrappingCountdown.Start(0);
        InvincibilityTimer = WrappingCountdown.Start(0);
        HurtAiTime = 0;
        DrawOffset = Offset16<RoomPixels>.Zero;
        foreach (Action clearWithRecord in _clearWithRecord.Values)
            clearWithRecord();
    }
}
