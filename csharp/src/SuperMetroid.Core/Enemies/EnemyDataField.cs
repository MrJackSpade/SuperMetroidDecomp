namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The word fields of a native 64-byte <c>EnemyData</c> record ($0F78 + slot*$40), by byte offset. Cartridge code that
/// indexes enemy RAM with an unrelated register reads these; the port projects them from the slot's typed state.
/// </summary>
internal enum EnemyDataField
{
    /// <summary>+$00: the enemy definition (header) pointer.</summary>
    Definition = 0x00,
    /// <summary>+$02: X position, whole pixels.</summary>
    XPosition = 0x02,
    /// <summary>+$04: X position fraction.</summary>
    XSubposition = 0x04,
    /// <summary>+$06: Y position, whole pixels.</summary>
    YPosition = 0x06,
    /// <summary>+$08: Y position fraction.</summary>
    YSubposition = 0x08,
    /// <summary>+$0A: X radius.</summary>
    XRadius = 0x0a,
    /// <summary>+$0C: Y radius.</summary>
    YRadius = 0x0c,
    /// <summary>+$0E: properties.</summary>
    Properties = 0x0e,
    /// <summary>+$10: extra properties.</summary>
    ExtraProperties = 0x10,
    /// <summary>+$12: AI-handler bits.</summary>
    AiHandler = 0x12,
    /// <summary>+$14: health.</summary>
    Health = 0x14,
    /// <summary>+$16: spritemap pointer.</summary>
    Spritemap = 0x16,
    /// <summary>+$18: general timer.</summary>
    Timer = 0x18,
    /// <summary>+$1A: instruction list pointer.</summary>
    Instruction = 0x1a,
    /// <summary>+$1C: instruction timer.</summary>
    InstructionTimer = 0x1c,
    /// <summary>+$1E: palette index.</summary>
    Palette = 0x1e,
    /// <summary>+$20: VRAM tiles index.</summary>
    VramTiles = 0x20,
    /// <summary>+$22: layer.</summary>
    Layer = 0x22,
    /// <summary>+$24: flash timer.</summary>
    FlashTimer = 0x24,
    /// <summary>+$26: frozen timer.</summary>
    FrozenTimer = 0x26,
    /// <summary>+$28: invincibility timer.</summary>
    InvincibilityTimer = 0x28,
    /// <summary>+$2A: shake timer.</summary>
    ShakeTimer = 0x2a,
    /// <summary>+$2C: frame counter.</summary>
    FrameCounter = 0x2c,
    /// <summary>+$30: AI variable A.</summary>
    VariableA = 0x30,
    /// <summary>+$32: AI variable B.</summary>
    VariableB = 0x32,
    /// <summary>+$34: AI variable C.</summary>
    VariableC = 0x34,
    /// <summary>+$36: AI variable D.</summary>
    VariableD = 0x36,
    /// <summary>+$38: AI variable E.</summary>
    VariableE = 0x38,
    /// <summary>+$3A: AI variable F.</summary>
    VariableF = 0x3a,
    /// <summary>+$3C: population parameter 1.</summary>
    Parameter1 = 0x3c,
    /// <summary>+$3E: population parameter 2.</summary>
    Parameter2 = 0x3e,
}
