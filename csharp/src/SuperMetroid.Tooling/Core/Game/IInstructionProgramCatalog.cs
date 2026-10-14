namespace SuperMetroid.Core.Game;

/// <summary>An instruction-program catalog whose compiled mechanics words can be enumerated.</summary>
internal interface IInstructionProgramCatalog
{
    /// <summary>Number of timing and control words owned by the compiled instruction program.</summary>
    static abstract int MechanicsWordCount { get; }

    /// <summary>Gets the compiled word at the given mechanics index for audit enumeration.</summary>
    static abstract InstructionMechanicsWord MechanicsWord(int index);
}
