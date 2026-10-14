namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Named bank-$8B cinematic function entry points and bank-$8C background-object lists.</summary>
internal static class CinematicCodePointers
{
    /// <summary>Bit distinguishing an executable code pointer from a timed list record.</summary>
    public const ushort InstructionCommandBit = 0x8000;

    /// <summary><c>RTS_8B93D9</c>, the shared cinematic-sprite no-op callback.</summary>
    public const ushort CinematicSpriteObject_PreInstruction_NoOp = 0x93d9;

    /// <summary><c>IndirectInstructionFunction_DrawToBG2Tilemap</c> at $8B:88FD.</summary>
    public const ushort IndirectInstruction_DrawToPortraitTilemap = 0x88fd;

    /// <summary>Named bank-$8C background-object lists consumed by translated intro scenes.</summary>
    public static class BackgroundLists
    {
        /// <summary>$8C:D5DF, initial background-object list for Samus's blinking eye.</summary>
        public const ushort SamusBlinking = 0xd5df;
        /// <summary>$8C:D613, blinking-eye background list installed when the intro reaches page six.</summary>
        public const ushort SamusBlinkingPage6 = 0xd613;
    }

    /// <summary>Whether an indirect instruction's function is <see cref="IndirectInstruction_DrawToPortraitTilemap"/>.</summary>
    internal static bool IsDrawToPortraitTilemap(ushort function) => function == IndirectInstruction_DrawToPortraitTilemap;
}
