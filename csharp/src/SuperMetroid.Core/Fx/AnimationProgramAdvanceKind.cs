namespace SuperMetroid.Core.Fx;

/// <summary>How an advance of an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/> program ended.</summary>
internal enum AnimationProgramAdvanceKind
{
    /// <summary>A new frame is shown for <see cref="AnimationProgramAdvance{TFrame}.Duration"/> updates.</summary>
    Show,
    /// <summary>The current frame stays for <see cref="AnimationProgramAdvance{TFrame}.Duration"/> more updates.</summary>
    Wait,
    /// <summary>The program holds its last frame for good.</summary>
    Hold,
    /// <summary>The program stopped; its owner deletes the actor.</summary>
    Stop,
}
