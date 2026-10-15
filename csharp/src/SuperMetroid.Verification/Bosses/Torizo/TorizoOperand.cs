namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>What one operand word of a native Torizo instruction carries.</summary>
internal enum TorizoOperand
{
    /// <summary>A movement index, spawn parameter or walking step: the cue's value.</summary>
    Value,
    /// <summary>A main-AI function.</summary>
    Function,
    /// <summary>A pre-instruction.</summary>
    PreInstruction,
    /// <summary>A list pointer: the cue's target program.</summary>
    Target,
    /// <summary>A second list pointer: the cue's alternate program.</summary>
    Alternate,
}
