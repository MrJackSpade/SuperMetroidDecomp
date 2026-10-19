namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One cue of the ninja Space Pirates' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Sound">For a sound cue: the library-two sound it plays.</param>
/// <param name="Palette">For a palette cue: the sprite palette the Pirate is drawn with from then on.</param>
/// <param name="ThrowsRight">For a claw cue: whether the claw flies right rather than left.</param>
/// <param name="Offset">The cue's signed pixel offset.</param>
/// <param name="OffsetY">The cue's second, vertical signed pixel offset.</param>
/// <param name="Function">The cue's Function operand.</param>
internal readonly record struct NinjaSpacePirateCue(
    NinjaSpacePirateCueKind Kind,
    SoundEffectId? Sound = null,
    SnesObjAttributeWord? Palette = null,
    bool ThrowsRight = false,
    short Offset = 0,
    short OffsetY = 0,
    NinjaSpacePirateFunction Function = default);
