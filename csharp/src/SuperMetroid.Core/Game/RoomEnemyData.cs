namespace SuperMetroid.Core.Game;

/// <summary>The complete parsed 64-byte bank-$A0 enemy definition.</summary>
/// <param name="TileDataSize">Header +$00 byte-count word; bit 15 selects explicit tile placement, while the low fifteen bits give the upload size.</param>
/// <param name="PalettePointer">Header +$02 bank-relative pointer to the sixteen-color palette in the enemy's code/data bank.</param>
/// <param name="Health">Header +$04 initial health in enemy damage units.</param>
/// <param name="Damage">Header +$06 contact-damage value before Samus damage-reduction rules.</param>
/// <param name="XRadius">Header +$08 horizontal collision half-extent in room pixels.</param>
/// <param name="YRadius">Header +$0A vertical collision half-extent in room pixels.</param>
/// <param name="Bank">Header +$0C bank used to resolve the enemy's AI, palette, and animation pointers.</param>
/// <param name="HurtAiTime">Header +$0D native hurt-AI duration byte, retained separately from mutable flash timers.</param>
/// <param name="HurtSoundEffect">Header +$0E native enemy cry sound selector.</param>
/// <param name="BossId">Header +$10 boss identity published when a room population contains this enemy; zero means no boss identity.</param>
/// <param name="InitializationAiPointer">Header +$12 bank-relative initialization AI entry point, resolved through <paramref name="Bank"/>.</param>
/// <param name="PartCount">Header +$14 native multipart-enemy count.</param>
/// <param name="Unused16">Header +$16 unused native word, retained verbatim rather than interpreted as runtime state.</param>
/// <param name="MainAiPointer">Header +$18 bank-relative ordinary AI entry point.</param>
/// <param name="GrappleAiPointer">Header +$1A bank-relative Grapple reaction entry point.</param>
/// <param name="HurtAiPointer">Header +$1C bank-relative hurt AI entry point.</param>
/// <param name="FrozenAiPointer">Header +$1E bank-relative frozen-enemy AI entry point, separate from globally frozen game time.</param>
/// <param name="TimeFrozenAiPointer">Header +$20 bank-relative AI entry point used when game time is frozen.</param>
/// <param name="DeathAnimation">Header +$22 native death-animation selector, rather than an AI address.</param>
/// <param name="Unused24">Header +$24 first word of an unused four-byte native field, retained verbatim.</param>
/// <param name="Unused26">Header +$26 second word of the unused field beginning at +$24.</param>
/// <param name="PowerBombReactionPointer">Header +$28 bank-relative Power Bomb reaction entry point.</param>
/// <param name="VariantIndex">Header +$2A Sidehopper variant selector; other headers retain unused prototype instruction-list data in this word.</param>
/// <param name="Unused2C">Header +$2C first word of an unused four-byte native field, retained verbatim.</param>
/// <param name="Unused2E">Header +$2E second word of the unused field beginning at +$2C.</param>
/// <param name="TouchAiPointer">Header +$30 bank-relative Samus-contact reaction entry point.</param>
/// <param name="ShotAiPointer">Header +$32 bank-relative projectile-hit reaction entry point.</param>
/// <param name="InitialSpritemapPointer">Header +$34 legacy-named unused spritemap-pointer-table field; it is not a universal initial drawing frame.</param>
/// <param name="TileDataAddress">Header +$36 full 24-bit source address of the enemy's planar character data.</param>
/// <param name="Layer">Header +$39 native drawing-layer byte, copied into the slot's word-sized layer field.</param>
/// <param name="ItemDropChancesPointer">Header +$3A bank-$B4 pointer to enemy pickup-drop probabilities.</param>
/// <param name="VulnerabilityPointer">Header +$3C bank-$B4 pointer to weapon-vulnerability data.</param>
/// <param name="NamePointer">Header +$3E bank-$B4 pointer to the debug enemy-name record.</param>
public readonly record struct RoomEnemyDefinition(
    ushort TileDataSize,
    ushort PalettePointer,
    ushort Health,
    ushort Damage,
    ushort XRadius,
    ushort YRadius,
    byte Bank,
    byte HurtAiTime,
    ushort HurtSoundEffect,
    ushort BossId,
    ushort InitializationAiPointer,
    ushort PartCount,
    ushort Unused16,
    ushort MainAiPointer,
    ushort GrappleAiPointer,
    ushort HurtAiPointer,
    ushort FrozenAiPointer,
    ushort TimeFrozenAiPointer,
    ushort DeathAnimation,
    ushort Unused24,
    ushort Unused26,
    ushort PowerBombReactionPointer,
    ushort VariantIndex,
    ushort Unused2C,
    ushort Unused2E,
    ushort TouchAiPointer,
    ushort ShotAiPointer,
    ushort InitialSpritemapPointer,
    int TileDataAddress,
    byte Layer,
    ushort ItemDropChancesPointer,
    ushort VulnerabilityPointer,
    ushort NamePointer);

