namespace SuperMetroid.Core.Frontend;

/// <summary>Immutable bank-$8B ending-sprite constructor data.</summary>
/// <param name="Position">Origin in pixels as native sixteen-bit words; negative offscreen positions use two's-complement wrapping.</param>
/// <param name="Attributes">OBJ attribute word providing the palette bits passed to the cinematic sprite constructor.</param>
/// <param name="InstructionPointer">Bank-relative pointer into bank $8B's cinematic sprite instruction lists, rather than a spritemap address.</param>
public readonly record struct EndingSpriteDefinition(
    Position16<ScreenPixels> Position,
    SnesObjAttributeWord Attributes,
    ushort InstructionPointer);
