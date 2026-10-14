namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's body programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Value">The cue's word operand: a movement-table index, spawn parameter or walking step its kind takes.</param>
/// <param name="Function">The main-AI function a <see cref="TorizoCueKind.SetFunction"/> cue selects.</param>
/// <param name="PreInstruction">The pre-instruction a <see cref="TorizoCueKind.SetPreInstruction"/> cue installs.</param>
/// <param name="Target">The program a branch, call or link cue continues at or saves.</param>
/// <param name="Alternate">The program a two-way branch or call continues at otherwise.</param>
/// <param name="Transfer">The <see cref="TorizoInstructionVramTransferDefinitions"/> entry a <see cref="TorizoCueKind.CopyTiles"/> cue copies.</param>
internal readonly record struct TorizoCue(
    TorizoCueKind Kind,
    ushort Value = 0,
    TorizoFunction Function = default,
    TorizoPreInstruction PreInstruction = default,
    TorizoBodyProgram Target = default,
    TorizoBodyProgram Alternate = default,
    int Transfer = 0);
