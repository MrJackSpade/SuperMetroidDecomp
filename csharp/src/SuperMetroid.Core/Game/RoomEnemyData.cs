namespace SuperMetroid.Core.Game;

/// <summary>The complete parsed 64-byte bank-$A0 enemy definition.</summary>
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
public readonly record struct RoomEnemyGraphicsSetEntry(
    ushort DefinitionPointer,
    ushort VramDestination,
    ushort VramTilesIndex,
    RoomEnemyDefinition Definition,
    int StagingOffset,
    int TileByteCount);

/// <summary>Immutable spawn words retained beside the mutable native enemy slot.</summary>
public readonly record struct RoomEnemySpawnSnapshot(
    RoomEnemyPopulationRecord Population,
    ushort XRadius,
    ushort YRadius,
    ushort Health,
    byte Layer,
    ushort VramTilesIndex,
    ushort PaletteIndex,
    RoomEnemySpawnNameWords NameWords);

/// <summary>
/// The six words copied by <c>RecordEnemySpawnData</c> from the bank-$B4 enemy-name
/// record. The retail routine intentionally skips source word five and stores word six in
/// its place; explicit field names preserve that oddity instead of presenting a false
/// contiguous string abstraction.
/// </summary>
public readonly record struct RoomEnemySpawnNameWords(
    ushort Word0,
    ushort Word1,
    ushort Word2,
    ushort Word3,
    ushort Word4,
    ushort Word6);

/// <summary>
/// Mutable projection of the 64-byte WRAM <c>EnemyData</c> record. Named properties retain
/// their native word widths even where current translated actors only consume a low byte.
/// </summary>
public sealed class RoomEnemySlot
{
    internal RoomEnemySlot(int slotIndex)
    {
        SlotIndex = slotIndex;
        NativeIndex = checked((ushort)(slotIndex * RoomEnemySystem.NativeSlotSize));
    }

    public int SlotIndex { get; }
    public ushort NativeIndex { get; }
    public ushort EnemyDefinitionPointer { get; internal set; }
    public RoomEnemyDefinition Definition { get; internal set; }
    public RoomEnemySpawnSnapshot Spawn { get; internal set; }
    public ushort XPosition { get; internal set; }
    public ushort XSubposition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort YSubposition { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }
    public ushort Properties { get; internal set; }
    public ushort ExtraProperties { get; internal set; }
    public ushort AiHandlerBits { get; internal set; }
    public ushort Health { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }
    public ushort Timer { get; internal set; }
    public ushort CurrentInstruction { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort PaletteIndex { get; internal set; }
    public ushort VramTilesIndex { get; internal set; }
    public ushort Layer { get; internal set; }
    public ushort FlashTimer { get; internal set; }
    public ushort FrozenTimer { get; internal set; }
    public ushort InvincibilityTimer { get; internal set; }
    public ushort ShakeTimer { get; internal set; }
    public ushort FrameCounter { get; internal set; }
    public byte AiBank { get; internal set; }
    public byte HurtAiTime { get; internal set; }
    public ushort Parameter1 { get; internal set; }
    public ushort Parameter2 { get; internal set; }
    public ushort VariableA { get; internal set; }
    public ushort VariableB { get; internal set; }
    public ushort VariableC { get; internal set; }
    public ushort VariableD { get; internal set; }
    public ushort VariableE { get; internal set; }
    public ushort VariableF { get; internal set; }
    public ushort SpawnXOffset { get; internal set; }
    public ushort SpawnYOffset { get; internal set; }

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
    None,
    LandingPadOpened,
    LandingPadClosed,
    LandingCompleted,
    EntryStarted,
    EntryPadClosing,
    SavePromptRequested,
    SavePromptAnswered,
    ExitPadClosing,
    ExitCompleted,
    EscapeTakeoffStarted,
    EscapeTakeoffCompleted,
}

/// <summary>
/// Native <c>loading_game_state</c> branch sampled while the Landing Site gunship is
/// initialized. These are mutually exclusive loader scenarios, not combinable flags.
/// </summary>
public enum GunshipLoadScenario
{
    Ordinary,
    EscapingCeres,
}