/// <summary>One literal 16-byte bank-$A1 room-population record.</summary>
/// <param name="DefinitionPointer">Record +$00 bank-$A0 enemy header pointer; population list terminator $FFFF is not a spawn record.</param>
/// <param name="XPosition">Record +$02 initial horizontal room position in pixels, retaining its native sixteen-bit representation.</param>
/// <param name="YPosition">Record +$04 initial vertical room position in pixels, retaining its native sixteen-bit representation.</param>
/// <param name="InitializationParameter">Record +$06 actor-specific initialization word, initially copied to the slot's instruction-list field.</param>
/// <param name="Properties">Record +$08 primary native enemy properties governing interaction, processing, and visibility.</param>
/// <param name="ExtraProperties">Record +$0A additional native properties, including extended-spritemap selection.</param>
/// <param name="Parameter1">Record +$0C first actor-specific spawn parameter; units and meaning belong to the selected enemy AI.</param>
/// <param name="Parameter2">Record +$0E second actor-specific spawn parameter; units and meaning belong to the selected enemy AI.</param>
public readonly record struct RoomEnemyPopulationRecord(
    ushort DefinitionPointer,
    ushort XPosition,
    ushort YPosition,
    ushort InitializationParameter,
    ushort Properties,
    ushort ExtraProperties,
    ushort Parameter1,
    ushort Parameter2);

/// <summary>One literal four-byte bank-$B4 room graphics-set record plus resolved data.</summary>
/// <param name="DefinitionPointer">Record +$00 bank-$A0 enemy header identity used to resolve character and palette artwork.</param>
/// <param name="VramDestination">Record +$02 packed palette/placement word, not a direct VRAM address: its low byte selects the OBJ palette and high bits participate in explicit tile placement.</param>
/// <param name="VramTilesIndex">Accumulated character index used as the enemy's sprite tile offset, advancing by each raw header size divided by 32.</param>
/// <param name="Definition">Resolved immutable header corresponding to the record's enemy identity.</param>
/// <param name="StagingOffset">Byte offset from the enemy VRAM byte base, selected sequentially or from the explicit placement bits.</param>
/// <param name="TileByteCount">Actual planar character upload size in bytes, with the header size word's placement bit removed.</param>
public readonly record struct RoomEnemyGraphicsSetEntry(
    ushort DefinitionPointer,
    ushort VramDestination,
    ushort VramTilesIndex,
    RoomEnemyDefinition Definition,
    int StagingOffset,
    int TileByteCount);

/// <summary>Immutable spawn words retained beside the mutable native enemy slot.</summary>
/// <param name="Population">Original sixteen-byte population values, preserved when the live enemy position, properties, or parameters change.</param>
/// <param name="VramTilesIndex">Graphics-set character offset assigned at initialization and retained independently of the live slot.</param>
public readonly record struct RoomEnemySpawnSnapshot(
    RoomEnemyPopulationRecord Population,
    ushort VramTilesIndex);

/// <summary>
/// Mutable projection of the 64-byte WRAM <c>EnemyData</c> record. Named properties retain
/// their native word widths even where current translated actors only consume a low byte.
/// </summary>
public sealed class RoomEnemySlot
{
    /// <summary>Creates a stable slot identity and computes its byte offset within the native EnemyData array.</summary>
    /// <param name="slotIndex">Zero-based enemy slot number used to derive the native $40-byte record offset.</param>
    internal RoomEnemySlot(int slotIndex)
    {
        SlotIndex = slotIndex;
        NativeIndex = checked((ushort)(slotIndex * RoomEnemySystem.NativeSlotSize));
    }

