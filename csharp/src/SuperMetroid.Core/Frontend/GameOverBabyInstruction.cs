namespace SuperMetroid.Core.Frontend;

/// <summary>One compiled native animation record, independent of cartridge memory.</summary>
/// <param name="Pointer">Bank-$82 byte pointer identifying the start of this native frame record.</param>
/// <param name="Duration">Menu-update countdown assigned when this frame record is installed.</param>
/// <param name="Frame">Semantic pose selecting editable Baby OAM artwork.</param>
/// <param name="Palette">Semantic phase selecting an editable sixteen-color Baby palette.</param>
/// <param name="SoundAfter">Cry command queued when this record's countdown expires, or None when no command follows.</param>
/// <param name="NextPointer">Next valid frame-record pointer, skipping any intervening cry opcode and wrapping the final record to <see cref="GameOverBabyAnimationDefinitions.FirstPointer"/>.</param>
/// <param name="RestartAfter">Whether the native $FFFF restart marker follows this final record.</param>
public readonly record struct GameOverBabyInstruction(
    ushort Pointer,
    ushort Duration,
    GameOverBabyFrame Frame,
    GameOverBabyPalette Palette,
    GameOverBabySound SoundAfter,
    ushort NextPointer,
    bool RestartAfter);
