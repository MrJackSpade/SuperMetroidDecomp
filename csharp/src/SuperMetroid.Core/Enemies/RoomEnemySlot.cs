using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Mutable projection of the 64-byte WRAM <c>EnemyData</c> record. Named properties retain
/// their native word widths even where current translated actors only consume a low byte.
/// </summary>
public sealed class RoomEnemySlot
{
    /// <summary>Assigns the stable native $40-byte slot index before population data is copied in.</summary>
    internal RoomEnemySlot(int slotIndex, EnemySpeciesRegistry registry)
    {
        _registry = Ensure.NotNull(registry);
        SlotIndex = slotIndex;
    }

    /// <summary>Zero-based managed slot number, fixed for the slot object's lifetime; room populations occupy at most 32 native slots.</summary>
    [IntegerQuantity("position of the slot in the enemy table")]
    public int SlotIndex { get; }
    /// <summary>Native byte index into EnemyData, equal to <see cref="SlotIndex"/> times $40, rather than a WRAM address.</summary>
    public int NativeIndex => SlotIndex * EnemySlotTable.NativeSlotSize;
    /// <summary>Enemy.ID at slot +$00: bank-$A0 header identity; zero denotes a cleared/inactive slot.</summary>
    public EnemyDefinitionId EnemyDefinitionPointer { get; private set; }
    /// <summary>Immutable resolved header for the current enemy identity, supplying initialization, collision, and AI definitions.</summary>
    public RoomEnemyDefinition Definition { get; private set; }

    /// <summary>The species registry that creates the entity for each installed occupant.</summary>
    private readonly EnemySpeciesRegistry _registry;

    /// <summary>The enemy occupying the slot, bound with its identity; null while the slot is empty.</summary>
    internal IEnemyEntity? Entity { get; private set; }

    /// <summary>
    /// Each species' state installed in this physical slot, keyed by state type. Like the slot's words it outlives the
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
    /// <param name="definitionPointer">The occupant's header pointer.</param>
    /// <param name="definition">The occupant's header.</param>
    internal void Bind(EnemyDefinitionId definitionPointer, RoomEnemyDefinition definition)
    {
        EnemyDefinitionPointer = definitionPointer;
        Definition = definition;
        Entity = definitionPointer == 0 ? null : _registry.Create(this);
    }

    /// <summary>Empties the slot's identity, as the deleted-property path does, leaving its other words.</summary>
    internal void Unbind()
    {
        EnemyDefinitionPointer = 0;
        Entity = null;
    }
    /// <summary>Original population and character-offset values retained independently of mutable live slot fields.</summary>
    public RoomEnemySpawnSnapshot Spawn { get; internal set; }
    /// <summary>
    /// Enemy.XPosition/XSubPosition/YPosition/YSubPosition at +$02..+$08: the native 16.16 room
    /// position, each whole word wrapping at sixteen bits.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Enemy.XHitboxRadius/YHitboxRadius at +$0A/+$0C: collision half-extents in pixels, initially copied from the header.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>Enemy.properties at +$0E: mutable primary native property bits controlling collision, drawing, and instruction processing.</summary>
    public EnemyPropertyWord Properties { get; internal set; }
    /// <summary>Enemy.properties2 at +$10: mutable additional properties, including extended-spritemap selection.</summary>
    public EnemyExtraPropertyWord ExtraProperties { get; internal set; }
    /// <summary>Enemy.AI at +$12: native handler-selection bits for ordinary, hurt, frozen, and other AI dispatch states.</summary>
    public EnemyAiStates AiStates { get; internal set; }
    /// <summary>Enemy.health at +$14: mutable remaining enemy health, initialized from the definition and reduced by weapon damage.</summary>
    public int Health { get; internal set; }
    /// <summary>Enemy.spritemap at +$16: the frame the actor shows, an installed spritemap or extended frame; null before any.</summary>
    public EnemyFrameId? Frame { get; internal set; }
    /// <summary>Enemy.loopCounter at +$18: actor/instruction-owned loop or delay word; it is not a universal elapsed-frame counter.</summary>
    public WrappingCountdown Timer { get; internal set; }
    /// <summary>
    /// Enemy.instList at +$1A, 0 through 65535: populated from the population's actor-specific initialization
    /// parameter, which several families read as packed selectors; an imported state carries the native instruction
    /// pointer here.
    /// </summary>
    public int CurrentInstruction
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>Enemy.instTimer at +$1C: animation instruction countdown in processing updates; initialization starts it at one.</summary>
    public WrappingCountdown InstructionTimer { get; internal set; }
    /// <summary>Enemy.palette at +$1E: native OBJ palette attribute bits, normally the graphics-set palette index shifted left nine, rather than a CGRAM color index.</summary>
    public int PaletteIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>Enemy.GFXOffset at +$20: sprite character-index offset assigned by the graphics set, in 32-byte planar tile units.</summary>
    public int VramTilesIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>
    /// Enemy.layer at +$22, 0 through 65535: drawing-queue layer word, initialized from the definition's byte-sized
    /// layer; the draw queue reads its low three bits.
    /// </summary>
    public int Layer
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
    /// <summary>Enemy.flashTimer at +$24: hurt-flash countdown consumed by enemy processing and palette selection; reaching the native threshold clears hurt-AI dispatch.</summary>
    public WrappingCountdown FlashTimer { get; internal set; }
    /// <summary>Enemy.freezeTimer at +$26: remaining freeze-processing countdown; frozen palette blinking and thaw decisions read the same native word.</summary>
    public WrappingCountdown FrozenTimer { get; internal set; }
    /// <summary>Enemy.invincibilityTimer at +$28: damage-interaction suppression countdown, sampled before its per-enemy decrement.</summary>
    public WrappingCountdown InvincibilityTimer { get; internal set; }
    /// <summary>Enemy.shakeTimer at +$2A: remaining draw calls applying the enemy's one-pixel horizontal shake, decremented during drawing.</summary>
    public WrappingCountdown ShakeTimer { get; internal set; }
    /// <summary>
    /// Enemy.frameCounter at +$2C, 0 through 65535: wrapping enemy-processing counter used by actor behavior and
    /// drawing, independent of the global NMI counter.
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
    /// <summary>Native spawn-data graphical offset, added to the live whole position before camera subtraction.</summary>
    public Offset16<RoomPixels> SpawnOffset { get; internal set; }

    /// <summary>Resets all mutable enemy, animation and actor scratch words when a native slot becomes inactive.</summary>
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
        CurrentInstruction = 0;
        Timer = WrappingCountdown.Start(0);
        Frame = null;
        FrozenTimer = WrappingCountdown.Start(0);
        FlashTimer = WrappingCountdown.Start(0);
        Layer = 0;
        VramTilesIndex = 0;
        PaletteIndex = 0;
        FrameCounter = 0;
        ShakeTimer = WrappingCountdown.Start(0);
        InvincibilityTimer = WrappingCountdown.Start(0);
        HurtAiTime = 0;
        SpawnOffset = Offset16<RoomPixels>.Zero;
        foreach (Action clearWithRecord in _clearWithRecord.Values)
            clearWithRecord();
    }
}