    /// <summary>Zero-based managed slot number, fixed for the slot object's lifetime; room populations occupy at most 32 native slots.</summary>
    public int SlotIndex { get; }
    /// <summary>Native byte index into EnemyData, equal to <see cref="SlotIndex"/> times $40, rather than a WRAM address.</summary>
    public ushort NativeIndex { get; }
    /// <summary>Enemy.ID at slot +$00: bank-$A0 header identity; zero denotes a cleared/inactive slot.</summary>
    public ushort EnemyDefinitionPointer { get; internal set; }
    /// <summary>Immutable resolved header for the current enemy identity, supplying initialization, collision, and AI definitions.</summary>
    public RoomEnemyDefinition Definition { get; internal set; }
    /// <summary>Original population and character-offset values retained independently of mutable live slot fields.</summary>
    public RoomEnemySpawnSnapshot Spawn { get; internal set; }
    /// <summary>Enemy.XPosition at +$02: whole-pixel horizontal room position, wrapping at sixteen bits.</summary>
    public ushort XPosition { get; internal set; }
    /// <summary>Enemy.XSubPosition at +$04: low sixteen-bit fraction paired with XPosition for native 16.16 motion.</summary>
    public ushort XSubposition { get; internal set; }
    /// <summary>Enemy.YPosition at +$06: whole-pixel vertical room position, wrapping at sixteen bits.</summary>
    public ushort YPosition { get; internal set; }
    /// <summary>Enemy.YSubPosition at +$08: low sixteen-bit fraction paired with YPosition for native 16.16 motion.</summary>
    public ushort YSubposition { get; internal set; }
    /// <summary>Enemy.XHitboxRadius at +$0A: horizontal collision half-extent in pixels, initially copied from the header.</summary>
    public ushort XRadius { get; internal set; }
    /// <summary>Enemy.YHitboxRadius at +$0C: vertical collision half-extent in pixels, initially copied from the header.</summary>
    public ushort YRadius { get; internal set; }
    /// <summary>Enemy.properties at +$0E: mutable primary native property bits controlling collision, drawing, and instruction processing.</summary>
    public ushort Properties { get; internal set; }
    /// <summary>Enemy.properties2 at +$10: mutable additional properties, including extended-spritemap selection.</summary>
    public ushort ExtraProperties { get; internal set; }
    /// <summary>Enemy.AI at +$12: native handler-selection bits for ordinary, hurt, frozen, and other AI dispatch states.</summary>
    public ushort AiHandlerBits { get; internal set; }
    /// <summary>Enemy.health at +$14: mutable remaining enemy health, initialized from the definition and reduced by weapon damage.</summary>
    public ushort Health { get; internal set; }
    /// <summary>Enemy.spritemap at +$16: current bank-relative drawing frame or extended-map identity selected by the actor.</summary>
    public ushort SpritemapPointer { get; internal set; }
    /// <summary>Enemy.loopCounter at +$18: actor/instruction-owned loop or delay word; it is not a universal elapsed-frame counter.</summary>
    public ushort Timer { get; internal set; }
    /// <summary>Enemy.instList at +$1A: current bank-relative animation instruction pointer, initially populated from the actor-specific initialization parameter.</summary>
    public ushort CurrentInstruction { get; internal set; }
    /// <summary>Enemy.instTimer at +$1C: animation instruction countdown in processing updates; initialization starts it at one.</summary>
    public ushort InstructionTimer { get; internal set; }
    /// <summary>Enemy.palette at +$1E: native OBJ palette attribute bits, normally the graphics-set palette index shifted left nine, rather than a CGRAM color index.</summary>
    public ushort PaletteIndex { get; internal set; }
    /// <summary>Enemy.GFXOffset at +$20: sprite character-index offset assigned by the graphics set, in 32-byte planar tile units.</summary>
    public ushort VramTilesIndex { get; internal set; }
    /// <summary>Enemy.layer at +$22: drawing-queue layer word, initialized from the definition's byte-sized layer.</summary>
    public ushort Layer { get; internal set; }
    /// <summary>Enemy.flashTimer at +$24: hurt-flash countdown consumed by enemy processing and palette selection; reaching the native threshold clears hurt-AI dispatch.</summary>
    public ushort FlashTimer { get; internal set; }
    /// <summary>Enemy.freezeTimer at +$26: remaining freeze-processing countdown; frozen palette blinking and thaw decisions read the same native word.</summary>
    public ushort FrozenTimer { get; internal set; }
    /// <summary>Enemy.invincibilityTimer at +$28: damage-interaction suppression countdown, sampled before its per-enemy decrement.</summary>
    public ushort InvincibilityTimer { get; internal set; }
    /// <summary>Enemy.shakeTimer at +$2A: remaining draw calls applying the enemy's one-pixel horizontal shake, decremented during drawing.</summary>
    public ushort ShakeTimer { get; internal set; }
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
    /// <summary>Native spawn-data graphical X offset, added to live XPosition before camera subtraction; wrapped words may represent negative pixel offsets.</summary>
    public ushort SpawnXOffset { get; internal set; }
    /// <summary>Native spawn-data graphical Y offset, added to live YPosition before camera subtraction; wrapped words may represent negative pixel offsets.</summary>
    public ushort SpawnYOffset { get; internal set; }

