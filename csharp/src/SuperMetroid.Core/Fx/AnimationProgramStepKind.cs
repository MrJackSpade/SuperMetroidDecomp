namespace SuperMetroid.Core.Fx;

/// <summary>What one step of an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/> program does.</summary>
internal enum AnimationProgramStepKind
{
    /// <summary>Shows a frame for a number of processing updates; the program yields until it expires.</summary>
    Show,
    /// <summary>Publishes a gameplay cue; its handler may redirect to another program, which then starts at once.</summary>
    Cue,
    /// <summary>Keeps the last frame indefinitely: the program stays here and publishes nothing more.</summary>
    Hold,
    /// <summary>Continues at the start of another program, or restarts this one.</summary>
    GoTo,
    /// <summary>
    /// Runs on into the program authored after this one. It behaves like <see cref="GoTo"/>, but a cursor resting
    /// here already belongs to the next program, as the cartridge's instruction pointer does.
    /// </summary>
    FallInto,
    /// <summary>Keeps the current frame for a number of updates without showing a new one; the program yields.</summary>
    Wait,
    /// <summary>Loads the owner's loop counter with a repeat count and continues.</summary>
    SetCounter,
    /// <summary>Counts the loop counter down; while it is not zero, continues at the start of another program.</summary>
    RepeatWhileCounting,
    /// <summary>Ends the owner: the program stops here for good and its actor is deleted.</summary>
    Stop,
}
