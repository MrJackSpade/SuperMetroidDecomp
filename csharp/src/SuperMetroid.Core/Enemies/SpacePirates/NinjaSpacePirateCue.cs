namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the ninja Space Pirates' programs, with the operands its native instruction carries.</summary>
internal abstract record NinjaSpacePirateCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private NinjaSpacePirateCue()
    {
    }

    /// <summary>Plays the operand's library-two sound (<c>Instruction_PirateNinja_QueueSoundInY_Lib2_Max6</c>, $B2:F546).</summary>
    /// <param name="Sound">For a sound cue: the library-two sound it plays.</param>
    internal sealed record PlaySound(SoundEffectId? Sound) : NinjaSpacePirateCue;

    /// <summary>Sets the function main AI runs next (<c>Instruction_PirateWall_FunctionInY</c>, $B2:EF83).</summary>
    /// <param name="Function">The cue's Function operand.</param>
    internal sealed record SetFunction(NinjaSpacePirateFunction Function) : NinjaSpacePirateCue;

    /// <summary>Sets the palette index to the operand (<c>Instruction_PirateNinja_PaletteIndexInY</c>, $B2:F536).</summary>
    /// <param name="Palette">For a palette cue: the sprite palette the Pirate is drawn with from then on.</param>
    internal sealed record SetPalette(SnesObjAttributeWord? Palette) : NinjaSpacePirateCue;

    /// <summary>Throws a claw in the operand's direction from the given offset (<c>Instruction_PirateNinja_SpawnClawProjWithThrowDirSpawnOffset</c>, $B2:F564).</summary>
    /// <param name="Throw">For a claw cue: the way the claw flies.</param>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    /// <param name="OffsetY">The cue's second, vertical signed pixel offset.</param>
    internal sealed record ThrowClaw(HorizontalDirection Throw, short Offset, short OffsetY) : NinjaSpacePirateCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(NinjaSpacePirateCueKind Kind) : NinjaSpacePirateCue;
}