    /// <summary>Resets all mutable enemy-record values while retaining this slot object's fixed index and native offset.</summary>
    internal void Clear()
    {
        EnemyDefinitionPointer = 0;
        Definition = default;
        Spawn = default;
        XPosition = XSubposition = YPosition = YSubposition = 0;
        XRadius = YRadius = Properties = ExtraProperties = AiHandlerBits = Health = 0;
        SpritemapPointer = Timer = CurrentInstruction = InstructionTimer = 0;
        PaletteIndex = VramTilesIndex = Layer = FlashTimer = FrozenTimer = 0;
        InvincibilityTimer = ShakeTimer = FrameCounter = 0;
        AiBank = HurtAiTime = 0;
        Parameter1 = Parameter2 = 0;
        VariableA = VariableB = VariableC = VariableD = VariableE = VariableF = 0;
        SpawnXOffset = SpawnYOffset = 0;
    }

    /// <summary>
    /// Reads the word at <paramref name="offset"/> of this slot's native 64-byte
    /// <c>EnemyData</c> record ($0F78 + slot*$40), for cartridge code that indexes enemy
    /// RAM with an unrelated register. Words whose native encoding the port does not hold
    /// verbatim are rejected rather than reconstructed.
    /// </summary>
    internal ushort ReadNativeWord(int offset) => offset switch
    {
        0x00 => EnemyDefinitionPointer,
        0x02 => XPosition,
        0x04 => XSubposition,
        0x06 => YPosition,
        0x08 => YSubposition,
        0x0a => XRadius,
        0x0c => YRadius,
        0x0e => Properties,
        0x10 => ExtraProperties,
        0x12 => AiHandlerBits,
        0x14 => Health,
        0x16 => SpritemapPointer,
        0x18 => Timer,
        0x1a => CurrentInstruction,
        0x1c => InstructionTimer,
        0x1e => PaletteIndex,
        0x20 => VramTilesIndex,
        0x22 => Layer,
        0x24 => FlashTimer,
        0x26 => FrozenTimer,
        0x28 => InvincibilityTimer,
        0x2a => ShakeTimer,
        0x2c => FrameCounter,
        0x30 => VariableA,
        0x32 => VariableB,
        0x34 => VariableC,
        0x36 => VariableD,
        0x38 => VariableE,
        0x3a => VariableF,
        0x3c => Parameter1,
        0x3e => Parameter2,
        _ => throw new InvalidOperationException(
            $"Enemy RAM word +${offset:X2} has no verbatim native projection."),
    };
}

/// <summary>Cross-system gunship transitions produced during the most recent enemy frame.</summary>
public enum GunshipFrameEvent
{
    /// <summary>No gunship transition has been published for the current enemy pass.</summary>
    None,
    /// <summary>Post-Ceres landing has started the entrance-pad opening animation before Samus is ejected.</summary>
    LandingPadOpened,
    /// <summary>Post-Ceres ejection has reached its target and started the entrance-pad closing animation.</summary>
    LandingPadClosed,
    /// <summary>Post-Ceres landing and ejection have completed, the gunship is idle, and Samus input is unlocked.</summary>
    LandingCompleted,
    /// <summary>Idle-gunship entry was accepted, locking Samus input and starting the entrance-pad opening animation.</summary>
    EntryStarted,
    /// <summary>Samus has been lowered inside the ship and the entrance-pad closing animation has started.</summary>
    EntryPadClosing,
    /// <summary>Ordinary restoration has finished and the gunship requests the save-confirmation prompt.</summary>
    SavePromptRequested,
    /// <summary>The save prompt has returned an answer; both answers start the same pad-opening and exit sequence.</summary>
    SavePromptAnswered,
    /// <summary>Samus has risen to the exit target and the pad begins closing before input is restored.</summary>
    ExitPadClosing,
    /// <summary>Ordinary gunship exit has completed, restoring idle gunship behavior and unlocking Samus input.</summary>
    ExitCompleted,
    /// <summary>The active Zebes timebomb bypasses restoration and saving, starting escape takeoff with Samus attached inside.</summary>
    EscapeTakeoffStarted,
    /// <summary>Gunship escape takeoff has reached its completion boundary, allowing the outer game to begin the ending sequence.</summary>
    EscapeTakeoffCompleted,
}

/// <summary>
/// Native <c>loading_game_state</c> branch sampled while the Landing Site gunship is
/// initialized. These are mutually exclusive loader scenarios, not combinable flags.
/// </summary>
public enum GunshipLoadScenario
{
    /// <summary>Ordinary room loading initializes the Landing Site gunship in its normal parked scenario.</summary>
    Ordinary,
    /// <summary>The Ceres-escape loader branch initializes the gunship descent, landing, and Samus-ejection sequence.</summary>
    EscapingCeres,
}
