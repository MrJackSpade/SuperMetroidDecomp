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
        NativeIndex = checked((ushort)(slotIndex * RoomEnemySystem.NativeSlotSize));
    }

    /// <summary>Zero-based managed slot number, fixed for the slot object's lifetime; room populations occupy at most 32 native slots.</summary>
    public int SlotIndex { get; }
    /// <summary>Native byte index into EnemyData, equal to <see cref="SlotIndex"/> times $40, rather than a WRAM address.</summary>
    public ushort NativeIndex { get; }
    /// <summary>Enemy.ID at slot +$00: bank-$A0 header identity; zero denotes a cleared/inactive slot.</summary>
    public EnemyDefinitionId EnemyDefinitionPointer { get; private set; }
    /// <summary>Immutable resolved header for the current enemy identity, supplying initialization, collision, and AI definitions.</summary>
    public RoomEnemyDefinition Definition { get; private set; }

    /// <summary>The species registry that creates the entity for each installed occupant.</summary>
    private readonly EnemySpeciesRegistry _registry;

    /// <summary>The enemy occupying the slot, bound with its identity; null while the slot is empty.</summary>
    internal IEnemyEntity? Entity { get; private set; }

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
    public ushort Properties { get; internal set; }
    /// <summary>Enemy.properties2 at +$10: mutable additional properties, including extended-spritemap selection.</summary>
    public ushort ExtraProperties { get; internal set; }
    /// <summary>Enemy.AI at +$12: native handler-selection bits for ordinary, hurt, frozen, and other AI dispatch states.</summary>
    public EnemyAiStates AiStates { get; internal set; }
    /// <summary>Enemy.health at +$14: mutable remaining enemy health, initialized from the definition and reduced by weapon damage.</summary>
    public ushort Health { get; internal set; }
    /// <summary>Enemy.spritemap at +$16: current bank-relative drawing frame or extended-map identity selected by the actor.</summary>
    public ushort SpritemapPointer { get; internal set; }
    /// <summary>Enemy.loopCounter at +$18: actor/instruction-owned loop or delay word; it is not a universal elapsed-frame counter.</summary>
    public NativeCountdown Timer { get; internal set; }
    /// <summary>Enemy.instList at +$1A: current bank-relative animation instruction pointer, initially populated from the actor-specific initialization parameter.</summary>
    public ushort CurrentInstruction { get; internal set; }
    /// <summary>Enemy.instTimer at +$1C: animation instruction countdown in processing updates; initialization starts it at one.</summary>
    public NativeCountdown InstructionTimer { get; internal set; }
    /// <summary>Enemy.palette at +$1E: native OBJ palette attribute bits, normally the graphics-set palette index shifted left nine, rather than a CGRAM color index.</summary>
    public ushort PaletteIndex { get; internal set; }
    /// <summary>Enemy.GFXOffset at +$20: sprite character-index offset assigned by the graphics set, in 32-byte planar tile units.</summary>
    public ushort VramTilesIndex { get; internal set; }
    /// <summary>Enemy.layer at +$22: drawing-queue layer word, initialized from the definition's byte-sized layer.</summary>
    public ushort Layer { get; internal set; }
    /// <summary>Enemy.flashTimer at +$24: hurt-flash countdown consumed by enemy processing and palette selection; reaching the native threshold clears hurt-AI dispatch.</summary>
    public NativeCountdown FlashTimer { get; internal set; }
    /// <summary>Enemy.freezeTimer at +$26: remaining freeze-processing countdown; frozen palette blinking and thaw decisions read the same native word.</summary>
    public NativeCountdown FrozenTimer { get; internal set; }
    /// <summary>Enemy.invincibilityTimer at +$28: damage-interaction suppression countdown, sampled before its per-enemy decrement.</summary>
    public NativeCountdown InvincibilityTimer { get; internal set; }
    /// <summary>Enemy.shakeTimer at +$2A: remaining draw calls applying the enemy's one-pixel horizontal shake, decremented during drawing.</summary>
    public NativeCountdown ShakeTimer { get; internal set; }
    /// <summary>Enemy.frameCounter at +$2C: wrapping enemy-processing counter used by actor behavior and drawing, independent of the global NMI counter.</summary>
    public ushort FrameCounter { get; internal set; }
    /// <summary>Low byte of Enemy.bank at +$2E: bank used to resolve the current actor's code and animation pointers.</summary>
    public byte AiBank { get; internal set; }
    /// <summary>Definition-derived hurt duration byte retained beside AiBank, corresponding to the other byte of the native bank/hurt-time word.</summary>
    public byte HurtAiTime { get; internal set; }
    /// <summary>Enemy.init0 at +$3C: mutable first actor-specific parameter initialized from the room population record.</summary>
    public ushort Parameter1 { get; internal set; }
    /// <summary>Enemy.init1 at +$3E: mutable second actor-specific parameter initialized from the room population record.</summary>
    public ushort Parameter2 { get; internal set; }
    /// <summary>Enemy.var0 at +$30: first actor-owned working word; its timer, pointer, or arithmetic meaning depends on the active enemy AI.</summary>
    public ushort VariableA { get; internal set; }
    /// <summary>Enemy.var1 at +$32: second actor-owned working word, preserving native sixteen-bit wrapping across actor phases.</summary>
    public ushort VariableB { get; internal set; }
    /// <summary>Enemy.var2 at +$34: third actor-owned working word, whose units and lifetime belong to the selected enemy routine.</summary>
    public ushort VariableC { get; internal set; }
    /// <summary>Enemy.var3 at +$36: fourth actor-owned working word, retained across the actor's AI and instruction callbacks.</summary>
    public ushort VariableD { get; internal set; }
    /// <summary>Enemy.var4 at +$38: fifth actor-owned working word, interpreted by the selected enemy routine rather than a shared global schema.</summary>
    public ushort VariableE { get; internal set; }
    /// <summary>Enemy.var5 at +$3A: sixth actor-owned working word; some actors store their current function pointer here.</summary>
    public ushort VariableF { get; internal set; }
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
        Properties = ExtraProperties = Health = 0;
        AiStates = EnemyAiStates.None;
        InstructionTimer = NativeCountdown.Start(0);
        CurrentInstruction = 0;
        Timer = NativeCountdown.Start(0);
        SpritemapPointer = 0;
        FrozenTimer = NativeCountdown.Start(0);
        FlashTimer = NativeCountdown.Start(0);
        Layer = 0;
        VramTilesIndex = 0;
        PaletteIndex = 0;
        FrameCounter = 0;
        ShakeTimer = NativeCountdown.Start(0);
        InvincibilityTimer = NativeCountdown.Start(0);
        AiBank = HurtAiTime = 0;
        Parameter1 = Parameter2 = 0;
        VariableA = VariableB = VariableC = VariableD = VariableE = VariableF = 0;
        SpawnOffset = Offset16<RoomPixels>.Zero;
    }

    /// <summary>
    /// Reads the word at <paramref name="offset"/> of this slot's native 64-byte
    /// <c>EnemyData</c> record ($0F78 + slot*$40), for cartridge code that indexes enemy
    /// RAM with an unrelated register. Words whose native encoding the port does not hold
    /// verbatim are rejected rather than reconstructed.
    /// </summary>
    internal ushort ReadNativeWord(int offset) => (EnemyDataField)offset switch
    {
        EnemyDataField.Definition => (ushort)EnemyDefinitionPointer,
        EnemyDataField.XPosition => Position.X.Whole,
        EnemyDataField.XSubposition => Position.X.Fraction,
        EnemyDataField.YPosition => Position.Y.Whole,
        EnemyDataField.YSubposition => Position.Y.Fraction,
        EnemyDataField.XRadius => Radius.X,
        EnemyDataField.YRadius => Radius.Y,
        EnemyDataField.Properties => Properties,
        EnemyDataField.ExtraProperties => ExtraProperties,
        EnemyDataField.AiHandler => AiStates.NativeWord,
        EnemyDataField.Health => Health,
        EnemyDataField.Spritemap => SpritemapPointer,
        EnemyDataField.Timer => Timer.RawValue,
        EnemyDataField.Instruction => CurrentInstruction,
        EnemyDataField.InstructionTimer => InstructionTimer.RawValue,
        EnemyDataField.Palette => PaletteIndex,
        EnemyDataField.VramTiles => VramTilesIndex,
        EnemyDataField.Layer => Layer,
        EnemyDataField.FlashTimer => FlashTimer.RawValue,
        EnemyDataField.FrozenTimer => FrozenTimer.RawValue,
        EnemyDataField.InvincibilityTimer => InvincibilityTimer.RawValue,
        EnemyDataField.ShakeTimer => ShakeTimer.RawValue,
        EnemyDataField.FrameCounter => FrameCounter,
        EnemyDataField.VariableA => VariableA,
        EnemyDataField.VariableB => VariableB,
        EnemyDataField.VariableC => VariableC,
        EnemyDataField.VariableD => VariableD,
        EnemyDataField.VariableE => VariableE,
        EnemyDataField.VariableF => VariableF,
        EnemyDataField.Parameter1 => Parameter1,
        EnemyDataField.Parameter2 => Parameter2,
        _ => throw new InvalidOperationException(
            $"Enemy RAM word +${offset:X2} has no verbatim native projection."),
    };
}
