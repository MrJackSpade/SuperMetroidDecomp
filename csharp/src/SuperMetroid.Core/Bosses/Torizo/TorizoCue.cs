namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One cue of Torizo's body programs, with the operands its native instruction carries.</summary>
internal abstract record TorizoCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TorizoCue()
    {
    }

    /// <summary>A branch, call or link to another body program.</summary>
    /// <param name="Kind">What the branch tests or does.</param>
    /// <param name="Target">The program a branch, call or link cue continues at or saves.</param>
    internal sealed record Branch(TorizoBranchKind Kind, TorizoBodyProgram Target) : TorizoCue;

    /// <summary>A two-way branch or call between two body programs.</summary>
    /// <param name="Kind">What the choice tests.</param>
    /// <param name="Target">The program a branch, call or link cue continues at or saves.</param>
    /// <param name="Alternate">The program a two-way branch or call continues at otherwise.</param>
    internal sealed record Choice(TorizoChoiceKind Kind, TorizoBodyProgram Target, TorizoBodyProgram Alternate) : TorizoCue;

    /// <summary>Stands Bomb Torizo up or sits it down, moving it by a movement-table entry.</summary>
    /// <param name="Kind">Which way the posture changes.</param>
    /// <param name="MapOffset">The movement-table offset the posture change applies, from zero to 38.</param>
    internal sealed record Posture(TorizoPostureKind Kind, int MapOffset) : TorizoCue;

    /// <summary>One walking step of a Torizo, read from its walking table.</summary>
    /// <param name="Kind">Which Torizo walks.</param>
    /// <param name="TableOffset">The walking-table offset of the step, from zero to 38.</param>
    internal sealed record Walk(TorizoWalkKind Kind, int TableOffset) : TorizoCue;

    /// <summary>Spawns a Torizo projectile with a spawn parameter.</summary>
    /// <param name="Kind">What the cue spawns.</param>
    /// <param name="Parameter">The spawn parameter the native instruction passes.</param>
    internal sealed record ParameterizedSpawn(TorizoParameterizedSpawnKind Kind, int Parameter) : TorizoCue;

    /// <summary>Selects the main-AI function (<c>Instruction_Torizo_FunctionInY</c>, $AA:B09C).</summary>
    /// <param name="Function">The main-AI function a <see cref="SetFunction"/> cue selects.</param>
    internal sealed record SetFunction(TorizoFunction Function) : TorizoCue;

    /// <summary>Installs the body's pre-instruction (<c>Instruction_CommonAA_Enemy0FB2_InY</c>, $AA:806B).</summary>
    /// <param name="PreInstruction">The pre-instruction a <see cref="SetPreInstruction"/> cue installs.</param>
    internal sealed record SetPreInstruction(TorizoPreInstruction PreInstruction) : TorizoCue;

    /// <summary>Copies a tile transfer to VRAM (the common <c>$814B</c> transfer).</summary>
    /// <param name="Transfer">The <see cref="TorizoInstructionVramTransferDefinitions"/> entry a <see cref="CopyTiles"/> cue copies.</param>
    internal sealed record CopyTiles(int Transfer) : TorizoCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(TorizoCueKind Kind) : TorizoCue;
}
